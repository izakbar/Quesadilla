using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Quesadilla.Classes.Processes
{
    public class ProcessMonitor
    {
        private const string DEFAULT_PROCESS_NAME = "ExeFile";

        private readonly IDictionary<IntPtr, string> _processCache;
        public ProcessMonitor()
        {
            _processCache = new Dictionary<IntPtr, string>(512);
        }
        private bool IsMonitoredProcess(string processName)
        {
            // This is a possible extension point
            return String.Equals(processName, ProcessMonitor.DEFAULT_PROCESS_NAME, StringComparison.OrdinalIgnoreCase);
        }
        public void GetUpdatedProcesses()
        {

			HashSet<IntPtr> knownProcesses = new HashSet<IntPtr>(_processCache.Keys);

			foreach (var process in Process.GetProcessesByName(DEFAULT_PROCESS_NAME))
			{
                string processName = process.ProcessName;

                if (!this.IsMonitoredProcess(processName))
                {
                    continue;
                }

                IntPtr mainWindowHandle = process.MainWindowHandle;
                if (mainWindowHandle == IntPtr.Zero)
                {
                    continue; // No need to monitor non-visual processes
                }

                string mainWindowTitle = process.MainWindowTitle;
                this._processCache.TryGetValue(mainWindowHandle, out string cachedTitle);

                if (cachedTitle == null)
                {
                    // This is a new process in the list
                    this._processCache.Add(mainWindowHandle, mainWindowTitle);
                }
                else
                {
                    // This is an already known process
                    if (cachedTitle != mainWindowTitle)
                    {
                        this._processCache[mainWindowHandle] = mainWindowTitle;
                    }

                    knownProcesses.Remove(mainWindowHandle);
                }
            }

            foreach (IntPtr index in knownProcesses)
            {
                this._processCache.Remove(index);
            }
        }
        public string GetTitleOfKnownProcess(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return null;
            }

			return _processCache.TryGetValue(
				windowHandle,
				out string title)
					? title
					: null;
        }

		public bool IsCharStillActive(string c)
		{
			return _processCache.Values.Contains($"EVE - {c}");
		}
	}
}
