using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Classes.ZKill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Queues
{
    public class AlertQueueItem : IEquatable<AlertQueueItem>
    {
        public PathInfo Path { get; set; }
        public LogEntry Entry { get; set; }
        public ZKillKill Kill { get; set; }
        public string AlertText { get; set; }
        public string MenuText { get; set; }
        public string RouteText { get; set; }
        public string Context { get; set; }
        public string SoundFile { get; set; }
        public string FriendlyName { get; set; }
        public bool isFaction { get; set; }
        public bool isDecloak { get; set; }
        public bool ShowAlert { get; set; }
        public Guid AlertGUID { get; set; }

        public AlertQueueItem()
        {
            AlertGUID = Guid.NewGuid();
        }

        public bool Equals(AlertQueueItem other)
        {
            return this.AlertGUID.Equals(other.AlertGUID);
        }
    }
}
