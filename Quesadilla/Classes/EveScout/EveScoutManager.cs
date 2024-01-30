using Newtonsoft.Json;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Classes.ZKill;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quesadilla.Classes.EveScout
{
    public class EveScoutManager
    {
        private ILogger log = Log.ForContext<ZKillRedisQ>();
        public List<EveScoutWH> Signatures = new List<EveScoutWH>();

        private BackgroundWorker Worker;

        private bool FeedPaused = false;
        private System.Timers.Timer t;

        public event EventHandler<EveScoutEventArgs> EveScoutEventHandler;

        public const int SYSTEMID_THERA = 31000005;
        public const int SYSTEMID_TURNUR = 30002086;

        //        public event EventHandler<ZKillEventArgs> ZKillEventHandler;

        public EveScoutManager()
        {
            Worker = new BackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.WorkerReportsProgress = false;
            Worker.DoWork += this.DoWork;
            Worker.RunWorkerCompleted += this.RunWorkerCompleted;

            t = new System.Timers.Timer()
            {
                Interval = 300000,
                AutoReset = true,
                Enabled = true,
            };
            t.Elapsed += this.Tick;

            // Start on init too 
            Worker.RunWorkerAsync();
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
            Log.Error("EveScout processing PAUSED");
            FeedPaused = true;
        }
        public void Resume()
        {
            Log.Error("EveScout processing RESUMED");
            FeedPaused = false;
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //            log.Information("zkill done");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            GetEveScoutData("Thera");
            GetEveScoutData("Turnur");
            if (EveScoutEventHandler != null)
            {
                EveScoutEventHandler((object)this, new EveScoutEventArgs());
            }
        }

        private void GetEveScoutData(string whConnector)
        {
            string strContent = string.Empty;
            try
            {
                Log.Information($"Getting WH Data from EveScout for {whConnector}");
                HttpClient hc = new HttpClient();
                string URL = $"https://api.eve-scout.com/v2/public/signatures?system_name={whConnector}";
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
                log.Error($"EveScout for {whConnector} request failed");
                return;
            }

            // if we have an error - sleep a long time - consider pause
            if (strContent == "Error")
            {
                log.Error("Getting error back from EveScout - sleeping worker");
                Thread.Sleep(500000);
                return;
            }


            // Parse EveScout
            try
            {
                var whData = JsonConvert.DeserializeObject<List<EveScoutWH>>(strContent);

                lock (Signatures)
                {
                    // clear down previous entries and readd new
                    Signatures.RemoveAll(o => o.Connector == whConnector);
                    foreach (var w in whData)
                    {
                        w.Connector = whConnector;
                        Signatures.Add(w);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"EveScout for {whConnector} request failed");
                return;
            }
        }
        public EveScoutWH FindClosest(UniverseManager u, long systemId, string connector)
        {
            EveScoutWH closestSystem = null;
            int closestJumps = 999;

            lock (Signatures)
            {
                foreach (var w in Signatures.Where(o => o.Connector == connector))
                {
                    int toSystem = (int)w.ToSystemId;
                    if (toSystem == SYSTEMID_THERA || toSystem == SYSTEMID_TURNUR)
                    {
                        toSystem = (int)w.FromSystemId;
                    }
                    var path = u.FindAndCachePath((int)systemId, toSystem);
                    if (path != null && path.TotalJumps < closestJumps)
                    {
                        closestJumps = path.TotalJumps;
                        closestSystem = w;
                    }
                }
            }

            return closestSystem;
        }

    }
}
