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
			StringBuilder sb = new StringBuilder();
			sb.Append(IskAmount);
			return sb.ToString();
		}
	}
}
