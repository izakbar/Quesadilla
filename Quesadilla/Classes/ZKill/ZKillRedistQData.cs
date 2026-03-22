using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedistQData
    {
        [JsonPropertyName("package")]
        public ZKillRedistQPackage Package { get; set; }
    }
}
