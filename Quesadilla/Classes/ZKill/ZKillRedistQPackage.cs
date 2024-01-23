using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedistQPackage
    {
        [JsonProperty("killID")]
        public long ID { get; set; }
        [JsonProperty("killmail")]
        public ZKillRedisQKillmail Killmail { get; set; }
        [JsonProperty("zkb")]
        public ZkillRedisQZkb Zkb { get; set; }
    }
}
