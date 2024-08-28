using Microsoft.Extensions.Configuration;
using QuesadillaExport.DataAccess;
using Quesadilla;
using Serilog;
using Quesadilla.Classes.SolarSystems;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json.Serialization;
using Quesadilla.Classes;
using Newtonsoft.Json;
using System.Xml;

namespace QuesadillaExport
{
    internal class Program
    {

        static void GenerateSolarSystemData()
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
                        }
                    }
                }

                bool IceField = false;

                XmlNodeList iceFieldNode = xmldoc.SelectNodes($@"//*[@id='def{solarSystem.solarSystemId}']/*[@id='ice{solarSystem.solarSystemId}']");
                if (iceFieldNode != null)
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
            var json = JsonConvert.SerializeObject(SolarSystems);
            File.WriteAllText(@"Quesadilla.Systems.json", json);

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
            var json = JsonConvert.SerializeObject(ItemTypes);
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

            GenerateSolarSystemData();
            GenerateItemTypesData();
        }
    }
}
