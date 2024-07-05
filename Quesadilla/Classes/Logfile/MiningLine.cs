using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Logfile
{
    public class MiningLine
    {
        public DateTime TimeAdded { get; set; }
        public int MiningAmount { get; set; }
        public int WastageAmount { get; set; }
        public string MiningItem { get; set; }
    }
}
