using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public class CombatLine
   {
        public DateTime TimeAdded { get; set; }
        public string Entity {  get; set; }
        public string Text { get; set; }
        public int DamageAmount { get; set; }
        public int NeutAmount { get; set; }

    }
}
