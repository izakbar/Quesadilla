using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Quesadilla.Classes.ZKill
{
	public class ZKillR2Z2Data
	{
		[JsonPropertyName("esi")]
		public ZKillR2Z2ESI esi { get; set; }
		public string hash {  get; set; }
		
		[JsonPropertyName("zkb")]
		public ZkillZkb zkb { get; set; }


	}
}
