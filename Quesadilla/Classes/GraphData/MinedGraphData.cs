using System.Text;

namespace Quesadilla.Classes.GraphData
{
	public class MinedGraphData
	{
		public int MinedAmount { get; set; }
		public int WastageAmount { get; set; }
		public string RockType { get; set; }

		public string ToCSV()
		{
			return $"{RockType},{MinedAmount},{WastageAmount}";
		}
	}
}
