using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedisQAttacker
    {
        [JsonPropertyName("character_id")]
        public long CharacterId { get; set; }

        [JsonPropertyName("corporation_id")]
        public long CorporationId { get; set; }

        [JsonPropertyName("alliance_id")]
        public long AllianceId { get; set; }

        [JsonPropertyName("damage_done")]
        public long DamageDone { get; set; }

        [JsonPropertyName("faction_id")]
        public long FactionId { get; set; }

        [JsonPropertyName("final_blow")]
        public bool FinalBlow { get; set; }

        [JsonPropertyName("security_status")]
        public Decimal SecurityStatus { get; set; }

        [JsonPropertyName("ship_type_id")]
        public long ShipTypeId { get; set; }

        [JsonPropertyName("weapon_type_id")]
        public long WeaponTypeId { get; set; }
    }
}
