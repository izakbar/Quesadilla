using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Quesadilla.Classes.EveScout
{
    public class DotlanEventArgs : EventArgs
    {
        public string RegionName { get; set; }
        public string MapContent { get; set; }
        public string DeltaContent { get; set; }
    }
}
