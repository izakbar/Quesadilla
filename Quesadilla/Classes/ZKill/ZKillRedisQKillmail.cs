using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedisQKillmail
    {
        [JsonProperty("attackers")]
        public List<ZKillRedisQAttacker> Attackers { get; set; }

        [JsonProperty("killmail_id")]
        public long ID { get; set; }

        [JsonProperty("killmail_time")]
        public System.DateTimeOffset Timestamp { get; set; }

        [JsonProperty("solar_system_id")]
        public long SolarSystemId { get; set; }

        [JsonProperty("victim")]
        public ZKillRedisQVictim Victim { get; set; }

    }
}
