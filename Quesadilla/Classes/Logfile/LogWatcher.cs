using Quesadilla.Classes.Events;
using Quesadilla.Classes.Logfile;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quesadilla.Classes
{
    public class LogWatcher
    {

        private ILogger logger = Log.ForContext<LogWatcher>();

        private string _logPath;
        private string _systemChannelPrefix;
        private Encoding _fileEncoding;
        public LogFileType _logFileType;
        private List<LogEntry> _previousLogEntries = new List<LogEntry>();
        private Dictionary<string, long> _fileSizes = new Dictionary<string, long>();
        private Dictionary<string, InterestingFile> _interestingFiles = new Dictionary<string, InterestingFile>();
        private readonly Regex _chatLogRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s(?<time>\\d{2}\\:\\d{2}\\:\\d{2})\\s\\]\\s(?<name>\\w.*)\\s>\\s(?<content>.*)", RegexOptions.Compiled);
        private readonly Regex _gameLogRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s(?<time>\\d{2}\\:\\d{2}\\:\\d{2})\\s\\]\\s\\(\\w.*\\)\\s(?<content>.*)", RegexOptions.Compiled);
        private readonly Regex _gameLogCombatRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s\\d{2}\\:\\d{2}\\:\\d{2}\\s\\]\\s\\(combat\\)", RegexOptions.Compiled);

        private readonly Regex _localSystemChange = new Regex("EVE\\sSystem\\s>\\sChannel\\schanged\\sto\\sLocal\\s:\\s(?<systemname>.*)", RegexOptions.Compiled);
        private readonly Regex _localListener = new Regex("Listener\\:\\s*(?<name>.*)", RegexOptions.Compiled);
        private readonly Regex _localInitialSystem = new Regex("Channel\\sID:\\s*\\(\\(\\'solarsystemid2\\',\\s(?<initialsystem>[0-9]{8})", RegexOptions.Compiled);

        private readonly Regex CHAT_CONNECTION_LOST_MESSAGE  = new Regex("EVE\\sSystem\\s>\\sConnection\\sto\\schat\\sserver\\slost.*", RegexOptions.Compiled); 
        private readonly Regex CHAT_CONNECTION_RESTORED_MESSAGE = new Regex("EVE\\sSystem\\s>\\sReconnected\\sto\\schat\\sserver.*", RegexOptions.Compiled);

        public event EventHandler<ProcessNewDataEventArgs> ProcessNewData;
        public event EventHandler<ProcessCombatEventArgs> ProcessCombat;
        public event EventHandler<ProcessSystemChangeEventArgs> SystemChange;
        public event EventHandler<ProcessChatServerChangeEventArgs> ChatServerChange;

        public bool Connection = true;
        public bool Enabled = true;
        public string FriendlyName = "";

        public string SystemChannelPrefix => this._systemChannelPrefix;
        public static string RootLogsPath { get; private set; }
        public LocalInfo InitialLocalInfo { get; set; }

        public LogWatcher(string systemChannelPrefix, LogFileType logFileType, string friendlyName, string logPath = "")
        {
            FriendlyName = friendlyName;
                
            _systemChannelPrefix = systemChannelPrefix;
            _logFileType = logFileType;

            if (LogWatcher.RootLogsPath == null)
                LogWatcher.RootLogsPath = logPath.Length != 0 ? logPath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "EVE\\logs");

            switch (this._logFileType)
            {
                case LogFileType.Game:
                    this._fileEncoding = Encoding.ASCII;
                    this._logPath = LogWatcher.RootLogsPath + "\\Gamelogs";
                    break;
                case LogFileType.ChatIntel:
                    this._fileEncoding = Encoding.Unicode;
                    this._logPath = LogWatcher.RootLogsPath + "\\Chatlogs";
                    break;
                case LogFileType.Local:
                    this._fileEncoding = Encoding.Unicode;
                    this._logPath = LogWatcher.RootLogsPath + "\\Chatlogs";
                    break;
                default:
                    this._fileEncoding = Encoding.Unicode;
                    this._logPath = LogWatcher.RootLogsPath + "\\Chatlogs";
                    break;
            }
        }

        public void Tick()
        {
            // do nothing if disabled
            if (Enabled == false) return;

            foreach (FileInfo changedLogFile in this.GetChangedLogFiles())
            {
                if (!this._interestingFiles.ContainsKey(changedLogFile.FullName))
                {
                    logger.Debug(changedLogFile.FullName);

                    if (this._logFileType == LogFileType.ChatIntel || this._logFileType == LogFileType.Game)
                    {
                        lock (this)
                        {
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
                    if (this._logFileType == LogFileType.Local)
                    {
                        lock (this)
                        {
                            long fileLength = this.GetFileLength(changedLogFile.FullName);
                            LocalInfo localInfo = this.InitLocal(changedLogFile.FullName);
                            this._interestingFiles.Add(changedLogFile.FullName, new InterestingFile(changedLogFile.FullName, fileLength, DateTime.Now, localInfo.CharName));
                            this.OnSystemChange(localInfo.CurrentSystem == string.Empty ? localInfo.InitialSystem.ToString() : localInfo.CurrentSystem, localInfo.CharName);
                        }
                    }
                }
            }
            if (this._logFileType == LogFileType.Game)
            {
                foreach (KeyValuePair<string, InterestingFile> keyValuePair in this._interestingFiles.Where<KeyValuePair<string, InterestingFile>>((Func<KeyValuePair<string, InterestingFile>, bool>)(interestingFile => interestingFile.Value.InCombat && !interestingFile.Value.TimeOutTriggered && interestingFile.Value.LastCombat < DateTime.Now.AddSeconds(-120.0))))
                {
                    keyValuePair.Value.TimeOutTriggered = true;
                    keyValuePair.Value.InCombat = false;
                    this.OnCombat(keyValuePair.Value.FileName, keyValuePair.Value.CharName, CombatEventType.Stop, null);
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
                foreach (KeyValuePair<string, InterestingFile> interestingFile in _interestingFiles)
                {
                    FileInfo fileInfo = new FileInfo(interestingFile.Key);
                    if (this.GetFileLength(fileInfo.FullName) != interestingFile.Value.LastPosition)
                    {
                        LogfileData newData;
                        lock (this)
                        {
                            newData = this.ReadLogFile(interestingFile.Key, interestingFile.Value.LastPosition);
                            this._interestingFiles[interestingFile.Key].LastPosition = newData.FileLength;
                            this._interestingFiles[interestingFile.Key].LastUpdate = DateTime.Now;
                        }

                        if ( this._logFileType == LogFileType.Local )
                        {
                            string[] array = ((IEnumerable<string>)newData.Data.Split(new string[1]
                            {
                "\n"
                            }, StringSplitOptions.RemoveEmptyEntries)).Where<string>((Func<string, bool>)(line => line.Length > 0)).ToArray<string>();
                            if (array.Length != 0)
                            {
                                foreach (string input in array)
                                {
                                    Match match = _localSystemChange.Match(input);
                                    if (match.Success) OnSystemChange(match.Groups["systemname"].ToString(), interestingFile.Value.CharName);
                                    match = CHAT_CONNECTION_LOST_MESSAGE.Match(input);
                                    if (match.Success) OnChatServerChange(false, interestingFile.Value.CharName);
                                    match = CHAT_CONNECTION_RESTORED_MESSAGE.Match(input);
                                    if (match.Success) OnChatServerChange(true, interestingFile.Value.CharName);
                                }
                            }
                        }
                        else 
                        { 
                        string[] array = ((IEnumerable<string>)newData.Data.Split(new string[1]
                        {
                "\n"
                        }, StringSplitOptions.RemoveEmptyEntries)).Select<string, string>((Func<string, string>)(line => line.Replace(Convert.ToChar(65279).ToString(), "").Replace(Convert.ToChar(65534).ToString(), "").Replace("\r", "").Trim())).Where<string>((Func<string, bool>)(line => line.Length > 0)).ToArray<string>();
                            if (array.Length != 0)
                            {
                                foreach (string input in array)
                                {
                                    LogEntry tempLogEntry = new LogEntry();
                                    Match match = this._logFileType == LogFileType.ChatIntel ? this._chatLogRegex.Match(input) : this._gameLogRegex.Match(input);
                                    tempLogEntry.RawLine = input;
                                    tempLogEntry.FileName = fileInfo.Name;
                                    tempLogEntry.LogPrefix = this._systemChannelPrefix;
                                    tempLogEntry.TimeAdded = DateTime.Now;
                                    tempLogEntry.LogType = this._logFileType;
                                    tempLogEntry.CharacterName = interestingFile.Value.CharName;

                                    if (match.Success)
                                    {
                                        tempLogEntry.LogTime = match.Groups["time"].ToString();
                                        tempLogEntry.LineContent = match.Groups["content"].ToString();
                                        tempLogEntry.EntryType = LogEntryType.ChatEvent;
                                        tempLogEntry.ParseSuccess = true;
                                        if (this._logFileType == LogFileType.ChatIntel)
                                            tempLogEntry.PlayerName = match.Groups["name"].ToString();
                                    }
                                    else
                                    {
                                        tempLogEntry.LineContent = input;
                                        tempLogEntry.ParseSuccess = false;
                                        tempLogEntry.EntryType = this._logFileType == LogFileType.ChatIntel ? LogEntryType.UnknownChatLogEvent : LogEntryType.UnknownGameLogEvent;
                                    }

                                    if (this._logFileType == LogFileType.Game && this._gameLogCombatRegex.IsMatch(input))
                                    {
                                        if (!this._interestingFiles[interestingFile.Key].InCombat)
                                        {
                                            this._interestingFiles[interestingFile.Key].InCombat = true;
                                            this.OnCombat(interestingFile.Value.FileName, interestingFile.Value.CharName, CombatEventType.Start, tempLogEntry);
                                        } else
                                        {
                                            this.OnCombat(interestingFile.Value.FileName, interestingFile.Value.CharName, CombatEventType.Combat, tempLogEntry);
                                        }
                                        this._interestingFiles[interestingFile.Key].TimeOutTriggered = false;
                                        this._interestingFiles[interestingFile.Key].LastCombat = this._interestingFiles[interestingFile.Key].LastUpdate;
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
            }
            if (logEntryList.Count <= 0)
                return;
            foreach (LogEntry entry in logEntryList)
                this.OnProcessNewData(entry);
        }
        private LogfileData ReadLogFile(string fileName, long startingPosition)
        {
            LogfileData newData = new LogfileData();
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
            string filespec = "*.txt";
            switch (this._logFileType)
            {
                case LogFileType.Game:
                    filespec = "*.txt";
                    break;
                case LogFileType.ChatIntel:
                    filespec = this._systemChannelPrefix + "*.txt";
                    break;
                case LogFileType.Local:
                    filespec = "local*.txt";
                    break;
                default:
                    filespec = "*.txt";
                    break;
            }

            List<FileInfo> changedLogFiles = new List<FileInfo>();
            foreach (FileInfo fileInfo in new List<FileInfo>((IEnumerable<FileInfo>)((IEnumerable<FileInfo>)new DirectoryInfo(this._logPath).GetFiles(filespec)).Where<FileInfo>((Func<FileInfo, bool>)(file => file.CreationTime > DateTime.Now.AddDays(-1.0))).OrderByDescending<FileInfo, DateTime>((Func<FileInfo, DateTime>)(file => file.LastWriteTime)).ThenByDescending<FileInfo, string>((Func<FileInfo, string>)(file => file.Name))))
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
            List<FileInfo> files = this.InitLogFileInfo();

            if (this._logFileType == LogFileType.Local)
            {
                if (files.Count > 0)
                {
                    lock (this)
                    {
                        // only want most recent for each toon
                        var foundChars = new List<string>();
                        foreach (var fileInfo in files)
                        {
                            long fileLength = this.GetFileLength(fileInfo.FullName);
                            InitialLocalInfo = InitLocal(fileInfo.FullName);
                            _interestingFiles.Add(fileInfo.FullName, new InterestingFile(fileInfo.FullName, fileLength, DateTime.Now, InitialLocalInfo.CharName));
                            if (!foundChars.Contains(InitialLocalInfo.CharName))
                            {
                                OnSystemChange(InitialLocalInfo.CurrentSystem == string.Empty ? InitialLocalInfo.InitialSystem.ToString() : InitialLocalInfo.CurrentSystem, InitialLocalInfo.CharName);
                                foundChars.Add(InitialLocalInfo.CharName);
                            }
                        }
                    }
                }
            }
            else
            {
                if (files.Count > 0)
                {
                    lock (this)
                    {
                        // not local - so should only be one file
                        long fileLength = this.GetFileLength(files.First().FullName);
                        string logListener = this.GetLogListener(files.First().FullName);
                        this._interestingFiles.Add(files.First().FullName, new InterestingFile(files.First().FullName, fileLength, DateTime.Now, logListener));
                        this.OnProcessNewData(new LogEntry()
                        {
                            FileName = files.First().Name,
                            EntryType = this._logFileType == LogFileType.Game ? LogEntryType.OpenGameLogEvent : LogEntryType.OpenChatLogEvent,
                            LineContent = fileLength.ToString(),
                            CharacterName = logListener
                        });
                    }
                }
            }
        }
        private LocalInfo InitLocal(string fileName)
        {
            LocalInfo localInfo = new LocalInfo();
            using (StreamReader streamReader = new StreamReader((Stream)new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), this._fileEncoding))
            {
                try
                {
                    while (streamReader.Peek() >= 0)
                    {
                        string str = this.CleanLineData(streamReader.ReadLine());
                        if (localInfo.InitialSystem == -1)
                        {
                            Match match = this._localInitialSystem.Match(str ?? string.Empty);
                            if (match.Success)
                            {
                                localInfo.InitialSystem = int.Parse(match.Groups["initialsystem"].ToString());
                                continue;
                            }
                        }
                        if (localInfo.CharName == string.Empty)
                        {
                            Match match = this._localListener.Match(str ?? string.Empty);
                            if (match.Success)
                            {
                                localInfo.CharName = match.Groups["name"].ToString();
                                if (localInfo.CharName.Trim().Length <= 4)
                                {
                                    localInfo.CharName = string.Empty;
                                    continue;
                                }
                                continue;
                            }
                        }
                        Match match1 = this._localSystemChange.Match(str ?? string.Empty);
                        if (match1.Success)
                            localInfo.CurrentSystem = match1.Groups["systemname"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    return new LocalInfo();
                }
            }
            return localInfo;
        }
        private List<FileInfo> InitLogFileInfo()
        {

            List<FileInfo> files = new List<FileInfo>();

            string filespec = "*.txt";
            switch (this._logFileType )
            {
                case LogFileType.Game:
                    filespec = "*.txt";
                    break;
                case LogFileType.ChatIntel:
                    filespec = this._systemChannelPrefix + "*.txt";
                    break;
                case LogFileType.Local:
                    filespec = "local*.txt";
                    break;
                default:
                    filespec = "*.txt";
                    break;
            }
            List<FileInfo> fileInfoList = new List<FileInfo>((IEnumerable<FileInfo>)((IEnumerable<FileInfo>)new DirectoryInfo(this._logPath).GetFiles(filespec)).Where<FileInfo>((Func<FileInfo, bool>)(file => file.CreationTime > DateTime.Now.AddDays(-1.0))).OrderByDescending<FileInfo, DateTime>((Func<FileInfo, DateTime>)(file => file.LastWriteTime)).ThenByDescending<FileInfo, string>((Func<FileInfo, string>)(file => file.Name)));
            bool flag = true;
            foreach (FileInfo fileInfo2 in fileInfoList)
            {
                long fileLength = this.GetFileLength(fileInfo2.FullName);
                if (this._logFileType == LogFileType.Local)
                {
                    files.Add(fileInfo2);
                }
                else
                {
                    if (flag)
                    {
                        files.Add(fileInfo2);
                        flag = false;
                    }
                }
                if ( this._fileSizes.ContainsKey(fileInfo2.FullName) ) {
                    this._fileSizes.Remove(fileInfo2.FullName);
                }
                this._fileSizes.Add(fileInfo2.FullName, fileLength);
            }
            return files;
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
        private void OnCombat(string logFileName, string characterName, CombatEventType combatEvent, LogEntry entry)
        {
            logger.Debug(combatEvent.ToString() + "-" + (entry != null ? entry.LineContent: "") );
            if (this.ProcessCombat == null)
                return;
            this.ProcessCombat((object)this, new ProcessCombatEventArgs(logFileName, characterName, combatEvent, entry));
        }
        private void OnSystemChange(string systemName, string charName)
        {
            logger.Debug(charName + "->" +systemName);
            if (this.SystemChange == null)
                return;
            this.SystemChange((object)this, new ProcessSystemChangeEventArgs(systemName, charName));
        }
        private void OnChatServerChange(bool chatServerUp, string charName)
        {
            logger.Debug(charName + "-> Chat Server " + (chatServerUp == true ? "Up" : "Down"));
            if (this.ChatServerChange == null)
                return;
            this.ChatServerChange((object)this, new ProcessChatServerChangeEventArgs(chatServerUp, charName));
        }
    }
}
