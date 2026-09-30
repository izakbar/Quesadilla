using Newtonsoft.Json;

namespace Quesadilla.Classes.Anoikis
{
	public class AnoikisCelestialType
	{
		[JsonProperty("typeName")]
		public string TypeName { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }
	}
}