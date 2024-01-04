using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Events
{
    public class ProcessChatServerChangeEventArgs : EventArgs
    {
        public ProcessChatServerChangeEventArgs(bool chatServerUp, string charName)
        {
            this.ChatServerUp = chatServerUp;
            this.CharName = charName;
        }

        public bool ChatServerUp { get; set; }

        public string CharName { get; set; }
    }
}
