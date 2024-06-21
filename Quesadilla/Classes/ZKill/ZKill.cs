using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKill
    {
        public Queue<ZKillKill> OBSZKills {  get; set; }
        public List<ZKillKill> ZKillKills { get; set; }
        public ZKillRedisQ ZKillFeed { get; set; }

        public ZKill()
        {
            OBSZKills  = new Queue<ZKillKill>();
            ZKillKills = new List<ZKillKill>();
            ZKillFeed = new ZKillRedisQ();
        }

        public void EnqueueZKillOBS(ZKillKill kill)
        {
            // only add to new zkill list if not already in list. This should prevent multiple entries for same thing
            var f = OBSZKills.Where(o => o.ID == kill.ID).ToList();
            if (f.Count == 0 )
            {
                OBSZKills.Enqueue(kill);
            }
        }

        public void EnqueueZKillAlert(ZKillKill kill)
        {
            lock (ZKillKills)
            {
                ZKillKills.Add(kill);
            }
        }
        public void ExpireZKillQueueOBS()
        {
            // expire zkills (OBS OPage)?
            lock (OBSZKills)
            {
                bool removedOBSZkill = true;
                while (OBSZKills.Count > 0 && removedOBSZkill)
                {
                    var k = OBSZKills.First();
                    TimeSpan ts = DateTime.Now - k.TimeAdded;
                    if (ts.TotalSeconds > Program.q.MainConfiguration.ExpireZKillSeconds)
                    {
                        k.Expired = true;
                        OBSZKills.Dequeue();
                        removedOBSZkill = true;
                    }
                    else
                    {
                        removedOBSZkill = false;
                    }
                }
            }
        }
        public void ExpireZKillQueueAlerts()
        {
            // expire zkills ?
            lock (ZKillKills)
            {
                foreach (var k in ZKillKills)
                {
                    TimeSpan ts = DateTime.Now - k.TimeAdded;
                    if (ts.TotalSeconds > Program.q.MainConfiguration.ExpireZKillSeconds)
                    {
                        Program.Universe.SolarSystems[(int)k.SolarSystemId].Kills.Remove(k);
                        k.Expired = true;
                    }
                }
                ZKillKills.RemoveAll(o => o.Expired);
            }
        }

    }
}
