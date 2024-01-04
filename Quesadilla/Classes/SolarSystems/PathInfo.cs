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

        public int FromSystemId { get; set; }

        public int ToSystemId { get; set; }
        public string FromSystemName { get; set; }

        public string ToSystemName { get; set; }

        public string PathId => String.Format("{0}:{1}", FromSystemId, ToSystemId);
    }
}