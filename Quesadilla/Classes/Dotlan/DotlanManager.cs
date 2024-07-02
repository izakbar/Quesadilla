using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Classes.ZKill;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Dotlan
{
    public class DotlanManager
    {
        private ILogger log = Log.ForContext<DotlanManager>();

        private BackgroundWorker Worker;

        private bool FeedPaused = false;
        private System.Timers.Timer t;

        public event EventHandler<DotlanEventArgs> DotlanEventHandler;

        public List<string> RegionsToCheck = new List<string>();

        //        public event EventHandler<ZKillEventArgs> ZKillEventHandler;

        public DotlanManager()
        {
            Worker = new BackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.WorkerReportsProgress = false;
            Worker.DoWork += this.DoWork;
            Worker.RunWorkerCompleted += this.RunWorkerCompleted;

            t = new System.Timers.Timer()
            {
                Interval = 30000,
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
            Log.Error("Dotlan processing PAUSED");
            FeedPaused = true;
        }
        public void Resume()
        {
            Log.Error("Dotlan processing RESUMED");
            FeedPaused = false;
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //            log.Information("zkill done");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            string regionToProcess = "";
            foreach(var region in Program.Universe.Regions.Keys)
            {
                if (regionToProcess == "" &&
                    RegionsToCheck.Contains(region) &&
                    Program.Universe.Regions[region].AddMinutes(Program.MainConfiguration.DotlanDataExpiresMinutes) < DateTime.Now 
                    )
                {
                    regionToProcess = region;
                }
            }

            if (!regionToProcess.IsNullOrEmpty())
            {
                // get region data
                string content = GetDotlanData(regionToProcess, Program.Universe.Regions[regionToProcess]);

                if (content != null && content != "Error")
                {
                    Program.Universe.Regions[regionToProcess] = DateTime.Now;

                    if (DotlanEventHandler != null)
                    {
                        DotlanEventHandler((object)this, new DotlanEventArgs()
                        {
                            RegionName = regionToProcess,
                            Content = content,
                        });
                    }
                }
            }
        }

        private string GetDotlanData(string regionName, DateTime LastDotlanUpdate)
        {
            string strContent = string.Empty;
            try
            {
                Log.Information($"Getting Data from Dotlan for {regionName}");

                string URL = $"http://evemaps.dotlan.net/svg/{regionName.Replace(" ","_")}.svg";

                using (var hc = new HttpClient())
                {

                    string versionNum = "1";
                    string userAgent = $"Mozilla/5.0 (Quesadilla version {versionNum} )";
                    hc.DefaultRequestHeaders.Add("User-Agent", userAgent);
                    hc.DefaultRequestHeaders.IfModifiedSince = LastDotlanUpdate;

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

                /*

                if (response.IsSuccessStatusCode)
                {
                    strContent = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    strContent = "Error";
                }
                */

            }
            catch
            {
                log.Error($"Dotlan for {regionName} request failed");
                return null;
            }

            // if we have an error - sleep a long time - consider pause
            if (strContent == "Error")
            {
                log.Error("Getting error back from Dotlan - sleeping worker");
                Thread.Sleep(500000);
                return null;
            }

            // Parse Dotlan
            try
            {

                /*
                var whData = JsonConvert.DeserializeObject<List<EveScoutWH>>(strContent);

                lock (Signatures)
                {
                    // clear down previous entries and readd new
                    Signatures.RemoveAll(o => o.Connector == regionName);
                    foreach (var w in whData)
                    {
                        w.Connector = regionName;
                        Signatures.Add(w);
                    }
                }
                */

                return strContent;

            }
            catch (Exception ex)
            {
                log.Error($"Dotlan for {regionName} request failed");
                return null;
            }
        }
    }
}
