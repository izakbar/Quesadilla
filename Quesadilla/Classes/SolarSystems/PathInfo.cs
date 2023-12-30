using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.SolarSystems
{
    public class PathInfo
    {
        public int TotalJumps => PathSystems.Count - 1;

        public List<int> PathSystems { get; set; }

        public int FromSystem { get; set; }

        public int ToSystem { get; set; }

        public string PathId => String.Format("{0}:{1}", FromSystem, ToSystem);
    }
}