using Newtonsoft.Json;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.SolarSystems;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillRedisQ
    {
        private ILogger log = Log.ForContext<ZKillRedisQ>();

        private BackgroundWorker Worker;

        private string ZkillQueueId = "Quesadilla" + Guid.NewGuid().ToString();
        private bool FeedPaused = false;
        private bool FeedRunning = false;
        private System.Timers.Timer t;

        public event EventHandler<ZKillEventArgs> ZKillEventHandler;

        public List<ZKillKill> Kills { get; set; }

        public ZKillRedisQ()
        {
            Worker = new BackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.WorkerReportsProgress = false;
            Worker.DoWork += this.DoWork;
            Worker.RunWorkerCompleted += this.RunWorkerCompleted;

            t = new System.Timers.Timer()
            {
                Interval = 1000,
                AutoReset = true,
                Enabled = true,
            };
            t.Elapsed += this.Tick;
        }

        private void Tick(object sender, EventArgs e)
        {
            if (!Worker.IsBusy && !FeedPaused)
            {
                Worker.RunWorkerAsync();
            }
        }


        public void Pause()
        {
            FeedPaused = true;
        }
        public void Resume()
        {
            FeedPaused = false;
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
//            log.Information("zkill done");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            string URL = $"https://redisq.zkillboard.com/listen.php?queueID={ZkillQueueId}";
            string strContent = string.Empty;
            try
            {
                HttpClient hc = new HttpClient();
                var response = hc.GetAsync(URL).Result;
                if (response.IsSuccessStatusCode)
                {
                    strContent = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    strContent = "Error";
                }
            }
            catch
            {
                return ;
            }

            // if we have an error - sleep a long time - consider pause
            if (strContent == "Error")
            {
                Thread.Sleep(500000);
                return;
            }


            // Parse killmail
            var zkill = JsonConvert.DeserializeObject<ZKillRedistQData>(strContent);
            if (zkill == null || zkill.Package == null)
            {
                // Wait 10s for next km
                Thread.Sleep(10000);
            }
            else
            {
                log.Information($"ZkillID:{zkill.Package.ID} {zkill.Package.Killmail.SolarSystemId}");

                // if event handler AND not npc
                if ( ZKillEventHandler != null )
                {

                    if (zkill.Package.Zkb.Npc)
                    {
                        log.Information("NPC Kill");
                    }

                    var mainAttacker = zkill.Package.Killmail.Attackers.Where(o => o.FinalBlow).First();

                    ZKillKill z = new ZKillKill()
                    {
                        ID = zkill.Package.ID,
                        SolarSystemId = zkill.Package.Killmail.SolarSystemId,
                        Timestamp = zkill.Package.Killmail.Timestamp,

                        VictimAllianceId = zkill.Package.Killmail.Victim.AllianceId,
                        VictimCharacterId = zkill.Package.Killmail.Victim.CharacterId,
                        VictimCorporationId = zkill.Package.Killmail.Victim.CorporationId,
                        VictimShipTypeId = zkill.Package.Killmail.Victim.ShipTypeId,

                        KillerAllianceId = mainAttacker.AllianceId,
                        KillerCharacterId = mainAttacker.CharacterId,
                        KillerCorporationId = mainAttacker.CorporationId,
                        KillerShipTypeId = mainAttacker.ShipTypeId,
                        KillerWeaponTypeId = mainAttacker.WeaponTypeId,

                        href = zkill.Package.Zkb.Href,

                    };
                    ZKillEventHandler((object)this, new ZKillEventArgs(z));
                }
            }
        }
    }
}
