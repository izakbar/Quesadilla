using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quesadilla.Classes.SolarSystems
{
    public class SolarSystemNode : PriorityQueueNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PathInfo PathData { get; set; }
        public List<SolarSystemConnectionData> ConnectedTo { get; set; }
    }
}
