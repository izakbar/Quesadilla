using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKill
    {
        public ConcurrentQueue<ZKillKill> ZKillKills { get; set; }
        public ZKillRedisQ ZKillFeed { get; set; }

        public ZKill()
        {
            ZKillKills = new ConcurrentQueue<ZKillKill>();
            ZKillFeed = new ZKillRedisQ();
        }

        public void EnqueueZKillAlert(ConcurrentQueue<ZKillKill> q, ZKillKill kill, SolarSystem system)
        {
            // only add to new zkill list if not already in list. This should prevent multiple entries for same thing
            if ( q.Where(o => o.ID == kill.ID).Count() == 0 )
            {
            kill.VictimCharacterName = Program.Universe.GetCharacterName((int)kill.VictimCharacterId);
            if (system.Kills == null)
            {
                system.Kills = new List<ZKillKill>();
            }
            system.Kills.Add(kill);

            q.Enqueue(kill);
            }
        }

        public void EnqueueZKillAlert(ZKillKill kill, SolarSystem system)
        {
            EnqueueZKillAlert(ZKillKills, kill, system);
        }

        private int ExpireZkillQueue(ConcurrentQueue<ZKillKill> zq)
        {
            int Expired = 0;
            //lock (zq)
            //{
            bool removedOBSZkill = true;
            while (zq.Count > 0 && removedOBSZkill)
            {
                var k = zq.First();
                TimeSpan ts = DateTime.Now - k.TimeAdded;
                if (ts.TotalSeconds > Program.MainConfiguration.ExpireZKillSeconds)
                {
                    ZKillKill kk = null;
                    zq.TryDequeue(out kk);

                    SolarSystem system = Program.Universe.SolarSystems[(int)kk.SolarSystemId];
                    var removed = system.Kills.Remove(kk);
                    kk.Expired = true;

                    Expired++;
                    removedOBSZkill = true;
                }
                else
                {
                    removedOBSZkill = false;
                }
            }
            //}
            return Expired;
        }

        public int ExpireZKillQueue()
        {
            return ExpireZkillQueue(ZKillKills);
        }

    }
}
