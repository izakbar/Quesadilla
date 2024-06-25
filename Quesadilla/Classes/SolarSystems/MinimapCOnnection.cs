using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.SolarSystems
{
    public class MinimapConnection
    {
        public SolarSystem FromSystem { get; set; }
        public SolarSystem ToSystem { get; set; }
        public bool route {  get; set; }
        public Point P1 { get; set; }
        public Point P2 { get; set; }
    }
}
