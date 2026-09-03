using Quesadilla.Classes.Events;
using Quesadilla.Classes.Logfile;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Quesadilla.Classes.Logfile
{
	public class LogWatcher
	{
		private ILogger logger = Log.ForContext<LogWatcher>();
		private const int EXPIRE_OLD_INTEL = 1800;
		private readonly object _interestingFilesLock = new object();
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
		private readonly Regex _gameLogISKRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s\\d{2}\\:\\d{2}\\:\\d{2}\\s\\]\\s\\(bounty\\)", RegexOptions.Compiled);
		private readonly Regex _gameLogMiningRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s\\d{2}\\:\\d{2}\\:\\d{2}\\s\\]\\s\\(mining\\)", RegexOptions.Compiled);
		private readonly Regex _gameLogCloakRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s\\d{2}\\:\\d{2}\\:\\d{2}\\s\\]\\s\\(notify\\)\\s*.*cloak deactivates", RegexOptions.Compiled);

		private readonly Regex _gameLogMiningEndedFullRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s\\d{2}\\:\\d{2}\\:\\d{2}\\s\\]\\s\\(notify\\)\\s*.*completed operations. Ship's cargo hold is full", RegexOptions.Compiled);
		private readonly Regex _gameLogMiningEndedRoidPoppedRegex = new Regex("\\[\\s\\d{4}\\.\\d{2}\\.\\d{2}\\s\\d{2}\\:\\d{2}\\:\\d{2}\\s\\]\\s\\(notify\\)\\s*.*deactivates as it finds the resource it was harvesting\\s", RegexOptions.Compiled);

		private readonly Regex _localSystemChange = new Regex("EVE\\sSystem\\s>\\sChannel\\schanged\\sto\\sLocal\\s:\\s(?<systemname>.*)", RegexOptions.Compiled);
		private readonly Regex _localListener = new Regex("Listener\\:\\s*(?<name>.*)", RegexOptions.Compiled);
		private readonly Regex _localInitialSystem = new Regex("Channel\\sID:\\s*\\(\\(\\'solarsystemid2\\',\\s(?<initialsystem>[0-9]{8})", RegexOptions.Compiled);

		private readonly Regex CHAT_CONNECTION_LOST_MESSAGE = new Regex("EVE\\sSystem\\s>\\sConnection\\sto\\schat\\sserver\\slost.*", RegexOptions.Compiled);
		private readonly Regex CHAT_CONNECTION_RESTORED_MESSAGE = new Regex("EVE\\sSystem\\s>\\sReconnected\\sto\\schat\\sserver.*", RegexOptions.Compiled);

		public event EventHandler<ProcessNewDataEventArgs> ProcessNewData;
		public event EventHandler<ProcessCombatEventArgs> ProcessCombat;
		public event EventHandler<ProcessISKEventArgs> ProcessISK;
		public event EventHandler<ProcessMiningEventArgs> ProcessMining;

		public event EventHandler<ProcessDecloakEventArgs> ProcessDecloak;
		public event EventHandler<ProcessSystemChangeEventArgs> ProcessSystemChange;
		public event EventHandler<ProcessChatServerChangeEventArgs> ProcessChatServerChange;

		public bool Connection = true;
		public bool Enabled = true;
		public string FriendlyName = "";

		public string SystemChannelPrefix => this._systemChannelPrefix;
		public static string RootLogsPath { get; private set; }

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
			if (!Enabled) return;

			var now = DateTime.Now;
			var combatTimeout = now.AddSeconds(-120);
			var cutoff = now.AddMinutes(-120);

			foreach (FileInfo changedLogFile in GetChangedLogFiles())
			{
				long fileLength = 0;
				string logListener = "";
				LocalInfo localInfo = null;

				lock (_interestingFilesLock)
				{
					if (this._interestingFiles.ContainsKey(changedLogFile.FullName)) continue;
					logger.Debug(changedLogFile.FullName);

					fileLength = GetFileLength(changedLogFile.FullName);

					if (this._logFileType == LogFileType.ChatIntel || this._logFileType == LogFileType.Game)
					{
						logListener = GetLogListener(changedLogFile.FullName);
						this._interestingFiles.Add(changedLogFile.FullName, new InterestingFile(changedLogFile.FullName, fileLength, now, logListener));
					}

					if (this._logFileType == LogFileType.Local)
					{
						localInfo = this.InitLocal(changedLogFile.FullName);
						_interestingFiles.Add(changedLogFile.FullName, new InterestingFile(changedLogFile.FullName, fileLength, now, localInfo.CharName));

					}

				}
				if (this._logFileType == LogFileType.ChatIntel || this._logFileType == LogFileType.Game)
				{
					this.OnProcessNewData(new LogEntry()
					{
						FileName = changedLogFile.Name,
						EntryType = this._logFileType == LogFileType.Game ? LogEntryType.NewGameLogEvent : LogEntryType.NewChatLogEvent,
						LineContent = fileLength.ToString(),
						LogType = this._logFileType,
						CharacterName = logListener
					});

				}

				if (this._logFileType == LogFileType.Local)
				{
					OnSystemChange(localInfo.CurrentSystem == string.Empty ? localInfo.InitialSystem.ToString() : localInfo.CurrentSystem, localInfo.CharName);
				}
			}

			if (this._logFileType == LogFileType.Game)
			{
				List<InterestingFile> activeFiles;

				lock (_interestingFilesLock)
				{
					activeFiles = _interestingFiles.Values.ToList();
				}

				foreach (var file in activeFiles)
				{
					if (!file.InCombat) continue;
					if (file.TimeOutTriggered) continue;
					if (file.LastCombat >= combatTimeout) continue;

					file.TimeOutTriggered = true;
					file.InCombat = false;

					OnCombat(file.FileName, file.CharName, CombatEventType.Stop, null);
				}
			}
			lock (_interestingFilesLock)
			{
				var expiredKeys = new List<string>();

				foreach (var file in _interestingFiles)
					if (file.Value.LastUpdate < cutoff) expiredKeys.Add(file.Key);

				foreach (var key in expiredKeys)
					_interestingFiles.Remove(key);
			}


			List<LogEntry> logEntryList = new List<LogEntry>();
			try
			{

				List<KeyValuePair<string, InterestingFile>> files;

				lock (_interestingFilesLock)
				{
					files = _interestingFiles.ToList();
				}

				foreach (var interestingFile in files)
				{
					var file = interestingFile.Value;
					FileInfo fileInfo = new FileInfo(interestingFile.Key);
					if (this.GetFileLength(fileInfo.FullName) != file.LastPosition)
					{
						LogfileData newData;
						newData = ReadLogFile(interestingFile.Key, file.LastPosition);

						if (newData.FileLength < 0) continue;

						file.LastPosition = newData.FileLength;
						file.LastUpdate = now;

						if (this._logFileType == LogFileType.Local)
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
									if (match.Success) OnSystemChange(match.Groups["systemname"].Value, file.CharName);
									match = CHAT_CONNECTION_LOST_MESSAGE.Match(input);
									if (match.Success) OnChatServerChange(false, file.CharName);
									match = CHAT_CONNECTION_RESTORED_MESSAGE.Match(input);
									if (match.Success) OnChatServerChange(true, file.CharName);
								}
							}
						}
						else
						{
							var lines = newData.Data
								.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
								.Select(line => line
									.Replace("\uFEFF", "")
									.Replace("\uFFFE", "")
									.Replace("\r", "")
									.Trim())
								.Where(line => !string.IsNullOrEmpty(line))
								.ToArray(); 
							
							foreach (string input in lines)
								{
									LogEntry tempLogEntry = new LogEntry();
									Match match = this._logFileType == LogFileType.ChatIntel ? this._chatLogRegex.Match(input) : this._gameLogRegex.Match(input);
									tempLogEntry.RawLine = input;
									tempLogEntry.FileName = fileInfo.Name;
									tempLogEntry.LogPrefix = this._systemChannelPrefix;
									tempLogEntry.TimeAdded = now;
									tempLogEntry.LogType = this._logFileType;
									tempLogEntry.CharacterName = file.CharName;

									if (match.Success)
									{
										tempLogEntry.LogTime = match.Groups["time"].Value;
										tempLogEntry.LineContent = match.Groups["content"].Value;
										tempLogEntry.EntryType = LogEntryType.ChatEvent;
										tempLogEntry.ParseSuccess = true;
										if (this._logFileType == LogFileType.ChatIntel)
											tempLogEntry.PlayerName = match.Groups["name"].Value;
									}
									else
									{
										tempLogEntry.LineContent = input;
										tempLogEntry.ParseSuccess = false;
										tempLogEntry.EntryType = this._logFileType == LogFileType.ChatIntel ? LogEntryType.UnknownChatLogEvent : LogEntryType.UnknownGameLogEvent;
									}

									if (this._logFileType == LogFileType.Game)
									{
										if (this._gameLogCombatRegex.IsMatch(input))
										{
											if (!file.InCombat)
											{
												file.InCombat = true;
												this.OnCombat(file.FileName, file.CharName, CombatEventType.Start, tempLogEntry);
											}
											else
											{
												this.OnCombat(file.FileName, file.CharName, CombatEventType.Combat, tempLogEntry);
											}
											file.TimeOutTriggered = false;
											file.LastCombat = file.LastUpdate;
										}
										else if (this._gameLogISKRegex.IsMatch(input))
										{
											this.OnISK(file.FileName, file.CharName, tempLogEntry);
										}
										else if (
											this._gameLogMiningRegex.IsMatch(input) ||
											this._gameLogMiningEndedFullRegex.IsMatch(input) ||
											this._gameLogMiningEndedRoidPoppedRegex.IsMatch(input)
											)
										{
											this.OnMining(file.FileName, file.CharName, tempLogEntry);
										}
										else if (this._gameLogCloakRegex.IsMatch(input))
										{
											this.OnDecloak(file.FileName, file.CharName, tempLogEntry);
										}

									}
									else
									{
										// don't treat as intel if seen in gamelog !
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
			catch (Exception ex)
			{
				Log.Error(ex, "Error processing logfile tick.");
			}


			if (logEntryList.Any())
			{
				foreach (LogEntry entry in logEntryList)
				{
					OnProcessNewData(entry);
				}
			}

			// always expire old intel
			if (_previousLogEntries.Any())
			{
				// check if we should expire first entry.. slow trickle....
				var oldestEntry = _previousLogEntries.First();
				var diffInSeconds = (now - oldestEntry.TimeAdded).TotalSeconds;
				if (diffInSeconds > EXPIRE_OLD_INTEL)
				{
					// expired so dequeue
					_previousLogEntries.Remove(oldestEntry);
				}
			}
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
			try
			{
				/*
								using (StreamReader streamReader = new StreamReader((Stream)new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), this._fileEncoding))
									fileLength = streamReader.BaseStream.Length;
				*/

				using (FileStream fs =
					new FileStream(
						fileName,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite)
					)
					return fs.Length;
			}
			catch (Exception ex)
			{
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
							empty = match.Groups["name"].Value;
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
					lock (_interestingFilesLock)
					{
						// only want most recent for each toon
						var foundChars = new HashSet<string>();
						foreach (var fileInfo in files)
						{
							long fileLength = this.GetFileLength(fileInfo.FullName);
							LocalInfo InitialLocalInfo = InitLocal(fileInfo.FullName);
							_interestingFiles.Add(fileInfo.FullName, new InterestingFile(fileInfo.FullName, fileLength, DateTime.Now, InitialLocalInfo.CharName));
							if (foundChars.Add(InitialLocalInfo.CharName))
							{
								OnSystemChange(InitialLocalInfo.CurrentSystem == string.Empty ? InitialLocalInfo.InitialSystem.ToString() : InitialLocalInfo.CurrentSystem, InitialLocalInfo.CharName);
							}
						}
					}
				}
			}
			else
			{
				if (files.Count == 0)
				{
					return;
				}
				lock (_interestingFilesLock)
				{
					// not local - so should only be one file
					var file = files.First();
					if (!_interestingFiles.ContainsKey(file.FullName))
					{
						long fileLength = this.GetFileLength(file.FullName);
						string logListener = this.GetLogListener(file.FullName);
						this._interestingFiles.Add(file.FullName, new InterestingFile(file.FullName, fileLength, DateTime.Now, logListener));
						this.OnProcessNewData(new LogEntry()
						{
							FileName = file.Name,
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
								localInfo.InitialSystem = int.Parse(match.Groups["initialsystem"].Value);
								continue;
							}
						}
						if (localInfo.CharName == string.Empty)
						{
							Match match = this._localListener.Match(str ?? string.Empty);
							if (match.Success)
							{
								localInfo.CharName = match.Groups["name"].Value;
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
						{
							localInfo.CurrentSystem = match1.Groups["systemname"].Value;
							//OnSystemChange(match1.Groups["systemname"].Value, localInfo.CharName);
						}
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
				if (this._fileSizes.ContainsKey(fileInfo2.FullName))
				{
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
			logger.Debug(
				"{Character}: {Line}",
				entry.CharacterName,
				entry.LineContent);

			ProcessNewData?.Invoke(
				this,
				new ProcessNewDataEventArgs(entry, _systemChannelPrefix));
		}
		private void OnCombat(string logFileName, string characterName, CombatEventType combatEvent, LogEntry entry)
		{
			logger.Debug("{CombatEvent} - {Line}", combatEvent, entry?.LineContent);

			ProcessCombat?.Invoke(this,
				new ProcessCombatEventArgs(
					logFileName,
					characterName,
					combatEvent,
					entry));
		}
		private void OnISK(string logFileName, string characterName, LogEntry entry)
		{
			logger.Debug("{Line}", entry?.LineContent);

			ProcessISK?.Invoke(
				this,
				new ProcessISKEventArgs(
					logFileName,
					characterName,
					entry));
		}
		private void OnMining(string logFileName, string characterName, LogEntry entry)
		{
			logger.Debug("{Line}", entry?.LineContent);

			ProcessMining?.Invoke(
				this,
				new ProcessMiningEventArgs(
					logFileName,
					characterName,
					entry));
		}
		private void OnDecloak(string logFileName, string characterName, LogEntry entry)
		{
			logger.Debug("{Line}", entry?.LineContent);

			ProcessDecloak?.Invoke(
				this,
				new ProcessDecloakEventArgs(
					logFileName,
					characterName,
					entry));
		}
		private void OnSystemChange(string systemName, string charName)
		{
			logger.Debug("{Character} -> {System}", charName, systemName);

			ProcessSystemChange?.Invoke(
				this,
				new ProcessSystemChangeEventArgs(
					systemName,
					charName));
		}
		private void OnChatServerChange(bool chatServerUp, string charName)
		{
			logger.Debug("{Character} -> Chat Server {Status}", charName, chatServerUp ? "Up" : "Down");

			ProcessChatServerChange?.Invoke(
				this,
				new ProcessChatServerChangeEventArgs(
					chatServerUp,
					charName));
		}
	}
}
