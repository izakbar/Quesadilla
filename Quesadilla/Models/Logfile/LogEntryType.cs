using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Models.Logfile
{
    public enum LogEntryType
    {
        OpenChatLogEvent,
        NewChatLogEvent,
        OpenGameLogEvent,
        NewGameLogEvent,
        UnknownChatLogEvent,
        UnknownGameLogEvent,
        ExpiredChatLogEvent,
        ExpiredGameLogEvent,
        ChatEvent,
    }
}
