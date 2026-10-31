using Newtonsoft.Json;
using System.Collections.Generic;

namespace Quesadilla.Classes.Anoikis
{
	public class AnoikisStaticData
	{
		[JsonProperty("version")]
		public int Version { get; set; }

		[JsonProperty("systems")]
		public Dictionary<string, AnoikisSystemInfo> Systems { get; set; }

		[JsonProperty("wormholes")]
		public Dictionary<string, AnoikisWormhole> Wormholes { get; set; }

		[JsonProperty("wormholeclasses")]
		public Dictionary<string, AnoikisWormholeClass> WormholeClasses { get; set; }

		[JsonProperty("regions")]
		public Dictionary<long, string> Regions { get; set; }

		[JsonProperty("constellations")]
		public Dictionary<long, string> Constellations { get; set; }

		[JsonProperty("celestialtypes")]
		public Dictionary<long, AnoikisCelestialType> CelestialTypes { get; set; }
	}
}