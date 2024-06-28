using Quesadilla.Classes.Logfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Configuration
{
    public class LogfileConfig
    {
        public string FriendlyName { get; set; }
        public bool Enabled { get; set; }
        public LogFileType LogType { get; set; }
        public string Logfile {  get; set; }
    }
}
