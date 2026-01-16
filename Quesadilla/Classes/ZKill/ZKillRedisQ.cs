using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.SolarSystems;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
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

        private int ZkillErrorCount = 0;

        public event EventHandler<ZKillEventArgs> ZKillEventHandler;

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
            Log.Error("ZKill RedisQ processing PAUSED");
            FeedPaused = true;
        }
        public void Resume()
        {
            Log.Error("ZKill RedisQ processing RESUMED");
            FeedPaused = false;
            ZkillErrorCount = 0;            // Reset error counter
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
//            log.Information("zkill done");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            string URL = $"https://zkillredisq.stream/listen.php?queueID={ZkillQueueId}";
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
                log.Error("ZKill RedisQ request failed");
                ZkillErrorCount++;
                return ;
            }

            // if we have an error - sleep a long time - consider pause
            if (strContent == "Error")
            {
                log.Error("Getting error back from ZKill - sleeping worker");
                ZkillErrorCount++;

                if (ZkillErrorCount > 50) this.Pause();  // too many errors - pause

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
                //log.Information($"ZkillID:{zkill.Package.ID} {zkill.Package.Killmail.SolarSystemId}");

                // if event handler AND not npc
                if ( ZKillEventHandler != null )
                {

                    var km = Program.ESIHandler.GetKillMail(zkill.Package.killID, zkill.Package.Zkb.Hash);

                    var mainAttacker = km?.Attackers?.Where(o => o.FinalBlow).First();

                    string isk = "";

                    if (zkill.Package.Zkb.TotalValue < 1000000)
                    {
                        isk = Math.Truncate(zkill.Package.Zkb.TotalValue / 1000).ToString() + "k";
                    }
                    else if (zkill.Package.Zkb.TotalValue < 1000000000)
                    {
                        isk = Math.Truncate(zkill.Package.Zkb.TotalValue / 1000000).ToString() + "m";
                    }
                    else if (zkill.Package.Zkb.TotalValue < 1000000000000)
                    {
                        isk = Math.Round(zkill.Package.Zkb.TotalValue / 1000000000,1).ToString() + "b";
                    }
                    else
                    {
                        isk = Math.Round(zkill.Package.Zkb.TotalValue / 1000000000000,1).ToString() + "t";
                    }

                    ZKillKill z = new ZKillKill()
                    {
                        ID = zkill.Package.killID,
                        SolarSystemId = km.SolarSystemId,
                        Timestamp = km.KillmailTime,
                        TimeAdded = DateTime.Now,

                        VictimAllianceId = km.Victim.AllianceId,
                        VictimCharacterId = km.Victim.CharacterId,
                        VictimCorporationId = km.Victim.CorporationId,
                        VictimShipTypeId = km.Victim.ShipTypeId,

                        KillerAllianceId = mainAttacker.AllianceId,
                        KillerCharacterId = mainAttacker.CharacterId,
                        KillerCorporationId = mainAttacker.CorporationId,
                        KillerShipTypeId = mainAttacker.ShipTypeId,
                        KillerWeaponTypeId = mainAttacker.WeaponTypeId,

                        href = zkill.Package.Zkb.Href,

                        NPC = zkill.Package.Zkb.Npc,
                        TotalValue = isk,

                        Expired = false,

                    };
                    ZKillEventHandler((object)this, new ZKillEventArgs(z));
                }
            }
        }
    }
}
