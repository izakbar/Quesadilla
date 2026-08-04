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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ESIData
{
    public class ESIWorker
    {
        private ILogger _log = Log.ForContext<ESIWorker>();
        public EVEStandardAPI _esiClient;

        private BackgroundWorker Worker;
        private int _MAX_ESI_REQUESTS;

        private bool FeedPaused = false;
        private System.Timers.Timer t;

        public event EventHandler<ESIEventArgs> ESIEventHandler;

        public Queue<ESIRequest> ESIRequests = new Queue<ESIRequest>();

        private string LastETAG = "";

        public ESIWorker(EVEStandardAPI esiClient, ILogger Log)
        {
            _esiClient = esiClient;
            _log = Log;
            _MAX_ESI_REQUESTS = 5; // maybe in config later ?

            FeedPaused = true;

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

            // Start on init too 
            //Worker.RunWorkerAsync();
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
            Log.Error("ESI processing PAUSED");
            FeedPaused = true;
        }
        public void Resume()
        {
            Log.Error("ESI processing RESUMED");
            FeedPaused = false;
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //            log.Information("ESI done");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            // check queue etc
            // Log.Information("ESI DoWork");

            ESIRequest request = null;
            int requestsProcessed = 0;

            while (ESIRequests.Count > 0 && requestsProcessed++ < _MAX_ESI_REQUESTS)
            {
                lock (ESIRequests)
                {
                    if (ESIRequests.Count > 0)
                    {
                        request = ESIRequests.Dequeue();
                    }
                }

                if (request == null) { return; }

                //Log.Information($"ESI Request dequeued {request.RequestType} {request.RequestContext}");
                switch (request.RequestType)
                {
                    case ESIRequestType.System:
                        ESIGetSystem(request.RequestContext);
                        break;
					case ESIRequestType.Planet:
						ESIGetPlanet(request.RequestContext);
						break;
					case ESIRequestType.Incursions:
						ESIGetIncursions();
						break;
				}

				if (ESIEventHandler != null)
                {
                    ESIEventHandler((object)this, new ESIEventArgs()
                    {
                        RequestType = request.RequestType,
                    });
                }
            }
        }

        private void ESIGetSystem(int systemId)
        {
            try
            {

                _log.Debug($"Getting planet information for {systemId}");
                var systemInfo = _esiClient.Universe.GetSolarSystemInfoAsync(systemId).Result.Model;
                if (systemInfo != null)
                {
                    _log.Debug($"{systemInfo.Name} {systemInfo.Planets.Count}");
                    List<PlanetInfo> planets = new List<PlanetInfo>();
                    foreach (var p in systemInfo.Planets)
                    {
                        var planetInfo = _esiClient.Universe.GetPlanetInfoAsync(p.PlanetId).Result.Model;
                        var newPlanet = new PlanetInfo()
                        {
                            PlanetId = planetInfo.PlanetId,
                            PlanetName = planetInfo.Name,
                            PlanetTypeId = planetInfo.TypeId,
                            PlanetTypeName = Program.Universe.ItemTypes.ContainsKey((int)planetInfo.TypeId) ? Program.Universe.ItemTypes[(int)planetInfo.TypeId].Name : "Unknown:",
                        };
                        planets.Add(newPlanet);
                    }

                    // all planet info added - add to solarsystem info
                    Program.Universe.SolarSystems[systemId].Planets = planets;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error getting system ESI data {systemId}");
                _log.Debug(ex.Message);
                if (ex.InnerException != null)
                {
                    _log.Debug(ex.InnerException.Message);
                }
            }
        }
        private void ESIGetPlanet(int planetId)
        {
            try
            {
                var planetInfo = _esiClient.Universe.GetPlanetInfoAsync(planetId).Result.Model;
                if (planetInfo != null)
                {
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error getting planet ESI data {planetId}");
                _log.Debug(ex.Message);
                if (ex.InnerException != null)
                {
                    _log.Debug(ex.InnerException.Message);
                }
            }
        }

        private void ESIGetIncursions()
        {
			try
			{
                var incursionInfo = _esiClient.Incursion.ListIncursionsAsync().Result;
				if (incursionInfo != null)
				{
                    // clear all incursions
                    foreach( var s in Program.Universe.SolarSystems.Where(o => o.Value.Incursion)) {
                        s.Value.Incursion = false;
                    }

                    // add new ones
					foreach (var incursion in  incursionInfo.Model)
                    {
                        foreach(var systemId in incursion.InfestedSolarSystems)
                        {
                            Program.Universe.SolarSystems[(int)systemId].Incursion = true;
                        }
                    }
				}
			}
			catch (Exception ex)
			{
				_log.Error($"Error getting incursions ESI data");
				_log.Debug(ex.Message);
				if (ex.InnerException != null)
				{
					_log.Debug(ex.InnerException.Message);
				}
			}
		}

		private void EnqueueRequest(ESIRequestType type, int context)
        {
            lock (ESIRequests)
            {
                var f = ESIRequests.Where(o =>
                (o.RequestType == type) &&
                (o.RequestContext == context)
                ).ToList();
                if (f.Count == 0 || ESIRequests.Count == 0)
                {
                    //Log.Information($"ESI enqueue {context}");
                    ESIRequests.Enqueue(
                        new ESIRequest()
                        {
                            RequestType = type,
                            RequestContext = context,
                        });
                }
            }
        }
        public void EnqueueRequestSystem(int systemid)
        {
            EnqueueRequest(ESIRequestType.System, systemid);
        }
        public void EnqueueRequestPlanet(int planetid)
        {
            EnqueueRequest(ESIRequestType.Planet, planetid);
        }
        public void EnqueueRequestMoon(int moonid)
        {
            EnqueueRequest(ESIRequestType.Moon, moonid);
        }
		public void EnqueueRequestBelt(int beltid)
		{
			EnqueueRequest(ESIRequestType.Belt, beltid);
		}
		public void EnqueueRequestIncursions()
		{
			EnqueueRequest(ESIRequestType.Incursions, -1);
		}

		public Killmail GetKillMail(long killId, string killHash)
        {
            try
            {
                var kill = _esiClient.Killmails.GetKillmailAsync((int)killId, killHash);
                return kill.Result.Model;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex?.InnerException.Message);
                return null;
            }
		}
    }
}
