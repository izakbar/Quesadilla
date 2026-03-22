using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedistQPackage
    {
        [JsonPropertyName("killID")]
        public long killID { get; set; }
        //[JsonPropertyName("killmail")]
        //public ZKillRedisQKillmail Killmail { get; set; }
        [JsonPropertyName("zkb")]
        public ZkillRedisQZkb Zkb { get; set; }
    }
}
