using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Events
{
    public class ProcessSystemChangeEventArgs : EventArgs
    {
        public ProcessSystemChangeEventArgs(string systemName, string charName)
        {
            this.SystemName = systemName;
            this.CharName = charName;
        }

        public string SystemName { get; set; }

        public string CharName { get; set; }
    }
}
