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
        private Dictionary<string, DateTime> retryAfterList = new Dictionary<string, DateTime>();

		public ESIWorker(EVEStandardAPI esiClient, ILogger log)
        {
            _esiClient = esiClient;
            _log = log;
            _MAX_ESI_REQUESTS = 50; // maybe in config later ?

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
            _log.Information("ESI processing PAUSED");
            FeedPaused = true;
        }
        public void Resume()
        {
            _log.Information("ESI processing RESUMED");
            FeedPaused = false;
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //            _log.Information("ESI done");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            // check queue etc
            // _log.Information("ESI DoWork");

            ESIRequest request = null;
            int requestsProcessed = 0;


			while (requestsProcessed++ < _MAX_ESI_REQUESTS)
			{
				lock (ESIRequests)
				{
					if (ESIRequests.Count == 0)
						break;

					request = ESIRequests.Dequeue();
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
					case ESIRequestType.SkyhookTheftData:
                        ESIGetRaidableSkyhooks();
						break;
				}

				ESIEventHandler?.Invoke(this,
					new ESIEventArgs
					{
						RequestType = request.RequestType
					});
			}
        }

        private void ESIGetSystem(int systemId)
        {
            try
            {

                _log.Debug($"Getting planet information for {systemId}");
                var systemInfo = _esiClient.Universe.GetSolarSystemInfoAsync(systemId).GetAwaiter().GetResult().Model;
                if (systemInfo != null)
				{
					_log.Debug($"{systemInfo.Name} {systemInfo.Planets?.Count ?? 0}");
					List<PlanetInfo> planets = new List<PlanetInfo>();

					using (var semaphore = new SemaphoreSlim(4))
					{
						var planetTasks = systemInfo.Planets.Select(async p =>
						{
							await semaphore.WaitAsync();
							try
							{
								return await _esiClient.Universe.GetPlanetInfoAsync(p.PlanetId);
							}
							finally
							{
								semaphore.Release();
							}
						});
						var planetResults = Task.WhenAll(planetTasks).GetAwaiter().GetResult();

						foreach (var result in planetResults)
						{
							var planetInfo = result.Model;

							planets.Add(new PlanetInfo
							{
								PlanetId = planetInfo.PlanetId,
								PlanetName = planetInfo.Name,
								PlanetTypeId = planetInfo.TypeId,
								PlanetTypeName = Program.Universe.ItemTypes.TryGetValue((int)planetInfo.TypeId, out var itemType) ? itemType.Name : "Unknown"
							});
						}
					}

					// all planet info added - add to solarsystem info
					if (Program.Universe.SolarSystems.TryGetValue(systemId, out var solarSystem))
					{
						solarSystem.Planets = planets;
					}

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
                var incursionInfo = _esiClient.Incursion.ListIncursionsAsync().GetAwaiter().GetResult();
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
							if (Program.Universe.SolarSystems.TryGetValue((int)systemId, out var solarSystem))
							{
								solarSystem.Incursion = true;
							}
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
                    //_log.Information($"ESI enqueue {context}");
                    ESIRequests.Enqueue(
                        new ESIRequest()
                        {
                            RequestType = type,
                            RequestContext = context,
                        });
                }
            }
        }
		public void EnqueueRequestSkyhookData()
		{
			EnqueueRequest(ESIRequestType.SkyhookTheftData, -1);
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
                return kill.GetAwaiter().GetResult().Model;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex?.InnerException.Message);
                return null;
            }
		}

        public void ESIGetRaidableSkyhooks()
        {
            if (!retryAfterCheck(System.Reflection.MethodBase.GetCurrentMethod().Name)) return;

            try
            {
                var hooks = _esiClient.Activities.ListRaidableSkyhooksAsync().GetAwaiter().GetResult();
                StoreRetryAfter(System.Reflection.MethodBase.GetCurrentMethod().Name, hooks.RetryAfter);

                foreach (var hook in hooks.Model.Skyhooks)
                {
                    _log.Debug($"Hook : {hook.SolarSystemId} {hook.PlanetId} {hook.TheftVulnerability.Start} {hook.TheftVulnerability.End}");

                    if (Program.Universe.SolarSystems.TryGetValue((int)hook.SolarSystemId, out var solarSystem))
                    {
                        if (solarSystem.Planets != null) // might not have loaded yet :-/
                        {
							var planet = solarSystem.Planets.FirstOrDefault(o => o.PlanetId == hook.PlanetId);
                            if (planet != null)
                            {
                                planet.SkyhookVulneralbilityStart = hook.TheftVulnerability.Start;
                                planet.SkyhookVulneralbilityEnd = hook.TheftVulnerability.End;
                            }
                        }
                    }
                }
            }
            catch
            {
                if (!retryAfterList.ContainsKey(System.Reflection.MethodBase.GetCurrentMethod().Name))
                {
                    StoreRetryAfter(System.Reflection.MethodBase.GetCurrentMethod().Name, 120);
                }
			}
		}

        private void StoreRetryAfter(string method, int seconds) {
            retryAfterList[method] = DateTime.Now.AddSeconds(seconds);
		}

        private bool retryAfterCheck(string method)
        {
            bool result = true;
            if ( retryAfterList.ContainsKey(method) )
            {
                if (retryAfterList[method] > DateTime.Now )
                {
                    result = false;
                } 
			}

            return result;
        }

	}
}
