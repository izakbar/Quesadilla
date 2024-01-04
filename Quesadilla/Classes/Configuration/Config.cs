using Newtonsoft.Json;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public class Config
    {
        public static Config Load(string filename)
        {
            var c = new Config();

            StreamReader reader = new StreamReader(filename);
            var json = reader.ReadToEnd();
            c = JsonConvert.DeserializeObject<Config>(json);

            if (c.ignoreSystems == null ) c.ignoreSystems = new List<int>();
            if (c.ignoreStrings == null)  c.ignoreStrings = new List<Regex>();
            if (c.AlertConfiguration == null)
            {
                c.AlertConfiguration = new List<AlertConfig>();
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.AllCharacters,
                    TextDisplay = "Hostile {jumps} jumps from {character} in {tosystem}",
                    SoundFile = "sound1.mp3",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.OneCharacter,
                    TextDisplay = "Hostile {jumps} jumps from {character} in {tosystem}",
                    SoundFile = "sound1.mp3",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                    TriggerText = "Izakbar",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.Home,
                    TextDisplay = "Hostile {jumps} jumps from {fromsystem} to {tosystem}",
                    SoundFile = "sound1.mp3",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.System,
                    TextDisplay = "Hostile {jumps} jumps from {fromsystem} to {tosystem}",
                    SoundFile = "sound1.mp3",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                    TriggerText = "1DQ1-A",
                });
            }

            return (c);
        }

        public void Save(string filename)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        public string HomeSystem { get; set; }
        public List<Regex> ignoreStrings { get; set; }
        public  List<int> ignoreSystems { get; set; }
        public Point InitialPosition { get; set; }
        public int IgnoreAsTooManyJumps { get; set; }
        public List<AlertConfig> AlertConfiguration { get; set; }
    }
}