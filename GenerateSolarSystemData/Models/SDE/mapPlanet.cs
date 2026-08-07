using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateSolarSystemData.Models.SDE
{
	public class mapPlanet
	{
		public int _key { get; set; }
		public LocaleStrings name { get; set; }
		public int solarSystemID { get; set; }
		public int typeID { get; set; }
		public int celestialIndex { get; set; }
	}
}
