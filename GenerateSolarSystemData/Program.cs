using GenerateSolarSystemData.Models.SDE;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Quesadilla;
using Quesadilla.Classes;
using Quesadilla.Classes.SolarSystems;
using QuesadillaExport.DataAccess;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                Regions.Add(regionData._key, regionData.Name.en);
            }
            readerRegions.Close();

			// read solarsystems
			StreamReader readerSolarsystems = new StreamReader("SDE/mapSolarsystems.jsonl");
			while ((json = readerSolarsystems.ReadLine()) != null)
			{
				var Solarsystem = JsonSerializer.Deserialize<mapSolarsystem>(json);

                var ss = new SolarSystem()
				{
					Id = Solarsystem._key,
					Name = Solarsystem.Name.en,
					RegionName = Regions[Solarsystem.regionID],
					X = Solarsystem.Position.X,
					Y = Solarsystem.Position.Y,
					Z = Solarsystem.Position.Z,
					Security = Solarsystem.securityStatus,
					ConnectedTo = new List<SolarSystemConnectionData>(),
				};

                if (Solarsystem.Position2D != null)
                {
                    ss.X2D = 0- Solarsystem.Position2D.X;
                    ss.Y2D = Solarsystem.Position2D.Y;
                }

				SolarSystems.Add(Solarsystem._key, ss);
			}
			readerSolarsystems.Close();


			var Outputjson = JsonSerializer.Serialize(SolarSystems);
			File.WriteAllText(@"Quesadilla.Systems.json", Outputjson);

		}

		static void GenerateSolarSystemDataFromDB()
        {
            var solarSystems = DataAccess.mapSolarSystems.SelectAll();
            Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();

            XmlDocument xmldoc = new XmlDocument
            {
                XmlResolver = null
            };
            string previousRegion = "";
            bool dotlanRegionDataLoaded = false;
            XmlNodeList dotlanNodes = null;

            foreach (var solarSystem in solarSystems)
            {
                if (previousRegion != solarSystem.RegionName)
                {
                    // load dotlan svg for this region
                    previousRegion = solarSystem.RegionName;
                    var dotlanRegionName = solarSystem.RegionName.Replace(" ", "_");
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
                        long systemID = long.Parse(xn.Attributes["id"].Value.Replace("sys",""));
                        float x = float.Parse(xn.Attributes["x"].Value);
                        float y = float.Parse(xn.Attributes["y"].Value);
                        if (systemID == solarSystem.solarSystemId)
                        {
                            dotlanX = x;
                            dotlanY = y;
                            link = xn.Attributes["xlink:href"].Value.Replace("#", "");
                        }
                    }
                }

                bool IceField = false;

                XmlNodeList iceFieldNode = xmldoc.SelectNodes($@"//*[@id='{link}']/*/*[@id='{link.Replace("def", "ice")}']");
                if (iceFieldNode != null && iceFieldNode.Count > 0 )
                {
                    IceField = true;
                }


                SolarSystems.Add(solarSystem.solarSystemId,
                new SolarSystem()
                {
                    Name = solarSystem.solarSystemname,
                    RegionName = solarSystem.RegionName,
                    X = solarSystem.X,
                    Y = solarSystem.Y,
                    Z = solarSystem.Z,
                    DotlanX = dotlanX,
                    DotlanY = dotlanY,
                    Id = solarSystem.solarSystemId,
                    Incursion = false,
                    IceField = IceField,
                    Security = solarSystem.Security,
                    ConnectedTo = new List<SolarSystemConnectionData>(),
                }
            );
            }

            var solarSystemJumps = DataAccess.mapSolarSystemJumps.SelectAll();
            foreach (var jump in solarSystemJumps)
            {
                SolarSystems[jump.fromSolarSystemId].ConnectedTo.Add(new SolarSystemConnectionData()
                {
                    ToSystemId = jump.toSolarSystemId,
                    IsRegional = jump.fromRegionId == jump.toRegionId ? false : true,
                });
            }
            var json = JsonSerializer.Serialize(SolarSystems);
            File.WriteAllText(@"Quesadilla.Systems.json", json);
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

        static void GenerateSolarSystemConnectionsFromDB()
        {

			Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
			StreamReader reader = new StreamReader("Quesadilla.Systems.json");
			var json = reader.ReadToEnd();
			SolarSystems = JsonSerializer.Deserialize<Dictionary<int, SolarSystem>>(json);
			reader.Close();

			var solarSystemJumps = DataAccess.mapSolarSystemJumps.SelectAll();
			foreach (var jump in solarSystemJumps)
			{
				SolarSystems[jump.fromSolarSystemId].ConnectedTo.Add(new SolarSystemConnectionData()
				{
					ToSystemId = jump.toSolarSystemId,
					IsRegional = jump.fromRegionId == jump.toRegionId ? false : true,
				});
			}

			var Outputjson = JsonSerializer.Serialize(SolarSystems);
			File.WriteAllText(@"Quesadilla.Systems.json", Outputjson);

		}

		static void GenerateItemTypesData()
        {
            var itemTypes = DataAccess.invTypes.SelectAll();
            Dictionary<int, ItemType> ItemTypes = new Dictionary<int, ItemType>();
            foreach (var item in itemTypes)
            {
                ItemTypes.Add(item.TypeId,
                new ItemType()
                {
                    Name = item.TypeName,
                    Group = item.GroupName,
                    Id = item.TypeId,
                });
            }
            var json = JsonSerializer.Serialize(ItemTypes);
            File.WriteAllText(@"Quesadilla.ItemTypes.json", json);
        }


        static void Main(string[] args)
        {

            // Build a config object, using env vars and JSON providers.
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("GenerateSolarSystemData.json")
                .AddEnvironmentVariables()
                .Build();

            QuesadillaExport.DataAccess.DataAccess.ConnectionString = Configuration.GetConnectionString("db");

            using var Log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            //GenerateSolarSystemDataFromDB();
            GenerateItemTypesData();
            GenerateSolarSystemsDataFromSDEJson();
            GenerateSolarSystemDataFromDotlan();
            GenerateSolarSystemConnectionsFromDB();
			GenerateUniHoleData();
		}
	}
}
