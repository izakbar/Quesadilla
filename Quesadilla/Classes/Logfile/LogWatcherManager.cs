using Quesadilla.Classes.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Quesadilla.Classes.Logfile
{
	public class LogWatcherManager
	{
		private List<LogWatcher> _logfileWatchers = new List<LogWatcher>();
		public IReadOnlyCollection<LogWatcher> Watchers	=> _logfileWatchers;
		public event EventHandler<ProcessCombatEventArgs> ProcessCombat;
		public event EventHandler<ProcessISKEventArgs> ProcessISK;
		public event EventHandler<ProcessMiningEventArgs> ProcessMining;
		public event EventHandler<ProcessDecloakEventArgs> ProcessDecloak;

		public event EventHandler<ProcessSystemChangeEventArgs> ProcessSystemChange;
		public event EventHandler<ProcessChatServerChangeEventArgs> ProcessChatServerChange;
		public event EventHandler<ProcessNewDataEventArgs> ProcessNewData;
		private readonly Timer _timer;
		private readonly int _timerInterval;
		public LogWatcherManager(int intervalMs)
		{
			_timer = new Timer(
			Tick,
			null,
			Timeout.Infinite,
			Timeout.Infinite);

			_timerInterval = intervalMs;
		}

		public IEnumerable<string> GetFriendlyNames() => _logfileWatchers.Select(o => o.FriendlyName);
		public LogWatcher GetWatcherByFriendlyName(string friendlyName) => _logfileWatchers.FirstOrDefault(o => o.FriendlyName == friendlyName) ?? null;
		public LogWatcher AddWatcher(string logfile, LogFileType type, string friendlyName)
		{
			var watcher = new LogWatcher(logfile, type, friendlyName);
			_logfileWatchers.Add(watcher);
			return watcher;
		}
		public void Start()
		{
			foreach (LogWatcher watcher in _logfileWatchers)
			{
				watcher.StartWatch();
			}

			// start timer
			_timer.Change(0, _timerInterval);
		}

		private void Tick(object state)
		{
			foreach (var watcher in _logfileWatchers)
			{
				watcher.Tick();
			}
		}
		private void Watcher_ProcessCombat(object sender, ProcessCombatEventArgs e)
		{
			ProcessCombat?.Invoke(sender, e);
		}

		private void Watcher_ProcessISK(object sender, ProcessISKEventArgs e)
		{
			ProcessISK?.Invoke(sender, e);
		}

		private void Watcher_ProcessMining(object sender, ProcessMiningEventArgs e)
		{
			ProcessMining?.Invoke(sender, e);
		}

		private void Watcher_ProcessDecloak(object sender, ProcessDecloakEventArgs e)
		{
			ProcessDecloak?.Invoke(sender, e);
		}

		private void Watcher_ProcessSystemChange(object sender, ProcessSystemChangeEventArgs e)
		{
			ProcessSystemChange?.Invoke(sender, e);
		}

		private void Watcher_ProcessChatServerChange(object sender, ProcessChatServerChangeEventArgs e)
		{
			ProcessChatServerChange?.Invoke(sender, e);
		}

		private void Watcher_ProcessNewData(object sender, ProcessNewDataEventArgs e)
		{
			ProcessNewData?.Invoke(sender, e);
		}

		public void SetupEvents()
		{
			foreach (var watcher in _logfileWatchers)
			{
				switch (watcher._logFileType)
				{
					case LogFileType.Local:
						watcher.ProcessSystemChange += Watcher_ProcessSystemChange;
						watcher.ProcessChatServerChange += Watcher_ProcessChatServerChange;
						break;

					case LogFileType.Game:
						watcher.ProcessCombat += Watcher_ProcessCombat;
						watcher.ProcessISK += Watcher_ProcessISK;
						watcher.ProcessMining += Watcher_ProcessMining;
						watcher.ProcessDecloak += Watcher_ProcessDecloak;
						break;

					default:
						watcher.ProcessNewData += Watcher_ProcessNewData;
						break;
				}
			}
		}
	}
}
