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

        public Config Duplicate()
        {
            Config c = new Config()
            {
                TickMiliseconds = TickMiliseconds,
                HowManyTicksBeforeCleardown = HowManyTicksBeforeCleardown,
                AlertPopupOpacityStep = AlertPopupOpacityStep,
                AlertPopupOpacity = AlertPopupOpacity,
                AlertPopupAlertTextColour = AlertPopupAlertTextColour,
                AlertPopupBackground = AlertPopupBackground,
                AlertPopupContextInfoColour = AlertPopupContextInfoColour,
                AlertPopupIntelLineColour = AlertPopupIntelLineColour,
                AlertPopupLocation = AlertPopupLocation,
                DPSLocation = DPSLocation,
                AlertPopupRegionNameColour = AlertPopupRegionNameColour,
                AlertPopupSeconds = AlertPopupSeconds,
                AlertPopupSystemNameColour = AlertPopupSystemNameColour,
                ExpireAlertSeconds = ExpireAlertSeconds,
                ShowLastUnexpiredAlerts = ShowLastUnexpiredAlerts,
                IgnoreAsTooManyJumps = IgnoreAsTooManyJumps,
                ExpireCombatSeconds = ExpireCombatSeconds,
                HomeSystem = HomeSystem,
                InitialPosition = InitialPosition,
                StayOnTop = StayOnTop,
                DPSGraphStayOnTop = DPSGraphStayOnTop,
                DPSGraphTransparent= DPSGraphTransparent,
                DPSShowWindow = DPSShowWindow,
                DPSSaveCSV = DPSSaveCSV,
                DPSAllToons = DPSAllToons,
                WindowSize = WindowSize,
                GraphCombatMaxPoints = GraphCombatMaxPoints,
                GraphCombatMinPointSeconds = GraphCombatMinPointSeconds,
                GraphCombatSeconds = GraphCombatSeconds,
            };

            c.ignoreStrings = new List<Regex>();
            c.ignoreSystems = new List<int>();
            c.AlertConfiguration = new List<AlertConfig>();

            foreach (var s in ignoreStrings)
            {
                c.ignoreStrings.Add(s);
            }
            foreach (var s in ignoreSystems)
            {
                c.ignoreSystems.Add(s);
            }
            foreach(var a in AlertConfiguration)
            {
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    AlertRangeType = a.AlertRangeType,
                    AlertType = a.AlertType,
                    CustomTriggerEverySeconds = a.CustomTriggerEverySeconds,
                    Enabled = a.Enabled,
                    FriendlyName = a.FriendlyName,
                    Jumps1Comparison = a.Jumps1Comparison,
                    MenuDisplay = a.MenuDisplay,
                    TextDisplay = a.TextDisplay,
                    Jumps1Number = a.Jumps1Number,
                    Jumps2Comparison = a.Jumps2Comparison,
                    SoundFile = a.SoundFile,
                    Jumps2Number = a.Jumps2Number,
                    TriggerText = a.TriggerText
                });
            }

            c.InitialPosition = new Point(InitialPosition.X, InitialPosition.Y);
            c.WindowSize = new Point(WindowSize.X, WindowSize.Y);

            return (c);
        }


        public static Config Load(string filename)
        {
            var c = new Config();

            StreamReader reader = new StreamReader(filename);
            var json = reader.ReadToEnd();
            c = JsonConvert.DeserializeObject<Config>(json);

            if (c is null)
            {
                c = new Config();
                c.TickMiliseconds = 505;
                c.HowManyTicksBeforeCleardown = 8;
                c.HomeSystem = "K7D-II";
                c.InitialPosition = new Point(0, 0);
                c.WindowSize = new Point(760, 404);
                c.StayOnTop = true;
                c.DPSGraphStayOnTop = true;
                c.DPSGraphTransparent = true;
                c.DPSShowWindow = true;
                c.DPSSaveCSV = false;
                c.DPSAllToons = false;
                c.IgnoreAsTooManyJumps = 15;
                c.ExpireAlertSeconds = 500;
                c.ExpireCombatSeconds = 1200;
                c.GraphCombatSeconds = 180;
                c.GraphCombatMaxPoints = 30;
                c.GraphCombatMinPointSeconds = 10;

                c.AlertPopupSeconds = 8;
                c.AlertPopupOpacity = 99;
                c.AlertPopupOpacityStep = 5;
                c.AlertPopupLocation = PopupLocationType.BottomLeft;
                c.DPSLocation = PopupLocationType.BottomRight;
                c.AlertPopupBackground = "background2";

                c.AlertPopupSystemNameColour = Color.Red;
                c.AlertPopupRegionNameColour = Color.Red;
                c.AlertPopupContextInfoColour = Color.Red;
                c.AlertPopupIntelLineColour = Color.SaddleBrown;
                c.AlertPopupAlertTextColour = Color.MistyRose;

                c.ShowLastUnexpiredAlerts = 5;

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
                    FriendlyName = "All online toons <= 5 jumps",
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.AllOnlineCharacters,
                    TextDisplay = "Hostile {jumps} jumps from {character} in {tosystem} {route}",
                    MenuDisplay = "{jumps} jumps from {character} in {tosystem}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Izakbar (if online) <= 5 jumps",
                    Enabled = false,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.OneOnlineCharacter,
                    TextDisplay = "Hostile {jumps} jumps from {character} in {tosystem} {route}",
                    MenuDisplay = "{jumps} jumps from {character} in {tosystem}",
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
                    MenuDisplay = "{jumps} jumps from {fromsystem} in {tosystem}",
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
                    MenuDisplay = "{jumps} jumps from {fromsystem} in {tosystem}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                    TriggerText = "1DQ1-A",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "ESS robbery",
                    Enabled = false,
                    AlertType = AlertConfigType.Text,
                    TextDisplay = "ESS robbery {text} reported by {reporter}",
                    MenuDisplay = "ESS robbery {text} reported by {reporter}",
                    SoundFile = "Boo2",
                    TriggerText = "ess",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Faction Spawn",
                    Enabled = true,
                    AlertType = AlertConfigType.FactionSpawn,
                    TextDisplay = "Spawn {context}",
                    MenuDisplay = "Spawn {context}",
                    SoundFile = "trumpet",
                    CustomTriggerEverySeconds = 30,
                });
            }

            return (c);
        }

        public void Save(string filename)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }


        public int TickMiliseconds {  get; set; }
        public int HowManyTicksBeforeCleardown { get; set; }
        public string HomeSystem { get; set; }
        public List<Regex> ignoreStrings { get; set; }
        public List<int> ignoreSystems { get; set; }
        public Point InitialPosition { get; set; }
        public Point WindowSize { get; set; }
        public bool StayOnTop { get; set; }
        public bool DPSGraphStayOnTop { get; set; }
        public bool DPSGraphTransparent { get; set; }
        public bool DPSShowWindow { get; set; }
        public bool DPSAllToons { get; set; }
        public bool DPSSaveCSV { get; set; }
        public int IgnoreAsTooManyJumps { get; set; }
        public int ExpireAlertSeconds { get; set; }
        public int ExpireCombatSeconds { get; set; }
        public int GraphCombatSeconds { get; set; }
        public int GraphCombatMaxPoints { get; set; }
        public int GraphCombatMinPointSeconds { get; set; }
        public List<AlertConfig> AlertConfiguration { get; set; }

        public int AlertPopupSeconds { get; set; }
        public int AlertPopupOpacity { get; set; }
        public int AlertPopupOpacityStep { get; set; }
        public PopupLocationType AlertPopupLocation { get; set; }
        public PopupLocationType DPSLocation { get; set; }
        public string AlertPopupBackground { get; set; }
        public Color AlertPopupSystemNameColour { get; set; }
        public Color AlertPopupRegionNameColour { get; set; }
        public Color AlertPopupContextInfoColour { get; set; }
        public Color AlertPopupIntelLineColour { get; set; }
        public Color AlertPopupAlertTextColour { get; set; }

        public int ShowLastUnexpiredAlerts { get; set; }
    }
}