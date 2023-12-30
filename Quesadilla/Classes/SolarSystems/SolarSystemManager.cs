using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.SolarSystems
{
    public class SolarSystemManager
    {
        public Dictionary<int, SolarSystem> SolarSystems = new Dictionary<int, SolarSystem>();
        public Dictionary<string, int> Names = new Dictionary<string, int>();

        public void ReadSolarSystems(string jsonFile)
        {
            StreamReader reader = new StreamReader(jsonFile);
            var json = reader.ReadToEnd();

            SolarSystems = JsonConvert.DeserializeObject<Dictionary<int, SolarSystem>>(json);
            foreach( var s in SolarSystems ) {
                Names.Add(s.Value.Name, s.Key);
                s.Value.SetNameRegex();
            }
        }
    }
}
