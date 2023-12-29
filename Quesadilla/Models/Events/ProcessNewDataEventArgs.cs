using Quesadilla.Models.Logfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Models.Events
{
    public class ProcessNewDataEventArgs : EventArgs
    {
        public ProcessNewDataEventArgs(LogEntry logEntry, string logPrefix)
        {
            this.LogEntry = logEntry;
            this.LogPrefix = logPrefix;
        }

        public LogEntry LogEntry { get; set; }

        public string LogFileName { get; set; }

        public string LogPrefix { get; set; }
    }
}