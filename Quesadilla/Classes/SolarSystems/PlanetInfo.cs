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
        public int PlanetId { get; set; }
        public string PlanetName { get; set; }
        public int PlanetTypeId { get; set; }
        public string PlanetTypeName { get; set; }
    }
}
