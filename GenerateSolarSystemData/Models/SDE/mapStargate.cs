using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateSolarSystemData.Models.SDE
{
	public class mapStargate
	{
		public int _key { get; set; }
		public int solarSystemID { get; set; }
		public stargateDestination destination { get; set; }
	}
}
