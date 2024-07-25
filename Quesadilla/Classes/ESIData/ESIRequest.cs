using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ESIData
{
    public class ESIRequest
    {
        public ESIRequestType RequestType {  get; set; }
        public int RequestContext { get; set; }
    }
}
