using Newtonsoft.Json;

namespace Quesadilla.Classes.Anoikis
{
	public class AnoikisWormholeClass
	{
		[JsonIgnore]
		public string Name { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("wormholeClassID")]
		public int WormholeClassID { get; set; }

		[JsonProperty("effectPower")]
		public int EffectPower { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }
	}
}