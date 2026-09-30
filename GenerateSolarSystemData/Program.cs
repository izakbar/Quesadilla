using EVEStandard.Models;
using GenerateSolarSystemData.Models.SDE;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Quesadilla;
using Quesadilla.Classes;
using Quesadilla.Classes.SolarSystems;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace QuesadillaExport
{
    internal class Program
    {
		static void GenerateUniHoleData()
        {
			Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
			StreamReader reader = new StreamReader("Quesadilla.Systems.json");
			var json = reader.ReadToEnd();
			SolarSystems = JsonSerializer.Deserialize<Dictionary<int, SolarSystem>>(json);
			reader.Close();

            StringBuilder UniHoles = new StringBuilder();
			using (StreamReader drifterReader = new StreamReader("UniHoleData/RegionData.html"))
			{
				while (drifterReader.Peek() >= 0)
				{
					var drifterLine = drifterReader.ReadLine();
                    if (drifterLine.StartsWith("<tr>") &&
                        drifterLine.Contains("https://evemaps.dotlan.net/map/") )
                    {

						var dl = drifterLine.Split('"').
							Where(o => o.Contains("https://evemaps.dotlan.net/map/")).ToList();
						;

                        if (dl.Count > 1)
                        {

                            var info = dl[1].Split('/');

                            var region = info[4];
                            var systems = info[5].Split("#")[0].Split(',');

                            foreach (var system in systems)
                            {
                                var s = SolarSystems.FirstOrDefault(s => s.Value.Name == system.Replace("_", " "));
                                s.Value.UniHole = true;

                                UniHoles.AppendLine($"INSERT IGNORE INTO MapUpgrades(SystemId, UpgradeType, UpgradeText) VALUES('{s.Value.Id}','UniHole','');");

                            }
                        }

					}
				}
			}
			json = JsonSerializer.Serialize(SolarSystems);
			File.WriteAllText(@"Quesadilla.Systems.json", json);
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"UniHoleData\UniHoles.txt"))
			{
				file.WriteLine(UniHoles.ToString());
			}

		}

        static void GenerateSolarSystemsDataFromSDEJson()
		{
			Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
            Dictionary<int, string> Regions = new Dictionary<int, string>();

            // read region data
            StreamReader readerRegions = new StreamReader("SDE/mapRegions.jsonl");
            string json;
            while ((json = readerRegions.ReadLine()) != null)
            {
                var regionData = JsonSerializer.Deserialize<mapRegion>(json);
                Regions.Add(regionData._key, regionData.name.en);
            }
            readerRegions.Close();

			// read solarsystems
			StreamReader readerSolarsystems = new StreamReader("SDE/mapSolarsystems.jsonl");
			while ((json = readerSolarsystems.ReadLine()) != null)
			{

				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				
				var Solarsystem = JsonSerializer.Deserialize<mapSolarsystem>(json, options);

                var ss = new SolarSystem()
				{
					Id = Solarsystem._key,
					Name = Solarsystem.name.en,
					RegionName = Regions[Solarsystem.regionID],
					X = Solarsystem.position.X,
					Y = Solarsystem.position.Y,
					Z = Solarsystem.position.Z,
					Security = Solarsystem.securityStatus,
					ConnectedTo = new List<SolarSystemConnectionData>(),
				};

                if (Solarsystem.position2D != null)
                {
                    ss.X2D = 0- Solarsystem.position2D.X;
                    ss.Y2D = Solarsystem.position2D.Y;
                }

				SolarSystems.Add(Solarsystem._key, ss);
			}
			readerSolarsystems.Close();



			// read types data
			Dictionary<int, ItemType> ItemTypes = new Dictionary<int, ItemType>();
			StreamReader readerTypes = new StreamReader("SDE/types.jsonl");
			while ((json = readerTypes.ReadLine()) != null)
			{
				var typeData = JsonSerializer.Deserialize<type>(json);
				ItemTypes.Add(typeData._key,
					new ItemType()
					{
						Name = typeData.name.en,
						GroupId = typeData.groupID,
						Id = typeData._key,
						// Group = Groups.ContainsKey(typeData.groupID) ? Groups[typeData.groupID].name.en : "Unknown"
					});
			}
			readerTypes.Close();


			// Read planets
			StreamReader readerPlanets = new StreamReader("SDE/mapPlanets.jsonl");
            while ((json = readerPlanets.ReadLine()) != null)
            {

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var planet = JsonSerializer.Deserialize<mapPlanet>(json, options);
                var solarSystem = SolarSystems[planet.solarSystemID];
                if ( solarSystem.Planets == null)
				{
					solarSystem.Planets = new List<PlanetInfo>();
				}

				solarSystem.Planets.Add(new PlanetInfo()
                {
                    PlanetId = planet._key,
                    PlanetTypeId = planet.typeID,
                    PlanetTypeName = ItemTypes.FirstOrDefault(i => i.Value.Id == planet.typeID).Value?.Name ?? "Unknown",
                    PlanetName = planet.celestialIndex.ToString()
                });
			}

			var Outputjson = JsonSerializer.Serialize(SolarSystems);
			File.WriteAllText(@"Quesadilla.Systems.json", Outputjson);

		}
        static void GenerateSolarSystemDataFromDotlan()
        {
            Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
            StreamReader reader = new StreamReader("Quesadilla.Systems.json");
            var json = reader.ReadToEnd();
            SolarSystems = JsonSerializer.Deserialize<Dictionary<int, SolarSystem>>(json);
            reader.Close();

            XmlDocument xmldoc = new XmlDocument
            {
                XmlResolver = null
            };
            string previousRegion = "";
            bool dotlanRegionDataLoaded = false;
            XmlNodeList dotlanNodes = null;

            foreach (var solarSystem in SolarSystems)
            {
                if (previousRegion != solarSystem.Value.RegionName)
                {
                    // load dotlan svg for this region
                    previousRegion = solarSystem.Value.RegionName;
                    var dotlanRegionName = solarSystem.Value.RegionName.Replace(" ", "_");
                    try
                    {
                        FileStream fs = new FileStream($"dotlan_data\\{dotlanRegionName}.svg", FileMode.Open, FileAccess.Read);
                        xmldoc.Load(fs);
                        dotlanNodes = xmldoc.SelectNodes(@"//*[@id='sysuse']/*");
                        dotlanRegionDataLoaded |= true;
                    }
                    catch (Exception ex)
                    {
                        dotlanRegionDataLoaded |= false;
                    }
                }

                double dotlanX = 0;
                double dotlanY = 0;
                string link = "";

                if (dotlanRegionDataLoaded)
                {
                    foreach (XmlNode xn in dotlanNodes)
                    {
                        long systemID = long.Parse(xn.Attributes["id"].Value.Replace("sys", ""));
                        float x = float.Parse(xn.Attributes["x"].Value);
                        float y = float.Parse(xn.Attributes["y"].Value);
                        if (systemID == solarSystem.Value.Id)
                        {
                            dotlanX = x;
                            dotlanY = y;
                            link = xn.Attributes["xlink:href"].Value.Replace("#", "");
                        }
                    }
                }

                bool IceField = false;

                XmlNodeList iceFieldNode = xmldoc.SelectNodes($@"//*[@id='{link}']/*/*[@id='{link.Replace("def", "ice")}']");
                if (iceFieldNode != null && iceFieldNode.Count > 0)
                {
                    IceField = true;
                }

                solarSystem.Value.IceField = IceField;
                solarSystem.Value.DotlanX = dotlanX;
                solarSystem.Value.DotlanY = dotlanY;
            }

            var Outputjson = JsonSerializer.Serialize(SolarSystems);
            File.WriteAllText(@"Quesadilla.Systems.json", Outputjson);
        }

		static void GenerateSolarSystemConnectionsFromSDEJson()
		{
			string json;

			Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
			StreamReader reader = new StreamReader("Quesadilla.Systems.json");
			json = reader.ReadToEnd();
			SolarSystems = JsonSerializer.Deserialize<Dictionary<int, SolarSystem>>(json);
			reader.Close();

			// read group data
			Dictionary<int, mapStargate> solarSystemJumps = new Dictionary<int, mapStargate>();
			StreamReader readerMapStargates = new StreamReader("SDE/mapStargates.jsonl");
			while ((json = readerMapStargates.ReadLine()) != null)
			{
				var stargateData = JsonSerializer.Deserialize<mapStargate>(json);
				solarSystemJumps.Add(stargateData._key, stargateData);
			}
			readerMapStargates.Close();

			// todo add regions into above

			foreach (var jump in solarSystemJumps)
			{
				if (!SolarSystems[jump.Value.solarSystemID].ConnectedTo.Any(x => x.ToSystemId == jump.Value.destination.solarSystemID))
				{
					SolarSystems[jump.Value.solarSystemID].ConnectedTo.Add(new SolarSystemConnectionData()
					{
						ToSystemId = jump.Value.destination.solarSystemID,
						IsRegional = SolarSystems[jump.Value.solarSystemID].RegionName == SolarSystems[jump.Value.destination.solarSystemID].RegionName ? false : true,
					});
				}
			}

			// sort destinations 
			foreach (var solarSystem in SolarSystems)
			{
				solarSystem.Value.ConnectedTo.Sort((a, b) => a.ToSystemId.CompareTo(b.ToSystemId));
				if (!solarSystem.Value.ConnectedTo.Any() || solarSystem.Value.Name.Equals("Thera") || solarSystem.Value.Name.Equals("Turnur"))
				{
					solarSystem.Value.IsWormhole = true;
				}
			}

			var Outputjson = JsonSerializer.Serialize(SolarSystems);
			File.WriteAllText(@"Quesadilla.Systems.json", Outputjson);
		}

		static void GenerateItemTypesDataFromSDEJson()
		{
			string json;

			// read group data
			Dictionary<int, group> Groups = new Dictionary<int, group>();
			StreamReader readerGroups = new StreamReader("SDE/groups.jsonl");
			while ((json = readerGroups.ReadLine()) != null)
			{
				var groupData = JsonSerializer.Deserialize<group>(json);
				Groups.Add(groupData._key, groupData);
			}
			readerGroups.Close();

			// read types data
			Dictionary<int, ItemType> ItemTypes = new Dictionary<int, ItemType>();
			StreamReader readerTypes = new StreamReader("SDE/types.jsonl");
			while ((json = readerTypes.ReadLine()) != null)
			{
				var typeData = JsonSerializer.Deserialize<type>(json);
				ItemTypes.Add(typeData._key,
					new ItemType()
					{
						Name = typeData.name.en,
						GroupId = typeData.groupID,
						Id = typeData._key,
                        Group = Groups.ContainsKey(typeData.groupID) ? Groups[typeData.groupID].name.en : "Unknown"
					});
			}
			readerTypes.Close();

			var jsonOut = JsonSerializer.Serialize(ItemTypes);
			File.WriteAllText(@"Quesadilla.ItemTypes.json", jsonOut);
		}

		static void Main(string[] args)
        {

            // Build a config object, using env vars and JSON providers.
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("GenerateSolarSystemData.json")
                .AddEnvironmentVariables()
                .Build();

            using var Log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

			GenerateItemTypesDataFromSDEJson(); 

			GenerateSolarSystemsDataFromSDEJson();
            GenerateSolarSystemDataFromDotlan();

			GenerateSolarSystemConnectionsFromSDEJson();

			GenerateUniHoleData();
		}
	}
}
