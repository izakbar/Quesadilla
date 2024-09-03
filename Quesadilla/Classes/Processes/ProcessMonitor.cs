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
        private ProcessInfo _currentProcessInfo;

        public ProcessMonitor()
        {
            this._processCache = new Dictionary<IntPtr, string>(512);

            // This field cannot be initialized properly in constructor
            // At the moment this code is executed the main application window is not yet initialized
            this._currentProcessInfo = new ProcessInfo(IntPtr.Zero, "");
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        public IntPtr GetForegroundWindowHandle()
        {
            return GetForegroundWindow();
        }


        [DllImport("user32.dll")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        public IntPtr SetForegroundWindowHandle(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hwnd, ref Rect rectangle);
        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hwnd, ref Point point);
        public bool GetWindowDimensions(IntPtr hwnd, ref Rect rectangle, ref Point point)
        {
            var result = GetWindowRect(hwnd, ref rectangle);
            return result;
        }

        private bool IsMonitoredProcess(string processName)
        {
            // This is a possible extension point
            return String.Equals(processName, ProcessMonitor.DEFAULT_PROCESS_NAME, StringComparison.OrdinalIgnoreCase);
        }
        public void GetUpdatedProcesses()
        {

            IList<IntPtr> knownProcesses = new List<IntPtr>(this._processCache.Keys);
            foreach (Process process in Process.GetProcesses())
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
                string title = this._processCache[index];
                this._processCache.Remove(index);
            }
        }
        public string GetTitleOfKnownProcess(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return null;
            }

            if ( _processCache.ContainsKey(windowHandle) )
            {
                return _processCache[windowHandle];
            }
            return null;
        }

        public bool IsCharStillActive(string c)
        {
             var p = _processCache.Values.Where(o => o.Equals("EVE - " + c)).ToList();
             if ( p != null && p.Count > 0 ) { return true; }
            return false;
        }

    }
}
