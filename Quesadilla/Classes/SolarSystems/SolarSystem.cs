using Newtonsoft.Json;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.ZKill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quesadilla.Classes.SolarSystems
{
    public class SolarSystem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RegionName { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double DotlanX { get; set; }
        public double DotlanY { get; set; }
        [JsonIgnore]
        public Vector3 Xyz { get; set; }
        [JsonIgnore]
        public List<ZKillKill> Kills { get; set; }
        [JsonIgnore]
        public List<AlertQueueItem> Alerts { get; set; }
        [JsonIgnore]
        public DateTime AlertTime { get; set; }
        [JsonIgnore]
        private Regex _nameRegex;
        [JsonIgnore]
        public Regex NameRegex => this._nameRegex;
        public bool MatchNameRegex(string logLine) => this._nameRegex.IsMatch(logLine);
        public List<SolarSystemConnectionData> ConnectedTo { get; set; }
        public List<PlanetInfo> Planets { get; set; }
        [JsonIgnore]
        public int? NPCKillsDeltaLastHour { get; set; }
        public bool Incursion { get; set; }
        public bool IceField { get; set; }
        public void SetNameRegex()
        {
            string str = "";
            foreach (char c in Name)
                str = !char.IsLetter(c) ? str + c.ToString() : str + "[" + c.ToString().ToLower() + c.ToString().ToUpper() + "]";
            _nameRegex = new Regex("\\b" + str + "\\b", RegexOptions.Compiled);
        }
    }
}
