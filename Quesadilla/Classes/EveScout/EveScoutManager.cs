using EVEStandard;
using EVEStandard.Models;
using Microsoft.IdentityModel.Tokens;
using Quesadilla.Classes.Dotlan;
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
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Quesadilla.Classes.EveScout
{
    public class EveScoutManager
    {
		private readonly ILogger _log;
		public List<EveScoutWH> Signatures = new List<EveScoutWH>();
		public List<EveScoutStorm> Storms = new List<EveScoutStorm>();

		private BackgroundWorker Worker;

        private bool FeedPaused = false;
        private System.Timers.Timer t;

        public event EventHandler<EveScoutEventArgs> EveScoutEventHandler;

        public const int SYSTEMID_THERA = 31000005;
        public const int SYSTEMID_TURNUR = 30002086;

		//        public event EventHandler<ZKillEventArgs> ZKillEventHandler;

		public EveScoutManager(ILogger Log)
		{
			_log = Log;

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
            _log.Error("EveScout processing PAUSED");
            FeedPaused = true;
        }
        public void Resume()
        {
            _log.Error("EveScout processing RESUMED");
            FeedPaused = false;
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //            _log.Information("zkill done");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
			if (EveScoutEventHandler != null)
			{
				EveScoutEventHandler((object)this, new EveScoutEventArgs());
			}
			
            GetEveScoutData("Thera");
            Thread.Sleep(500);
            GetEveScoutData("Turnur");
            
            Thread.Sleep(500);
            GetEveScoutWeather();

			Thread.Sleep(2000);
			if (EveScoutEventHandler != null)
			{
				EveScoutEventHandler((object)this, new EveScoutEventArgs());
			}
		}

        private void GetEveScoutWeather()
        {
			string strContent = string.Empty;
			try
			{
				_log.Information($"Getting Weather Data from EveScout");
				HttpClient hc = new HttpClient();
				string URL = $"https://evescoutrescue.com/home/stormtrack.php";
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
				_log.Error($"EveScout for Weather request failed");
				return;
			}

			// if we have an error - sleep a long time - consider pause
			if (strContent == "Error")
			{
				_log.Error("Getting error back from EveScout Weather - sleeping worker");
				Thread.Sleep(500000);
				return;
			}

            // Parse EveScout Weather
            try
            {
                HtmlAgilityPack.HtmlDocument weatherData = new HtmlAgilityPack.HtmlDocument();
                weatherData.LoadHtml(strContent);

                lock (Storms)
                {
                    // clear down previous entries and readd new
                    Storms.Clear();

                    // Locate the storm tracking table
                    var weatherRows = weatherData.DocumentNode.SelectNodes("//table//tr");

                    foreach (var weatherRow in weatherRows.Skip(1)) // skip header row
                    {
                        var weatherCells = weatherRow.SelectNodes("td");
                        if (weatherCells == null || weatherCells.Count < 3)
                            continue;

                        var systemName = weatherCells[1].InnerText.Trim();
						var parsedSystemId = Program.Universe.SolarSystems.Values.FirstOrDefault(o => o.Name == systemName)?.Id ?? 0;
						Storms.Add(new EveScoutStorm()
                        {
							RegionName = weatherCells[0].InnerText.Trim(),
							SystemName = weatherCells[1].InnerText.Trim(),
							StormName = weatherCells[2].InnerText.Trim(),
							StormType = weatherCells[3].InnerText.Trim(),
                            SystemId = (int)parsedSystemId
						});
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"EveScout Weather request failed");
                return;
            }
		}

		private void GetEveScoutData(string whConnector)
        {
            string strContent = string.Empty;
            try
            {
                _log.Information($"Getting WH Data from EveScout for {whConnector}");
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
                _log.Error($"EveScout for {whConnector} request failed");
                return;
            }

            // if we have an error - sleep a long time - consider pause
            if (strContent == "Error")
            {
                _log.Error("Getting error back from EveScout - sleeping worker");
                Thread.Sleep(500000);
                return;
            }


            // Parse EveScout
            try
            {
                var whData = JsonSerializer.Deserialize<List<EveScoutWH>>(strContent);

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
                _log.Error(ex, $"EveScout for {whConnector} request failed");
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
