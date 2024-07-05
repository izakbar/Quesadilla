using EVEStandard.Enumerations;
using EVEStandard;
using Newtonsoft.Json;
using Quesadilla.Classes.ESIData;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Quesadilla.Classes.Queues;
using System.Windows.Forms;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Quesadilla.Classes.SolarSystems
{
    public class UniverseManager
    {
        public EVEStandardAPI esiClient;

        private const long LOWEST_WH_SYSTEM_ID = 31000000;
        private ILogger logger = Log.ForContext<UniverseManager>();

        public Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
        public Dictionary<string, int> Names = new Dictionary<string, int>();
        public Dictionary<int, ItemType> ItemTypes = new Dictionary<int, ItemType>();

        public Dictionary<int, ESICharacter> ESICharacters = new Dictionary<int, ESICharacter>();

        private Dictionary<string, PathInfo> _pathfindingCache = new Dictionary<string, PathInfo>();

        public Dictionary<string, DateTime> DotlanMapRegions = new Dictionary<string, DateTime>();
        public DateTime DotlanDeltaRegions;

        public int HomeSolarSystemID { get; set; }

        public void ReadSolarSystems(string jsonFile)
        {
            StreamReader reader = new StreamReader(jsonFile);
            var json = reader.ReadToEnd();

            SolarSystems = JsonConvert.DeserializeObject<Dictionary<int, SolarSystem>>(json);
            DotlanDeltaRegions = DateTime.Now.AddDays(-10);
            foreach (var s in SolarSystems)
            {
                Names.Add(s.Value.Name, s.Key);
                s.Value.SetNameRegex();

                if ( ! DotlanMapRegions.ContainsKey(s.Value.RegionName) )
                {
                    DotlanMapRegions.Add(s.Value.RegionName, DateTime.Now.AddDays(-10));
                }
            }
            reader.Close();
        }
        public void ReadItemTypes(string jsonFile)
        {
            StreamReader reader = new StreamReader(jsonFile);
            var json = reader.ReadToEnd();

            ItemTypes = JsonConvert.DeserializeObject<Dictionary<int, ItemType>>(json);
        }
        public void ReadESICharacters(string jsonFile)
        {
            if (File.Exists(jsonFile))
            {
                StreamReader reader = new StreamReader(jsonFile);
                var json = reader.ReadToEnd();
                ESICharacters = JsonConvert.DeserializeObject<Dictionary<int, ESICharacter>>(json);
            }
        }
        public void WriteESICharacters(string jsonFile)
        {
            var json = JsonConvert.SerializeObject(ESICharacters);
            File.WriteAllText(jsonFile, json);
        }

        public void SetupESI()
        {
            esiClient = new EVEStandardAPI(
                    "IzaWorks",                  // User agent
                    DataSource.Tranquility,         // Server [Tranquility/Singularity]
                    TimeSpan.FromSeconds(30));       // Timeout
        }

        public PathInfo FindAndCachePath(int fromSystemId, int toSystemId)
        {

            if ( fromSystemId >= LOWEST_WH_SYSTEM_ID || toSystemId >= LOWEST_WH_SYSTEM_ID) return null;

            PathInfo pathInfo = new PathInfo()
            {
                FromSystemId = fromSystemId,
                ToSystemId = toSystemId
            };
            // don't do any work if path already exists.
            if (_pathfindingCache.ContainsKey(pathInfo.PathId))
            {
                Log.Debug("Pathid " + pathInfo.PathId + " already exists");
                return (_pathfindingCache[pathInfo.PathId]);
            }

            Log.Debug("Pathid " + pathInfo.PathId + " does not exist - building");
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
                Log.Debug("Unable to find path");
                return null;
            }

            Log.Debug("Found path");
            _pathfindingCache.Add(FoundPath.PathId, FoundPath);
            return (FoundPath);
        }

        public string GetCharacterName(int characterId)
        {
            if ( ESICharacters.ContainsKey(characterId) ) 
            {
                return ESICharacters[characterId].CharacterName;
            }
            if ( characterId == 0 )
            {
                return "*NO ONE*";
            }
            try
            {
                var characterInfo = esiClient.Character.GetCharacterPublicInfoV5Async(characterId).Result;
                ESICharacters.Add(characterId, new ESICharacter()
                {
                    CharacterId = characterId,
                    CharacterName = characterInfo.Model.Name,
                });
                return characterInfo.Model.Name;
            }
            catch ( Exception ex) { 
                Log.Error(ex.Message); 
                if ( ex.InnerException != null )
                {
                    Log.Error(ex.InnerException.Message);
                }
                return "**ESIERROR**";
            }
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
            SolarSystems[systemId].AlertTime = DateTime.Now;

            if (!alert.isFaction && !alert.isDecloak && alert.Kill == null && alert.Path != null)
            {
                // add alert to solarsystem info
                if (SolarSystems[systemId].Alerts == null) SolarSystems[systemId].Alerts = new List<AlertQueueItem>();
                SolarSystems[systemId].Alerts.Add(alert);
            }
        }

        public List<MinimapSystem> MouseOverSystem(int X, int Y, List<MinimapSystem> mSystems)
        {
            List<MinimapSystem> returnSystems = new List<MinimapSystem>();
            const int tolerance = 2;

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

    }
}
