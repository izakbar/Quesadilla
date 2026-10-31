using Newtonsoft.Json;
using System.Collections.Generic;

namespace Quesadilla.Classes.Anoikis
{
	public class AnoikisSystemInfo
	{
		[JsonProperty("solarSystemName")]
		public string SolarSystemName { get; set; }

		[JsonProperty("solarSystemID")]
		public long SolarSystemID { get; set; }

		[JsonProperty("wormholeClass")]
		public string WormholeClass { get; set; }

		[JsonProperty("effectName")]
		public string EffectName { get; set; }

		[JsonProperty("regionID")]
		public long RegionID { get; set; }

		[JsonProperty("constellationID")]
		public long ConstellationID { get; set; }

		[JsonProperty("statics")]
		public List<string> Statics { get; set; } = new List<string>();

		[JsonProperty("cels")]
		public List<AnoikisCelestialDefinition> Celestials { get; set; } = new List<AnoikisCelestialDefinition>();
	}
}