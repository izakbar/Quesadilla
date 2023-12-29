using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Models
{
    public class LocalInfo
    {
        public LocalInfo()
        {
            this.CharName = string.Empty;
            this.InitialSystem = -1;
            this.CurrentSystem = string.Empty;
        }

        public string CharName { get; set; }

        public int InitialSystem { get; set; }

        public string CurrentSystem { get; set; }
    }
}
