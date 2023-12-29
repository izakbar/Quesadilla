using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Models
{
    public class InterestingFile
    {
        public InterestingFile(
          string fileName,
          long lastPosition,
          DateTime lastUpdate,
          string charName = "")
        {
            this.FileName = fileName;
            this.LastPosition = lastPosition;
            this.LastUpdate = lastUpdate;
            this.CharName = charName;
            this.TimeOutTriggered = true;
        }

        public string FileName { get; set; }

        public long LastPosition { get; set; }

        public DateTime LastUpdate { get; set; }

        public DateTime LastCombat { get; set; }

        public string CharName { get; set; }

        public bool TimeOutTriggered { get; set; }

        public bool InCombat { get; set; }
    }
}
