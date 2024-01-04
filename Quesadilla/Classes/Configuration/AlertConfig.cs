using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Configuration
{
    public class AlertConfig
    {
        public bool Enabled { get; set; }
        public AlertConfigType AlertType { get; set; }
        public AlertConfigRangeType AlertRangeType { get; set; }
        public string SoundFile { get; set; }
        public string TextDisplay { get; set; }
        public int? Jumps1Number { get; set; }
        public AlertConfigJumpsComparison Jumps1Comparison { get; set; }
        public int? Jumps2Number { get; set;}
        public AlertConfigJumpsComparison Jumps2Comparison { get; set; }
        public string TriggerText { get; set; }
        public int? CustomTriggerEverySeconds { get; set; }
    }
}
