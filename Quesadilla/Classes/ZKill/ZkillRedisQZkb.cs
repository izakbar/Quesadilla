using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZkillRedisQZkb
    {
        [JsonProperty("awox")]
        public bool Awox { get; set; }

        [JsonProperty("fittedValue")]
        public double FittedValue { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("locationID")]
        public long LocationId { get; set; }

        [JsonProperty("npc")]
        public bool Npc { get; set; }

        [JsonProperty("points")]
        public long Points { get; set; }

        [JsonProperty("solo")]
        public bool Solo { get; set; }

        [JsonProperty("totalValue")]
        public double TotalValue { get; set; }
    }
}
