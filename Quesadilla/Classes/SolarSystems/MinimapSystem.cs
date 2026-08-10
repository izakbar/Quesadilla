using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.SolarSystems
{
    public class MinimapSystem
    {
        public SolarSystem solarSystem { get; set; }

        public bool fromSystem { get; set; }
        public bool toSystem { get; set; }
        public bool route { get; set; }
		public bool textDisplay { get; set; }
		public bool textDisplayConnecting { get; set; }
		public int X { get; set; }
        public int Y { get; set; }
    }
}
