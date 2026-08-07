using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quesadilla.Classes.SolarSystems
{
    public class PlanetInfo
    {
        public long PlanetId { get; set; }
        public string PlanetName { get; set; }
        public long PlanetTypeId { get; set; }
        public string PlanetTypeName { get; set; }
		public DateTime? SkyhookVulneralbilityStart { get; set; }
		public DateTime? SkyhookVulneralbilityEnd { get; set; }
	}
}
