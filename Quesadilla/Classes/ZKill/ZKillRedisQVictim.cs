using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedisQVictim
    {
        [JsonPropertyName("alliance_id")]
        public long AllianceId { get; set; }

        [JsonPropertyName("character_id")]
        public long CharacterId { get; set; }

        [JsonPropertyName("corporation_id")]
        public long CorporationId { get; set; }

        [JsonPropertyName("damage_taken")]
        public long DamageTaken { get; set; }

/*
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }

        [JsonPropertyName("position")]
        public Position Position { get; set; }
*/
        [JsonPropertyName("ship_type_id")]
        public long ShipTypeId { get; set; }
    }
}
