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
		public localeStrings name { get; set; }
		public int regionID { get; set; }
		public mapPosition position { get; set; }
		public mapPosition2D position2D { get; set; }
		public double securityStatus { get; set; }
	}
}
