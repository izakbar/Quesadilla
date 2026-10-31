using System.Text;

namespace Quesadilla.Classes.GraphData
{
	public class CombatGraphData
	{
		public double DamageReceived { get; set; }
		public double DamageGiven { get; set; }
		public double NeutsReceived { get; set; }
		public double NeutsGiven { get; set; }
		public double ShieldReceived { get; set; }
		public double ShieldGiven { get; set; }
		public double ArmorReceived { get; set; }
		public double ArmorGiven { get; set; }
		public double HullReceived { get; set; }
		public double HullGiven { get; set; }
		public double CapReceived { get; set; }
		public double CapGiven { get; set; }

		public string ToCSV()
		{

			return string.Join(",",
				DamageReceived,
				DamageGiven,
				NeutsReceived,
				NeutsGiven,
				ShieldReceived,
				ShieldGiven,
				ArmorReceived,
				ArmorGiven,
				HullReceived,
				HullGiven,
				CapReceived,
				CapGiven);

		}
	}
}
