using Newtonsoft.Json;
using System.Collections.Generic;

namespace Quesadilla.Classes.Anoikis
{
	public class AnoikisWormhole
	{
		[JsonIgnore]
		public string Name { get; set; }

		[JsonProperty("typeID")]
		public long TypeID { get; set; }

		[JsonProperty("src")]
		public List<string> Source { get; set; } = new List<string>();

		[JsonProperty("dest")]
		public string Destination { get; set; }

		[JsonProperty("static")]
		public bool? Static { get; set; }

		[JsonProperty("lifetime")]
		public double? Lifetime { get; set; }

		[JsonProperty("max_mass_per_jump")]
		public long? MaxMassPerJump { get; set; }

		[JsonProperty("total_mass")]
		public long? TotalMass { get; set; }

		[JsonProperty("mass_regen")]
		public long? MassRegen { get; set; }

		public override string ToString()
		{
			return Name + " -> " + Destination;
		}
	}
}