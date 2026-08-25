using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.GraphData
{
	public class MinedGraphData
	{
		public int MinedAmount { get; set; }
		public int WastageAmount { get; set; }
		public string RockType { get; set; }

		public string ToCSV()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(RockType);
			sb.Append(",");
			sb.Append(MinedAmount);
			sb.Append(",");
			sb.Append(WastageAmount);
			return sb.ToString();
		}
	}
}
