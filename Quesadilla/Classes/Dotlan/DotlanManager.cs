using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Classes.ZKill;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private string LastDotlanDeltaETAG = "";

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
            RunWorkerIfPossible();
        }

        public void RunWorkerIfPossible()
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
            DateTime NowCheck = DateTime.Now;


            foreach(var region in Program.Universe.DotlanMapRegions.Keys)
            {
                if (regionToProcess == "" &&
                    RegionsToCheck.Contains(region) &&
                    (
                    (Program.MainConfiguration.DotlanMapPull && Program.Universe.DotlanMapRegions[region].AddMinutes(Program.MainConfiguration.DotlanMapExpiresMinutes) < NowCheck) ||
                    (Program.MainConfiguration.DotlanDeltaPull && Program.Universe.DotlanDeltaRegions.AddMinutes(Program.MainConfiguration.DotlanDeltaExpiresMinutes) < NowCheck) 
                    )
                    )
                {
                    regionToProcess = region;
                }
            }

            if (!regionToProcess.IsNullOrEmpty())
            {
                // get region map data if required
                string dotlanMapContent = "";
                string dotlanDeltaContent = "";
                if (
                    Program.MainConfiguration.DotlanMapPull &&
                    (Program.Universe.DotlanMapRegions[regionToProcess].AddMinutes(Program.MainConfiguration.DotlanMapExpiresMinutes) < NowCheck)
                    )
                {
                    Program.Universe.DotlanMapRegions[regionToProcess] = NowCheck;
                    dotlanMapContent = GetDotlanMapData(regionToProcess, Program.Universe.DotlanMapRegions[regionToProcess]);
                }

                if (
                    Program.MainConfiguration.DotlanDeltaPull &&
                    (Program.Universe.DotlanDeltaRegions.AddMinutes(Program.MainConfiguration.DotlanDeltaExpiresMinutes) < NowCheck)
                    )
                {
                    Program.Universe.DotlanDeltaRegions = NowCheck;
                    dotlanDeltaContent = GetDotlanDeltaData(Program.Universe.DotlanDeltaRegions);
                }

                if (DotlanEventHandler != null)
                {
                    DotlanEventHandler((object)this, new DotlanEventArgs()
                    {
                        RegionName = regionToProcess,
                        MapContent = dotlanMapContent,
                        DeltaContent = dotlanDeltaContent,
                    });
                }
            }
        }

        private string GetDotlanMapData(string regionName, DateTime LastDotlanUpdate)
        {
            string strContent = string.Empty;
            try
            {
                log.Information($"Getting Data from Dotlan for {regionName}");

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
        private string GetDotlanDeltaData(DateTime LastDotlanUpdate)
        {
            string strContent = string.Empty;
            try
            {
                log.Information($"Getting Delta from Dotlan");

                string URL = "https://evemaps.dotlan.net/ajax/npcdelta";

                using (var hc = new HttpClient())
                {

                    string versionNum = "1";
                    string userAgent = $"Mozilla/5.0 (Quesadilla version {versionNum} )";
                    hc.DefaultRequestHeaders.Add("User-Agent", userAgent);
                    hc.DefaultRequestHeaders.IfModifiedSince = LastDotlanUpdate;

                    if (LastDotlanDeltaETAG != "")
                    {
                        hc.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(LastDotlanDeltaETAG));
                    }

                    var response = hc.GetAsync(URL).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        strContent = response.Content.ReadAsStringAsync().Result;

                        // if Last Modified set - then set expiry of data
                        if (response.Content.Headers.LastModified.HasValue)
                        {
                            Random rndUpdateOffset = new Random();
                            Program.Universe.DotlanDeltaRegions = response.Content.Headers.LastModified.Value.DateTime.ToLocalTime() + TimeSpan.FromMinutes(60) 
                                + TimeSpan.FromSeconds(rndUpdateOffset.Next(1, 300)) 
                                - TimeSpan.FromMinutes(Program.MainConfiguration.DotlanDeltaExpiresMinutes);
                            log.Information($"dotlan delta - lastmodied {response.Content.Headers.LastModified} setting DotlanDeltaRegions {Program.Universe.DotlanDeltaRegions}");
                        }

                        if (response.Headers.ETag != null)
                        {
                            LastDotlanDeltaETAG = response.Headers.ETag.Tag;
                        }
                    }
                    else
                    {
                        strContent = "Error";
                    }
                }
            }
            catch
            {
                log.Error($"Dotlan delta request failed");
                return null;
            }

            // if we have an error - sleep a long time - consider pause
            if (strContent == "Error")
            {
                log.Error("Getting error back from Dotlan - sleeping worker");
                Thread.Sleep(500000);
                return null;
            }

            try
            {
                return strContent;
            }
            catch (Exception ex)
            {
                log.Error($"Dotlan delta request failed");
                return null;
            }
        }
    }
}
