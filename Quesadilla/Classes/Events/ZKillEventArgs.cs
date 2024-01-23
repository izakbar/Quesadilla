using Quesadilla.Classes.ZKill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Events
{
    public class ZKillEventArgs : EventArgs
    {
        public ZKillKill Kill { get; set; }
        public ZKillEventArgs(ZKillKill z)
        {
            Kill = z;
        }
    }
}
