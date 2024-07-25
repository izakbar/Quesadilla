using EVEStandard.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.SolarSystems
{
    public class PlanetInfoDisplay
    {
        public string PlanetType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Planets { get; set; }
        public bool CurrentMouseOver { get; set; }
    }
}
