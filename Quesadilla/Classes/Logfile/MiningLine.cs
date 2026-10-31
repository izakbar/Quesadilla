using System;

namespace Quesadilla.Classes.Logfile
{
    public class MiningLine
    {
        public DateTime TimeAdded { get; set; }
        public int MiningAmount { get; set; }
        public int WastageAmount { get; set; }
        public string MiningItem { get; set; }
        public bool Critical { get; set;  }
    }
}
