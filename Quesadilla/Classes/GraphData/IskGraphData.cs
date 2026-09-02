using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.GraphData
{
	public class IskGraphData
	{
		public double IskAmount { get; set; }

		public string ToCSV()
		{
			return IskAmount.ToString();
		}
	}
}
