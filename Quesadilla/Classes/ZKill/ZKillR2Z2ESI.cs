using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
	public class ZKillR2Z2ESI
	{
		[JsonPropertyName("attackers")]
		public List<ZKillAttacker> Attackers { get; set; }
		[JsonPropertyName("killmail_id")]
		public long ID { get; set; }

		[JsonPropertyName("killmail_time")]
		public System.DateTimeOffset Timestamp { get; set; }

		[JsonPropertyName("solar_system_id")]
		public long SolarSystemId { get; set; }

		[JsonPropertyName("victim")]
		public ZKillVictim Victim { get; set; }

	}
}
