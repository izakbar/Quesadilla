using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Logfile
{
    public class LogEntry
    {
        public string FileName { get; set; }

        public string LogTime { get; set; }

        public string PlayerName { get; set; }

        public string CharacterName { get; set; }

        public string LineContent { get; set; }

        public string LogPrefix { get; set; }

        public LogFileType LogType { get; set; }

        public LogEntryType EntryType { get; set; }

        public bool ParseSuccess { get; set; }

        public string RawLine { get; set; }

        public HashSet<int> MatchedIds { get; set; }

        public DateTime TimeAdded { get; set; }
        public string ToString()
        {
            return CharacterName + "(" + PlayerName + ") " + LogType + ":" + LineContent;        }
    }
}
