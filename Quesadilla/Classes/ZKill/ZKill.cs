using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKill
    {
        public Queue<ZKillKill> ZKillKills { get; set; }
        public ZKillRedisQ ZKillFeed { get; set; }

        public ZKill()
        {
            ZKillKills = new Queue<ZKillKill>();
            ZKillFeed = new ZKillRedisQ();
        }

        public void EnqueueZKillAlert(Queue<ZKillKill> q, ZKillKill kill)
        {
            // only add to new zkill list if not already in list. This should prevent multiple entries for same thing
            var f = q.Where(o => o.ID == kill.ID).ToList();
            if (f.Count == 0 )
            {
                lock (q)
                {
                    q.Enqueue(kill);
                }
            }
        }

        public void EnqueueZKillAlert(ZKillKill kill)
        {
            EnqueueZKillAlert(ZKillKills, kill);
        }

        private void ExpireZkillQueue(Queue<ZKillKill> zq)
        {
            lock (zq)
            {
                bool removedOBSZkill = true;
                while (zq.Count > 0 && removedOBSZkill)
                {
                    var k = zq.First();
                    TimeSpan ts = DateTime.Now - k.TimeAdded;
                    if (ts.TotalSeconds > Program.MainConfiguration.ExpireZKillSeconds)
                    {
                        k.Expired = true;
                        zq.Dequeue();
                        removedOBSZkill = true;
                    }
                    else
                    {
                        removedOBSZkill = false;
                    }
                }
            }
        }

        public void ExpireZKillQueue()
        {
            ExpireZkillQueue(ZKillKills);
        }

    }
}
