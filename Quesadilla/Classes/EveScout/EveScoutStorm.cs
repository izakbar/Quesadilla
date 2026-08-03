using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.EveScout
{
	public class EveScoutStorm
	{
		public string RegionName { get; set; }
		public string SystemName { get; set; }
		public int SystemId { get; set; }

		public string StormType { get; set; }
		public string StormName { get; set; }

		public List<int> StrongSystemIds { get; set; }
		public List<int> WeakSystemIds { get; set; }
	}
}

