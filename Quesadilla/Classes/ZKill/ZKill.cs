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
        // public ZKillRedisQ ZKillFeedRedisQ { get; set; }
        public ZKillR2Z2 ZKillFeedR2Z2 { get; set; }

        public ZKill()
        {
            ZKillKills = new ConcurrentQueue<ZKillKill>();
            // ZKillFeedRedisQ = new ZKillRedisQ();
            ZKillFeedR2Z2 = new ZKillR2Z2();
		}

        public void EnqueueZKillAlert(ConcurrentQueue<ZKillKill> q, ZKillKill kill, SolarSystem system)
        {
            // only add to new zkill list if not already in list. This should prevent multiple entries for same thing
            if ( q.Where(o => o.ID == kill.ID).Count() == 0 )
            {
                kill.VictimCharacterName = "**NO ONE**";
                if (kill.VictimCharacterId != null)
                {
                    kill.VictimCharacterName = Program.Universe.GetCharacterName((int)kill.VictimCharacterId);
                }
            system.AddKill(kill);
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
                    system.RemoveKill(kk);
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
