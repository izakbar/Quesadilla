using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZkillRedisQZkb
    {
        [JsonPropertyName("awox")]
        public bool Awox { get; set; }

        [JsonPropertyName("fittedValue")]
        public double FittedValue { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; }

        [JsonPropertyName("locationID")]
        public long LocationId { get; set; }

        [JsonPropertyName("npc")]
        public bool Npc { get; set; }

        [JsonPropertyName("points")]
        public long Points { get; set; }

        [JsonPropertyName("solo")]
        public bool Solo { get; set; }

        [JsonPropertyName("totalValue")]
        public double TotalValue { get; set; }
    }
}
