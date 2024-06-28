using Quesadilla.Classes.SolarSystems;
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

        public void EnqueueZKillAlert(Queue<ZKillKill> q, ZKillKill kill, SolarSystem system)
        {
            // only add to new zkill list if not already in list. This should prevent multiple entries for same thing
            var f = q.Where(o => o.ID == kill.ID).ToList();
            if (f.Count == 0 )
            {
                lock (q)
                {
                    kill.VictimCharacterName = Program.Universe.GetCharacterName((int)kill.VictimCharacterId);
                    q.Enqueue(kill);

                    if (system.Kills == null)
                    {
                        system.Kills = new List<ZKillKill>();
                    }
                    system.Kills.Add(kill);
                }
            }
        }

        public void EnqueueZKillAlert(ZKillKill kill, SolarSystem system)
        {
            EnqueueZKillAlert(ZKillKills, kill, system);
        }

        private int ExpireZkillQueue(Queue<ZKillKill> zq)
        {
            int Expired = 0;
            lock (zq)
            {
                bool removedOBSZkill = true;
                while (zq.Count > 0 && removedOBSZkill)
                {
                    var k = zq.First();
                    TimeSpan ts = DateTime.Now - k.TimeAdded;
                    if (ts.TotalSeconds > Program.MainConfiguration.ExpireZKillSeconds)
                    {
                        SolarSystem system = Program.Universe.SolarSystems[(int)k.SolarSystemId];
                        var removed = system.Kills.Remove(k);

                        k.Expired = true;

                        zq.Dequeue();
                        Expired++;
                        removedOBSZkill = true;
                    }
                    else
                    {
                        removedOBSZkill = false;
                    }
                }
            }
            return Expired;
        }

        public int ExpireZKillQueue()
        {
            return ExpireZkillQueue(ZKillKills);
        }

    }
}
