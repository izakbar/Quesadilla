using EVEStandard;
using EVEStandard.Enumerations;
using Microsoft.IdentityModel.Tokens;
using Quesadilla.Classes.ESIData;
using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.Queues;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quesadilla.Classes.SolarSystems
{
    public class UniverseManager
    {
        private readonly EVEStandardAPI _esiClient;
        private readonly ILogger _log;

        private const long LOWEST_WH_SYSTEM_ID = 31000000;

        public Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
		public List<EveScoutStorm> Storms = new List<EveScoutStorm>();
		public Dictionary<string, int> Names = new Dictionary<string, int>();
        public Dictionary<int, ItemType> ItemTypes = new Dictionary<int, ItemType>();

        public ConcurrentDictionary<int, ESICharacter> ESICharacters = new ConcurrentDictionary<int, ESICharacter>();

        private ConcurrentDictionary<string, PathInfo> _pathfindingCache = new ConcurrentDictionary<string, PathInfo>();

        public Dictionary<string, DateTime> DotlanMapRegions = new Dictionary<string, DateTime>();
        public DateTime DotlanDeltaRegions;

		public double MetersPerLightYear = 9.4607304725808e15;

        public int HomeSolarSystemID { get; set; }

        public UniverseManager(EVEStandardAPI esiClient, ILogger Log)
        {
            _esiClient = esiClient;
            _log = Log;
        }

        public void ReadSolarSystems(string jsonFile)
        {
            using (StreamReader reader = new StreamReader(jsonFile))
            {
                var json = reader.ReadToEnd();

                SolarSystems = JsonSerializer.Deserialize<Dictionary<int, SolarSystem>>(json);
                DotlanDeltaRegions = DateTime.Now.AddDays(-10);
                foreach (var s in SolarSystems)
                {
                    Names.Add(s.Value.Name, s.Key);
                    s.Value.SetNameRegex();

                    if (!DotlanMapRegions.ContainsKey(s.Value.RegionName))
                    {
                        DotlanMapRegions.Add(s.Value.RegionName, DateTime.Now.AddDays(-10));
                    }
                }
            }
        }
        public void ReadItemTypes(string jsonFile)
        {
			var json = File.ReadAllText(jsonFile);
			ItemTypes = JsonSerializer.Deserialize<Dictionary<int, ItemType>>(json);
		}
		public void ReadESICharacters(string jsonFile)
        {
            try
            {
                if (File.Exists(jsonFile))
                {
                    using (StreamReader reader = new StreamReader(jsonFile))
                    {
                        var json = reader.ReadToEnd();
                        ESICharacters = JsonSerializer.Deserialize<ConcurrentDictionary<int, ESICharacter>>(json);
                    }                }
            }
            catch (Exception ex)
            {
                _log.Error("Error reading character cache");
            }
        }
        public void WriteESICharacters(string jsonFile)
        {
            try
            {
                var json = JsonSerializer.Serialize(ESICharacters);
                File.WriteAllText(jsonFile, json);
            }
            catch 
            {

            }
        }

        public PathInfo FindAndCachePath(int fromSystemId, int toSystemId)
        {

            if ( fromSystemId >= LOWEST_WH_SYSTEM_ID || toSystemId >= LOWEST_WH_SYSTEM_ID || fromSystemId == -1 || toSystemId == -1) return null;

            PathInfo pathInfo = new PathInfo()
            {
                FromSystemId = fromSystemId,
                ToSystemId = toSystemId
            };
			// don't do any work if path already exists.
			if (_pathfindingCache.TryGetValue(pathInfo.PathId, out var cachedPath))
			{
				return cachedPath;
			}
			_log.Debug("Pathid " + pathInfo.PathId + " does not exist - building");
            if (fromSystemId == 0) return (null); 
            SolarSystem fromSystem = SolarSystems[fromSystemId];
            SolarSystem toSystem = SolarSystems[toSystemId];
            SolarSystemNode fromSystemNode = new SolarSystemNode()
            {
                Id = fromSystem.Id,
                Name = fromSystem.Name,
                ConnectedTo = fromSystem.ConnectedTo,
                PathData = new PathInfo()
                {
                    FromSystemId = fromSystemId,
                    ToSystemId = fromSystemId,
                    FromSystemName = fromSystem.Name,
                    ToSystemName = toSystem.Name,
                    ToSystemRegionName = toSystem.RegionName,
                    PathSystems = new List<int>() { fromSystemId }
                }
            };

            bool toSystemFound = false;
            Dictionary<int, bool> usedSystems = new Dictionary<int, bool>();
            HeapPriorityQueue<SolarSystemNode> systemsToCheck = new HeapPriorityQueue<SolarSystemNode>(SolarSystems.Count);
            
            PathInfo FoundPath = null;

            systemsToCheck.Enqueue(fromSystemNode,1);
            usedSystems[fromSystemNode.Id] = true;

            while (toSystemFound == false && systemsToCheck.Count > 0)
            {
                var checkingSystem = systemsToCheck.Dequeue();
                if (checkingSystem.Id == toSystemId)
                {
                    toSystemFound = true;
                    FoundPath = checkingSystem.PathData;
                }
                else
                {
                    // go through connections and add
                    foreach (var connection in checkingSystem.ConnectedTo)
                    {
                        SolarSystem connectionSystem = SolarSystems[connection.ToSystemId];
                        SolarSystemNode connectionSystemNode = new SolarSystemNode()
                        {
                            Id = connectionSystem.Id,
                            Name = connectionSystem.Name,
                            ConnectedTo = connectionSystem.ConnectedTo,
                            PathData = new PathInfo()
                            {
                                FromSystemId = fromSystemId,
                                ToSystemId = connection.ToSystemId,
                                FromSystemName = fromSystem.Name,
                                ToSystemName = SolarSystems[connection.ToSystemId].Name,
                                ToSystemRegionName = SolarSystems[connection.ToSystemId].RegionName,
                                PathSystems = new List<int>(checkingSystem.PathData.PathSystems),
                            }
                        };
                        connectionSystemNode.PathData.PathSystems.Add(connection.ToSystemId);

                        if (connection.ToSystemId == toSystemId)
                        {
                            toSystemFound = true;
                            FoundPath = connectionSystemNode.PathData;
                        }
                        else
                        {
                            // only enqueue if never checked previous ! don't want infinite loops
                            if (!usedSystems.ContainsKey(connectionSystemNode.Id))
                            {
                                systemsToCheck.Enqueue(connectionSystemNode, connectionSystemNode.PathData.PathSystems.Count);
                                usedSystems[connectionSystemNode.Id] = true;
                            }
                        }
                    }
                }
            }
            if (FoundPath == null)
            {
                _log.Debug("Unable to find path");
                return null;
            }

			_log.Debug("Found path");
			_pathfindingCache.TryAdd(FoundPath.PathId, FoundPath);

			return (FoundPath);
		}

		public string GetCharacterName(int characterId)
		{
			if (characterId == 0) return "*NO ONE*";

			ESICharacter character;
			if (ESICharacters.TryGetValue(characterId, out character))
			{
				return character.CharacterName;
			}

			var characterInfo = _esiClient.Character.GetCharacterPublicInfoAsync(characterId).GetAwaiter().GetResult();
			ESICharacters.TryAdd(characterId, new ESICharacter
			{
				CharacterId = characterId,
				CharacterName = characterInfo.Model.Name
			});
			return characterInfo.Model.Name;
		}

		public string PathToString(PathInfo path)
        {
            string route = "";
            if (path != null)
            {
                foreach (int systemId in path.PathSystems)
                {
                    if (route.Length > 0) route += " -> ";
                    route += systemId == path.FromSystemId ? "" : Program.Universe.SolarSystems[systemId].Name;
                }
            }
            return route;
        }

        public void AddAlert(int systemId, AlertQueueItem alert)
        {
            SolarSystems[systemId].SetAlertTime(DateTime.Now);

            if (!alert.isFaction && !alert.isDecloak && alert.Kill == null && alert.Path != null)
            {
                // add alert to solarsystem info
                SolarSystems[systemId].AddAlert(alert);
            }
        }

        public List<MinimapSystem> MouseOverSystem(int X, int Y, List<MinimapSystem> mSystems)
        {
            List<MinimapSystem> returnSystems = new List<MinimapSystem>();
            const int tolerance = 3;

            foreach(var m in mSystems)
            {
                if (
                    (m.X >= X - tolerance && m.X <= X + tolerance) &&
                    (m.Y >= Y - tolerance && m.Y <= Y + tolerance) )
                {
                    returnSystems.Add(m);
                }
            }

            return returnSystems;
        }

		public List<int> FindConnections(int systemId, int jumps)
		{
			return FindConnections(systemId, jumps, new HashSet<int> { systemId });
		}

		private List<int> FindConnections(int systemId, int jumps, HashSet<int> visited)
		{
			HashSet<int> data = new HashSet<int>();

			foreach (var system in SolarSystems[systemId].ConnectedTo)
			{
				if (!visited.Add(system.ToSystemId))
				{
					continue;
				}

				data.Add(system.ToSystemId);

				if (jumps > 1)
				{
					foreach (var sys in FindConnections(system.ToSystemId, jumps - 1, visited))
					{
						data.Add(sys);
					}
				}
			}

			return data.ToList();
		}

		public void GetSystemInfo(int systemid)
		{
			// check if we need to pull data from ESI for system (planets/moons/belts)
			if (Program.Universe.SolarSystems[systemid].Planets == null)
			{
				Program.ESIHandler.EnqueueRequestSystem(systemid);
			}
		}

	}
}
