using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Logfile
{
    public class CombatLine
   {
        public DateTime TimeAdded { get; set; }
        public string Entity {  get; set; }
        public string Text { get; set; }
        public int DamageAmount { get; set; }
        public int NeutAmount { get; set; }
        public int ShieldAmount { get; set; }
        public int ArmorAmount { get; set; }
        public int HullAmount { get; set; }
        public int CapAmount { get; set; }

        public bool Received { get; set; }
    }
}
