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

            if (c is null)
            {
                c = new Config();
                c.HomeSystem = "K7D-II";
                c.InitialPosition = new Point(0, 0);
                c.WindowSize = new Point(300, 100);
                c.StayOnTop = true;
                c.IgnoreAsTooManyJumps = 15;
                c.ExpireAlertSeconds = 500;
                c.ExpireCombatSeconds = 1200;

                c.AlertPopupSeconds = 8;
                c.AlertPopupOpacity = 99;
                c.AlertPopupOpacityStep = 5;
                c.AlertPopupLocation = PopupLocationType.BottomLeft;
                c.AlertPopupBackground = "background2";

                c.AlertPopupSystemNameColour = Color.Red;
                c.AlertPopupRegionNameColour = Color.Red;
                c.AlertPopupContextInfoColour = Color.Red;
                c.AlertPopupIntelLineColour = Color.SaddleBrown;
                c.AlertPopupAlertTextColour = Color.MistyRose;

            }

            if (c.ignoreSystems == null) c.ignoreSystems = new List<int>();
            if (c.ignoreStrings == null)
            {
                c.ignoreStrings = new List<Regex>();
                c.ignoreStrings.Add(new Regex("\\b" + "clr" + "\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled));
                c.ignoreStrings.Add(new Regex("\\b" + "clear" + "\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled));
                c.ignoreStrings.Add(new Regex("\\b" + "status" + "\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled));
            }

            if (c.AlertConfiguration == null)
            {
                c.AlertConfiguration = new List<AlertConfig>();
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "All toons <= 5 jumps",
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.AllCharacters,
                    TextDisplay = "Hostile {jumps} jumps from {character} in {tosystem} {route}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Izakbar <= 5 jumps",
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.OneCharacter,
                    TextDisplay = "Hostile {jumps} jumps from {character} in {tosystem} {route}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                    TriggerText = "Izakbar",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Home system < 5 jumps",
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.Home,
                    TextDisplay = "Hostile {jumps} jumps from {fromsystem} in {tosystem} {route}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "1DQ1-A < 5 jumps",
                    Enabled = false,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.System,
                    TextDisplay = "Hostile {jumps} jumps from {fromsystem} in {tosystem} {route}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                    TriggerText = "1DQ1-A",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "ESS robbery",
                    Enabled = true,
                    AlertType = AlertConfigType.Text,
                    TextDisplay = "ESS robbery {log} reported by {reporter}",
                    SoundFile = "Boo2",
                    TriggerText = "ess",
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
        public List<int> ignoreSystems { get; set; }
        public Point InitialPosition { get; set; }
        public Point WindowSize { get; set; }
        public bool StayOnTop { get; set; }
        public int IgnoreAsTooManyJumps { get; set; }
        public int ExpireAlertSeconds { get; set; }
        public int ExpireCombatSeconds { get; set; }
        public List<AlertConfig> AlertConfiguration { get; set; }

        public int AlertPopupSeconds { get; set; }
        public int AlertPopupOpacity { get; set; }
        public int AlertPopupOpacityStep { get; set; }
        public PopupLocationType AlertPopupLocation { get; set; }
        public string AlertPopupBackground { get; set; }
        public Color AlertPopupSystemNameColour { get; set; }
        public Color AlertPopupRegionNameColour { get; set; }
        public Color AlertPopupContextInfoColour { get; set; }
        public Color AlertPopupIntelLineColour { get; set; }
        public Color AlertPopupAlertTextColour { get; set; }
    }
}