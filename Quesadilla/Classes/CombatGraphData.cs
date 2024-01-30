using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public class CombatGraphData
    {
        public double DamageReceived { get; set; }
        public double DamageGiven { get; set; }
        public double NeutsReceived { get; set; }
        public double NeutsGiven { get; set; }
        public double ShieldReceived { get; set; }
        public double ShieldGiven { get; set; }
        public double ArmorReceived { get; set; }
        public double ArmorGiven { get; set; }
        public double HullReceived { get; set; }
        public double HullGiven { get; set; }
        public double CapReceived { get; set; }
        public double CapGiven { get; set; }

        public string ToCSV()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DamageReceived);
            sb.Append(",");
            sb.Append(DamageGiven);
            sb.Append(",");
            sb.Append(NeutsReceived);
            sb.Append(",");
            sb.Append(NeutsGiven);
            sb.Append(",");

            sb.Append(ShieldReceived);
            sb.Append(",");
            sb.Append(ShieldGiven);
            sb.Append(",");

            sb.Append(ArmorReceived);
            sb.Append(",");
            sb.Append(ArmorGiven);
            sb.Append(",");

            sb.Append(HullReceived);
            sb.Append(",");
            sb.Append(HullGiven);
            sb.Append(",");

            sb.Append(CapReceived);
            sb.Append(",");
            sb.Append(CapGiven);
            sb.Append(",");

            return sb.ToString();
        }
    }
}
