using Microsoft.Extensions.Configuration;
using QuesadillaExport.DataAccess;
using Quesadilla;
using Serilog;
using Quesadilla.Classes.SolarSystems;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json.Serialization;
using Quesadilla.Classes;
using Newtonsoft.Json;

namespace QuesadillaExport
{
    internal class Program
    {

        static void GenerateSolarSystemData()
        {
            var solarSystems = DataAccess.mapSolarSystems.SelectAll();
            Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();

            foreach (var solarSystem in solarSystems)
            {
                SolarSystems.Add(solarSystem.solarSystemId,
                    new SolarSystem()
                    {
                        Name = solarSystem.solarSystemname,
                        RegionName = solarSystem.RegionName,
                        X = solarSystem.X,
                        Y = solarSystem.Y,
                        Z = solarSystem.Z,
                        Id = solarSystem.solarSystemId,
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

        static void GenerateShiptypesData()
        {
            var shipTypes = DataAccess.invTypes.SelectAll(6);
            Dictionary<int, ShipType> ShipTypes = new Dictionary<int, ShipType>();
            foreach (var ship in shipTypes)
            {
                ShipTypes.Add(ship.TypeId,
                new ShipType()
                {
                    Name = ship.TypeName,
                    Group = ship.GroupName,
                    Id = ship.TypeId,
                });
            }
            var json = JsonConvert.SerializeObject(ShipTypes);
            File.WriteAllText(@"Quesadilla.ShipTypes.json", json);
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
            GenerateShiptypesData();
        }
    }
}
