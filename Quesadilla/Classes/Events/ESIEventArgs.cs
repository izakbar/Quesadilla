using Quesadilla.Classes.ESIData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Events
{
    public class ESIEventArgs : EventArgs
    {
        public ESIRequestType RequestType { get; set; }
    }
}
