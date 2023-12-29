using Quesadilla.Models.Events;
using Quesadilla.Models.Logfile;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quesadilla.Models
{
    public class LogWatcher
    {

        private ILogger logger = Log.ForContext<LogWatcher>();

        private string _logPath;
        private string _systemChannelPrefix;
        private Encoding _fileEncoding;
        private LogFileType _logFileType;
        private List<LogEntry> _previousLogEntries = new List<LogEntry>();
        private Dictionary<string, long> _fileSizes = new Dictionary<string, long>();
        private Dictionary<string, InterestingFile> _interestingFiles = new Dictionary<string, InterestingFile>();
        private readonly Regex _chatLogRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s(?<time>\\d{2}\\:\\d{2}\\:\\d{2})\\s\\]\\s(?<name>\\w.*)\\s>\\s(?<content>.*)", RegexOptions.Compiled);
        private readonly Regex _gameLogRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s(?<time>\\d{2}\\:\\d{2}\\:\\d{2})\\s\\]\\s\\(\\w.*\\)\\s(?<content>.*)", RegexOptions.Compiled);
        private readonly Regex _gameLogCombatRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s\\d{2}\\:\\d{2}\\:\\d{2}\\s\\]\\s\\(combat\\)", RegexOptions.Compiled);
        private readonly Regex _localListener = new Regex("Listener\\:\\s*(?<name>.*)", RegexOptions.Compiled);

        public event EventHandler<ProcessNewDataEventArgs> ProcessNewData;
        public event EventHandler<ProcessCombatEventArgs> ProcessCombat;

        public string SystemChannelPrefix => this._systemChannelPrefix;
        public static string RootLogsPath { get; private set; }
        public LocalInfo InitialLocalInfo { get; set; }
        public LogWatcher(string systemChannelPrefix, LogFileType logFileType, string logPath = "")
        {
            this._systemChannelPrefix = systemChannelPrefix;
            this._logFileType = logFileType;
            if (LogWatcher.RootLogsPath == null)
                LogWatcher.RootLogsPath = logPath.Length != 0 ? logPath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "EVE\\logs");
            if (this._logFileType == LogFileType.Game)
            {
                this._fileEncoding = Encoding.ASCII;
                this._logPath = LogWatcher.RootLogsPath + "\\Gamelogs";
            }
            else
            {
                this._fileEncoding = Encoding.Unicode;
                this._logPath = LogWatcher.RootLogsPath + "\\Chatlogs";
            }
        }

        public void Tick()
        {
            foreach (FileInfo changedLogFile in this.GetChangedLogFiles())
            {
                if (!this._interestingFiles.ContainsKey(changedLogFile.FullName))
                {
                    logger.Information(changedLogFile.FullName);

                    long fileLength = this.GetFileLength(changedLogFile.FullName);
                    string logListener = this.GetLogListener(changedLogFile.FullName);
                    this._interestingFiles.Add(changedLogFile.FullName, new InterestingFile(changedLogFile.FullName, fileLength, DateTime.Now, logListener));
                    this.OnProcessNewData(new LogEntry()
                    {
                        FileName = changedLogFile.Name,
                        EntryType = this._logFileType == LogFileType.Game ? LogEntryType.NewGameLogEvent : LogEntryType.NewChatLogEvent,
                        LineContent = fileLength.ToString(),
                        LogType = this._logFileType,
                        CharacterName = logListener
                    });
                }
            }
            if (this._logFileType == LogFileType.Game)
            {
                foreach (KeyValuePair<string, InterestingFile> keyValuePair in this._interestingFiles.Where<KeyValuePair<string, InterestingFile>>((Func<KeyValuePair<string, InterestingFile>, bool>)(interestingFile => interestingFile.Value.InCombat && !interestingFile.Value.TimeOutTriggered && interestingFile.Value.LastCombat < DateTime.Now.AddSeconds(-30.0))))
                {
                    keyValuePair.Value.TimeOutTriggered = true;
                    keyValuePair.Value.InCombat = false;
                    this.OnCombat(keyValuePair.Value.FileName, keyValuePair.Value.CharName, CombatEventType.Stop);
                }
            }
            foreach (string key in this._interestingFiles.Where<KeyValuePair<string, InterestingFile>>((Func<KeyValuePair<string, InterestingFile>, bool>)(interestingFile => interestingFile.Value.LastUpdate < DateTime.Now.AddMinutes(-120.0))).Select<KeyValuePair<string, InterestingFile>, string>((Func<KeyValuePair<string, InterestingFile>, string>)(interestingFile => interestingFile.Key)).ToList<string>())
            {
                lock (this)
                    this._interestingFiles.Remove(key);
            }
            List<LogEntry> logEntryList = new List<LogEntry>();
            lock (this)
            {
                foreach (KeyValuePair<string, InterestingFile> interestingFile in this._interestingFiles)
                {
                    FileInfo fileInfo = new FileInfo(interestingFile.Key);
                    if (this.GetFileLength(fileInfo.FullName) != interestingFile.Value.LastPosition)
                    {
                        NewData newData;
                        lock (this)
                        {
                            newData = this.ReadLogFile(interestingFile.Key, interestingFile.Value.LastPosition);
                            this._interestingFiles[interestingFile.Key].LastPosition = newData.FileLength;
                            this._interestingFiles[interestingFile.Key].LastUpdate = DateTime.Now;
                        }
                        string[] array = ((IEnumerable<string>)newData.Data.Split(new string[1]
                        {
                "\n"
                        }, StringSplitOptions.RemoveEmptyEntries)).Select<string, string>((Func<string, string>)(line => line.Replace(Convert.ToChar(65279).ToString(), "").Replace(Convert.ToChar(65534).ToString(), "").Replace("\r", "").Trim())).Where<string>((Func<string, bool>)(line => line.Length > 0)).ToArray<string>();
                        if (array.Length != 0)
                        {
                            foreach (string input in array)
                            {
                                LogEntry tempLogEntry = new LogEntry();
                                Match match = this._logFileType == LogFileType.Chat ? this._chatLogRegex.Match(input) : this._gameLogRegex.Match(input);
                                tempLogEntry.RawLine = input;
                                tempLogEntry.FileName = fileInfo.Name;
                                tempLogEntry.LogPrefix = this._systemChannelPrefix;
                                tempLogEntry.TimeAdded = DateTime.Now;
                                tempLogEntry.LogType = this._logFileType;
                                tempLogEntry.CharacterName = interestingFile.Value.CharName;
                                if (this._logFileType == LogFileType.Game && this._gameLogCombatRegex.IsMatch(input))
                                {
                                    if (!this._interestingFiles[interestingFile.Key].InCombat)
                                    {
                                        this._interestingFiles[interestingFile.Key].InCombat = true;
                                        this.OnCombat(interestingFile.Value.FileName, interestingFile.Value.CharName, CombatEventType.Start);
                                    }
                                    this._interestingFiles[interestingFile.Key].TimeOutTriggered = false;
                                    this._interestingFiles[interestingFile.Key].LastCombat = this._interestingFiles[interestingFile.Key].LastUpdate;
                                }
                                if (match.Success)
                                {
                                    tempLogEntry.LogTime = match.Groups["time"].ToString();
                                    tempLogEntry.LineContent = match.Groups["content"].ToString();
                                    tempLogEntry.EntryType = LogEntryType.ChatEvent;
                                    tempLogEntry.ParseSuccess = true;
                                    if (this._logFileType == LogFileType.Chat)
                                        tempLogEntry.PlayerName = match.Groups["name"].ToString();
                                }
                                else
                                {
                                    tempLogEntry.LineContent = input;
                                    tempLogEntry.ParseSuccess = false;
                                    tempLogEntry.EntryType = this._logFileType == LogFileType.Chat ? LogEntryType.UnknownChatLogEvent : LogEntryType.UnknownGameLogEvent;
                                }
                                if (this._previousLogEntries.All<LogEntry>((Func<LogEntry, bool>)(logEntry => logEntry.LineContent != tempLogEntry.LineContent)))
                                {
                                    this._previousLogEntries.Add(tempLogEntry);
                                    logEntryList.Add(tempLogEntry);
                                }
                            }
                        }
                    }
                }
            }
            if (logEntryList.Count <= 0)
                return;
            foreach (LogEntry entry in logEntryList)
                this.OnProcessNewData(entry);
        }
        private NewData ReadLogFile(string fileName, long startingPosition)
        {
            NewData newData = new NewData();
            using (StreamReader streamReader = new StreamReader((Stream)new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), this._fileEncoding))
            {
                try
                {
                    streamReader.BaseStream.Seek(startingPosition, SeekOrigin.Begin);
                    newData.Data = this.CleanLineData(streamReader.ReadToEnd());
                    newData.FileLength = streamReader.BaseStream.Length;
                    return newData;
                }
                catch (Exception ex)
                {
                    newData.Data = string.Empty;
                    newData.FileLength = -1L;
                    return newData;
                }
            }
        }
        private List<FileInfo> GetChangedLogFiles()
        {
            List<FileInfo> changedLogFiles = new List<FileInfo>();
            foreach (FileInfo fileInfo in new List<FileInfo>((IEnumerable<FileInfo>)((IEnumerable<FileInfo>)new DirectoryInfo(this._logPath).GetFiles((this._logFileType != LogFileType.Game ? this._systemChannelPrefix : "") + "*.txt")).Where<FileInfo>((Func<FileInfo, bool>)(file => file.CreationTime > DateTime.Now.AddDays(-1.0))).OrderByDescending<FileInfo, DateTime>((Func<FileInfo, DateTime>)(file => file.LastWriteTime)).ThenByDescending<FileInfo, string>((Func<FileInfo, string>)(file => file.Name))))
            {
                long fileLength = this.GetFileLength(fileInfo.FullName);
                if (this._fileSizes.ContainsKey(fileInfo.FullName))
                {
                    if (this._fileSizes[fileInfo.FullName] != fileLength)
                    {
                        this._fileSizes[fileInfo.FullName] = fileLength;
                        changedLogFiles.Add(fileInfo);
                    }
                }
                else
                {
                    this._fileSizes.Add(fileInfo.FullName, fileLength);
                    changedLogFiles.Add(fileInfo);
                }
            }
            return changedLogFiles;
        }
        public long GetFileLength(string fileName)
        {
            long fileLength = -1;
            using (StreamReader streamReader = new StreamReader((Stream)new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), this._fileEncoding))
            {
                try
                {
                    fileLength = streamReader.BaseStream.Length;
                }
                catch (Exception ex)
                {
                }
            }
            return fileLength;
        }
        private string GetLogListener(string fileName)
        {
            string empty = string.Empty;
            using (StreamReader streamReader = new StreamReader((Stream)new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), this._fileEncoding))
            {
                try
                {
                    while (streamReader.Peek() >= 0)
                    {
                        Match match = this._localListener.Match(this.CleanLineData(streamReader.ReadLine()) ?? string.Empty);
                        if (match.Success)
                        {
                            empty = match.Groups["name"].ToString();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return empty;
                }
            }
            return empty;
        }
        public void StartWatch()
        {
            this._interestingFiles.Clear();
            FileInfo fileInfo = this.InitLogFileInfo();
            long fileLength = this.GetFileLength(fileInfo.FullName);
            string logListener = this.GetLogListener(fileInfo.FullName);
            this._interestingFiles.Add(fileInfo.FullName, new InterestingFile(fileInfo.FullName, fileLength, DateTime.Now, logListener));
            this.OnProcessNewData(new LogEntry()
            {
                FileName = fileInfo.Name,
                EntryType = this._logFileType == LogFileType.Game ? LogEntryType.OpenGameLogEvent : LogEntryType.OpenChatLogEvent,
                LineContent = fileLength.ToString(),
                CharacterName = logListener
            });
        }
        private FileInfo InitLogFileInfo()
        {
            List<FileInfo> fileInfoList = new List<FileInfo>((IEnumerable<FileInfo>)((IEnumerable<FileInfo>)new DirectoryInfo(this._logPath).GetFiles("local*.txt")).Where<FileInfo>((Func<FileInfo, bool>)(file => file.CreationTime > DateTime.Now.AddDays(-1.0))).OrderByDescending<FileInfo, DateTime>((Func<FileInfo, DateTime>)(file => file.LastWriteTime)).ThenByDescending<FileInfo, string>((Func<FileInfo, string>)(file => file.Name)));
            bool flag = true;
            FileInfo fileInfo1 = (FileInfo)null;
            foreach (FileInfo fileInfo2 in fileInfoList)
            {
                long fileLength = this.GetFileLength(fileInfo2.FullName);
                if (flag)
                {
                    fileInfo1 = fileInfo2;
                    flag = false;
                }
                this._fileSizes.Add(fileInfo2.FullName, fileLength);
            }
            return fileInfo1;
        }
        private string CleanLineData(string line)
        {
            return line.Replace(Convert.ToChar(65279).ToString(), "").Replace(Convert.ToChar(65534).ToString(), "").Replace("\r", "").Trim();
        }
        private void OnProcessNewData(LogEntry entry)
        {
            logger.Debug(entry.ToString());
            if (this.ProcessNewData == null)
                return;
            this.ProcessNewData((object)this, new ProcessNewDataEventArgs(entry, this._systemChannelPrefix));
        }

        private void OnCombat(string logFileName, string characterName, CombatEventType combatEvent)
        {
            if (this.ProcessCombat == null)
                return;
            this.ProcessCombat((object)this, new ProcessCombatEventArgs(logFileName, characterName, combatEvent));
        }
    }
}
