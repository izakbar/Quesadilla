using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Quesadilla.Classes.Anoikis
{
	public class AnoikisManager
	{
		private readonly ILogger log = Log.ForContext<AnoikisManager>();
		private readonly object _lock = new object();

		private readonly BackgroundWorker Worker;
		private readonly System.Timers.Timer RefreshTimer;

		private AnoikisStaticData _data;

		private Dictionary<string, List<AnoikisWormhole>> _wormholesBySource = new Dictionary<string, List<AnoikisWormhole>>(StringComparer.OrdinalIgnoreCase);

		private const int RefreshMinutes = 61;
		private const string StaticDataUrl = "https://anoik.is/static/static.json?version=11";

		private readonly string CacheFilename;

		public event EventHandler<AnoikisLoadedEventArgs> AnoikisLoadedEventHandler;

		public bool IsLoaded
		{
			get
			{
				lock (_lock)
				{
					return _data != null;
				}
			}
		}

		public int Version
		{
			get
			{
				lock (_lock)
				{
					return _data != null ? _data.Version : 0;
				}
			}
		}

		public AnoikisManager()
		{
			CacheFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Quesadilla.anoikis-static.json");

			Worker = new BackgroundWorker();
			Worker.WorkerSupportsCancellation = false;
			Worker.WorkerReportsProgress = false;
			Worker.DoWork += DoWork;
			Worker.RunWorkerCompleted += RunWorkerCompleted;

			// Load anything we already have immediately.
			bool loadedFromCache = LoadCache();

			// Refresh in the background if we have nothing or the cache is old.
			if (!loadedFromCache || IsCacheExpired())
			{
				RunWorkerIfPossible();
			}

			// We only need to check periodically whether the cache is stale.
			RefreshTimer = new System.Timers.Timer()
			{
				Interval = 60000,
				AutoReset = true,
				Enabled = true
			};

			RefreshTimer.Elapsed += RefreshTimerElapsed;
		}

		private void RefreshTimerElapsed(object sender, EventArgs e)
		{
			if (IsCacheExpired())
			{
				RunWorkerIfPossible();
			}
		}

		private void RunWorkerIfPossible()
		{
			if (!Worker.IsBusy)
			{
				Worker.RunWorkerAsync();
			}
		}

		private bool LoadCache()
		{
			try
			{
				if (!File.Exists(CacheFilename))
				{
					log.Information("Anoikis cache does not exist");
					return false;
				}

				log.Information($"Loading Anoikis data from {CacheFilename}");

				string json = File.ReadAllText(CacheFilename);

				if (string.IsNullOrWhiteSpace(json))
				{
					log.Warning("Anoikis cache is empty");
					return false;
				}

				AnoikisStaticData data = ParseData(json);

				if (data == null)
				{
					log.Warning("Anoikis cache could not be parsed");
					return false;
				}

				InstallData(data);

				TimeSpan age = DateTime.UtcNow - File.GetLastWriteTimeUtc(CacheFilename);

				log.Information(
					$"Anoikis cache loaded. Version {data.Version}, " +
					$"{data.Systems.Count} systems, {data.Wormholes.Count} wormholes, " +
					$"age {age.TotalMinutes:0.0} minutes");

				return true;
			}
			catch (Exception ex)
			{
				// A bad local cache doesn't stop Anoikis from downloading a replacement.
				log.Error(ex, "Failed loading Anoikis cache");
				return false;
			}
		}

		private bool IsCacheExpired()
		{
			try
			{
				if (!File.Exists(CacheFilename))
					return true;

				DateTime lastWrite = File.GetLastWriteTimeUtc(CacheFilename);

				return lastWrite.AddMinutes(RefreshMinutes) < DateTime.UtcNow;
			}
			catch
			{
				return true;
			}
		}

		private void DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				log.Information("Refreshing Anoikis static data");

				string json = DownloadStaticData();

				if (string.IsNullOrWhiteSpace(json))
					return;

				//
				// Parse and validate BEFORE replacing anything.
				//
				AnoikisStaticData data = ParseData(json);

				if (data == null)
				{
					log.Warning("Downloaded Anoikis data was invalid");
					return;
				}

				InstallData(data);

				//
				// Only write a download we know successfully parsed.
				//
				SaveCache(json);

				e.Result = data.Version;
			}
			catch (Exception ex)
			{
				log.Error(ex, "Failed refreshing Anoikis static data");
			}
		}

		private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				log.Error(e.Error, "Anoikis worker failed");
				return;
			}

			if (!IsLoaded)
				return;

			AnoikisLoadedEventHandler?.Invoke(this, new AnoikisLoadedEventArgs()
			{
				Version = Version
			});
		}

		private string DownloadStaticData()
		{
			try
			{
				using (var hc = new HttpClient())
				{
					hc.Timeout = TimeSpan.FromSeconds(30);
					hc.DefaultRequestHeaders.Add("User-Agent", Program.UserAgent);

					var response = hc.GetAsync(StaticDataUrl).GetAwaiter().GetResult();

					if (!response.IsSuccessStatusCode)
					{
						log.Warning($"Anoikis static data returned {(int)response.StatusCode}");
						return null;
					}

					return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex, "Request for Anoikis static data failed");
				return null;
			}
		}

		private AnoikisStaticData ParseData(string json)
		{
			try
			{
				AnoikisStaticData data = JsonConvert.DeserializeObject<AnoikisStaticData>(json);

				if (data == null)
					return null;

				NormaliseData(data);

				//
				// Basic validation so an unexpected response doesn't destroy
				// our known-good in-memory data/cache.
				//
				if (data.Systems == null || data.Systems.Count == 0)
				{
					log.Warning("Anoikis data contains no systems");
					return null;
				}

				if (data.Wormholes == null || data.Wormholes.Count == 0)
				{
					log.Warning("Anoikis data contains no wormholes");
					return null;
				}

				return data;
			}
			catch (Exception ex)
			{
				log.Error(ex, "Failed parsing Anoikis static data");
				return null;
			}
		}

		private void InstallData(AnoikisStaticData data)
		{
			Dictionary<string, List<AnoikisWormhole>> wormholesBySource = BuildWormholeSourceIndex(data);

			lock (_lock)
			{
				_data = data;
				_wormholesBySource = wormholesBySource;
			}
		}

		private void SaveCache(string json)
		{
			string tempFilename = CacheFilename + ".tmp";

			try
			{
				//
				// Write the new data first so we don't destroy the existing
				// cache if the write fails halfway through.
				//
				File.WriteAllText(tempFilename, json);

				if (File.Exists(CacheFilename))
				{
					File.Delete(CacheFilename);
				}

				File.Move(tempFilename, CacheFilename);

				log.Information("Anoikis cache updated");
			}
			catch (Exception ex)
			{
				log.Error(ex, "Failed saving Anoikis cache");

				try
				{
					if (File.Exists(tempFilename))
					{
						File.Delete(tempFilename);
					}
				}
				catch
				{
				}
			}
		}

		private static void NormaliseData(AnoikisStaticData data)
		{
			if (data.Systems == null)
			{
				data.Systems = new Dictionary<string, AnoikisSystemInfo>();
			}

			if (data.Wormholes == null)
			{
				data.Wormholes = new Dictionary<string, AnoikisWormhole>();
			}

			if (data.WormholeClasses == null)
			{
				data.WormholeClasses = new Dictionary<string, AnoikisWormholeClass>();
			}

			if (data.Regions == null)
			{
				data.Regions = new Dictionary<long, string>();
			}

			if (data.Constellations == null)
			{
				data.Constellations = new Dictionary<long, string>();
			}

			foreach (var pair in data.Systems)
			{
				AnoikisSystemInfo system = pair.Value;

				if (system == null)
					continue;

				if (string.IsNullOrWhiteSpace(system.SolarSystemName))
				{
					system.SolarSystemName = pair.Key;
				}

				if (system.Statics == null)
				{
					system.Statics = new List<string>();
				}

				if (system.Celestials == null)
				{
					system.Celestials = new List<AnoikisCelestialDefinition>();
				}
			}

			foreach (var pair in data.Wormholes)
			{
				AnoikisWormhole wormhole = pair.Value;

				if (wormhole == null)
					continue;

				if (string.IsNullOrWhiteSpace(wormhole.Name))
				{
					wormhole.Name = pair.Key;
				}

				if (wormhole.Source == null)
				{
					wormhole.Source = new List<string>();
				}
			}

			foreach (var pair in data.WormholeClasses)
			{
				AnoikisWormholeClass wormholeClass = pair.Value;

				if (wormholeClass == null)
					continue;

				if (string.IsNullOrWhiteSpace(wormholeClass.Name))
				{
					wormholeClass.Name = pair.Key;
				}
			}
		}

		private static Dictionary<string, List<AnoikisWormhole>> BuildWormholeSourceIndex(AnoikisStaticData data)
		{
			var result = new Dictionary<string, List<AnoikisWormhole>>(StringComparer.OrdinalIgnoreCase);

			foreach (AnoikisWormhole wormhole in data.Wormholes.Values)
			{
				if (wormhole == null || wormhole.Source == null)
					continue;

				foreach (string source in wormhole.Source)
				{
					if (string.IsNullOrWhiteSpace(source))
						continue;

					List<AnoikisWormhole> list;

					if (!result.TryGetValue(source, out list))
					{
						list = new List<AnoikisWormhole>();
						result[source] = list;
					}

					list.Add(wormhole);
				}
			}

			return result;
		}

		public AnoikisSystemInfo GetSystem(string systemName)
		{
			if (string.IsNullOrWhiteSpace(systemName))
				return null;

			lock (_lock)
			{
				if (_data == null)
					return null;

				AnoikisSystemInfo system;

				if (_data.Systems.TryGetValue(systemName, out system))
				{
					return system;
				}

				return _data.Systems
					.Where(x => string.Equals(x.Key, systemName, StringComparison.OrdinalIgnoreCase))
					.Select(x => x.Value)
					.FirstOrDefault();
			}
		}

		public AnoikisSystemInfo GetSystem(long solarSystemID)
		{
			lock (_lock)
			{
				if (_data == null)
					return null;

				return _data.Systems.Values.FirstOrDefault(x => x != null && x.SolarSystemID == solarSystemID);
			}
		}

		public AnoikisWormhole GetWormhole(string wormholeName)
		{
			if (string.IsNullOrWhiteSpace(wormholeName))
				return null;

			lock (_lock)
			{
				if (_data == null)
					return null;

				AnoikisWormhole wormhole;

				if (_data.Wormholes.TryGetValue(wormholeName, out wormhole))
				{
					return wormhole;
				}

				return _data.Wormholes
					.Where(x => string.Equals(x.Key, wormholeName, StringComparison.OrdinalIgnoreCase))
					.Select(x => x.Value)
					.FirstOrDefault();
			}
		}

		public IReadOnlyList<AnoikisWormhole> GetStatics(AnoikisSystemInfo system)
		{
			var result = new List<AnoikisWormhole>();

			if (system == null || system.Statics == null)
				return result;

			foreach (string wormholeName in system.Statics)
			{
				AnoikisWormhole wormhole = GetWormhole(wormholeName);

				if (wormhole != null)
				{
					result.Add(wormhole);
				}
			}

			return result;
		}

		public IReadOnlyList<AnoikisWormhole> GetPossibleWanderers(AnoikisSystemInfo system)
		{
			if (system == null || string.IsNullOrWhiteSpace(system.WormholeClass))
				return new List<AnoikisWormhole>();

			lock (_lock)
			{
				List<AnoikisWormhole> wormholes;

				if (!_wormholesBySource.TryGetValue(system.WormholeClass, out wormholes))
					return new List<AnoikisWormhole>();

				return wormholes.Where(x => x.Static.HasValue && x.Static.Value == false).ToList();
			}
		}

		public AnoikisWormholeClass GetWormholeClass(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;

			lock (_lock)
			{
				if (_data == null)
					return null;

				AnoikisWormholeClass wormholeClass;

				if (_data.WormholeClasses.TryGetValue(name, out wormholeClass))
				{
					return wormholeClass;
				}

				return null;
			}
		}

		public IEnumerable<AnoikisSystemInfo> GetSystems()
		{
			lock (_lock)
			{
				if (_data == null)
					return Enumerable.Empty<AnoikisSystemInfo>();

				return _data.Systems.Values.ToList();
			}
		}
	}
}