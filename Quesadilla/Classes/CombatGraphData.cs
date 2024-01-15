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

            return sb.ToString();
        }
    }
}
