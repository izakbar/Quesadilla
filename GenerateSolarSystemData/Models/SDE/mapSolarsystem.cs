using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateSolarSystemData.Models.SDE
{
	public class mapSolarsystem
	{
		public int _key { get; set; }
		public LocaleStrings Name { get; set; }
		public int regionID { get; set; }
		public MapPosition Position { get; set; }
		public MapPosition2D Position2D { get; set; }
		public double securityStatus { get; set; }
	}
}
