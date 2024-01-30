using Quesadilla.Classes.Logfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Events
{
    public class ProcessISKEventArgs : EventArgs
    {
        public ProcessISKEventArgs(
          string logFileName,
          string characterName,
          LogEntry entry
            )
        {
            this.LogFileName = logFileName;
            this.CharacterName = characterName;
            this.Entry = entry;
        }

        public string LogFileName { get; set; }
        public string CharacterName { get; set; }
        public LogEntry Entry { get; set; }
    }
}