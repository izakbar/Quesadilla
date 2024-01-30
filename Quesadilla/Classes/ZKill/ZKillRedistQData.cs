using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedistQData
    {
        [JsonProperty("package")]
        public ZKillRedistQPackage Package { get; set; }
    }
}
