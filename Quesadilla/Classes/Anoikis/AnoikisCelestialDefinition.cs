using Newtonsoft.Json;

namespace Quesadilla.Classes.Anoikis
{
	[JsonConverter(typeof(AnoikisCelestialConverter))]
	public class AnoikisCelestialDefinition
	{
		public int GroupID { get; set; }

		public long TypeID { get; set; }

		public double X { get; set; }

		public double Y { get; set; }

		public double Z { get; set; }

		public int? CelestialIndex { get; set; }

		public int MoonCount { get; set; }

		public int? OrbitIndex { get; set; }

		public string ItemName { get; set; }
	}
}