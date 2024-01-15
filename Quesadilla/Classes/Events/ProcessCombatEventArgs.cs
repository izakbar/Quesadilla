using Quesadilla.Classes.Logfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Events
{
    public class ProcessCombatEventArgs : EventArgs
    {
        public ProcessCombatEventArgs(
          string logFileName,
          string characterName,
          CombatEventType combatEvent,
          LogEntry entry
            )
        {
            this.LogFileName = logFileName;
            this.CharacterName = characterName;
            this.CombatEvent = combatEvent;
            this.Entry = entry;
        }

        public string LogFileName { get; set; }

        public string CharacterName { get; set; }

        public CombatEventType CombatEvent { get; set; }
        public LogEntry Entry { get; set; }
    }
}