using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedisQVictim
    {
        [JsonProperty("alliance_id")]
        public long AllianceId { get; set; }

        [JsonProperty("character_id")]
        public long CharacterId { get; set; }

        [JsonProperty("corporation_id")]
        public long CorporationId { get; set; }

        [JsonProperty("damage_taken")]
        public long DamageTaken { get; set; }

/*
        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("position")]
        public Position Position { get; set; }
*/
        [JsonProperty("ship_type_id")]
        public long ShipTypeId { get; set; }
    }
}
