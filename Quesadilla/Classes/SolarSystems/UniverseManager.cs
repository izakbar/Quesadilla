using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.SolarSystems
{
    public class UniverseManager
    {

        private const long LOWEST_WH_SYSTEM_ID = 31000000;
        private ILogger logger = Log.ForContext<UniverseManager>();

        public Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
        public Dictionary<string, int> Names = new Dictionary<string, int>();
        public Dictionary<int, ItemType> ItemTypes = new Dictionary<int, ItemType>();

        private Dictionary<string, PathInfo> _pathfindingCache = new Dictionary<string, PathInfo>();
        public int HomeSolarSystemID { get; set; }

        public void ReadSolarSystems(string jsonFile)
        {
            StreamReader reader = new StreamReader(jsonFile);
            var json = reader.ReadToEnd();

            SolarSystems = JsonConvert.DeserializeObject<Dictionary<int, SolarSystem>>(json);
            foreach (var s in SolarSystems)
            {
                Names.Add(s.Value.Name, s.Key);
                s.Value.SetNameRegex();
            }
        }
        public void ReadItemTypes(string jsonFile)
        {
            StreamReader reader = new StreamReader(jsonFile);
            var json = reader.ReadToEnd();

            ItemTypes = JsonConvert.DeserializeObject<Dictionary<int, ItemType>>(json);
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
                Log.Error("Unable to find path");
                return null;
            }

            Log.Debug("Found path");
            _pathfindingCache.Add(FoundPath.PathId, FoundPath);
            return (FoundPath);
        }

        public void AddAlert(int systemId)
        {
            SolarSystems[systemId].AlertTime = DateTime.Now;
        }

    }
}
