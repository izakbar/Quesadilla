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
using System.Windows.Forms;

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
                HowManyTicksBeforeAlert = HowManyTicksBeforeAlert,
                HowManyTicksBeforeDPSGraphUpdate = HowManyTicksBeforeDPSGraphUpdate,
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
                ExpireZKillSeconds = ExpireZKillSeconds,
                ShowLastUnexpiredAlerts = ShowLastUnexpiredAlerts,
                IgnoreAsTooManyJumps = IgnoreAsTooManyJumps,
                ExpireCombatSeconds = ExpireCombatSeconds,
                ExpireISKSeconds = ExpireISKSeconds,
                HomeSystem = HomeSystem,
                InitialPosition = InitialPosition,
                StayOnTop = StayOnTop,
                DPSGraphStayOnTop = DPSGraphStayOnTop,
                DPSGraphLegends = DPSGraphLegends,
                DPSGraphTransparent = DPSGraphTransparent,
                AlertTransparent = AlertTransparent,
                DPSShowWindow = DPSShowWindow,
                DPSSaveCSV = DPSSaveCSV,
                DPSAllToons = DPSAllToons,
                DPSGraphLineSize = DPSGraphLineSize,
                GraphCombatMaxPoints = GraphCombatMaxPoints,
                GraphCombatMinPointSeconds = GraphCombatMinPointSeconds,
                GraphCombatSeconds = GraphCombatSeconds,
                ShowZKillAlerts = ShowZKillAlerts,
                ZKillAlertText = ZKillAlertText,
                ZKillMenuText = ZKillMenuText,
                ZKillContextText = ZKillContextText,
                ZKillCircleColour = ZKillCircleColour,
                ZKillIskColour = ZKillIskColour,
                ZKillBarColour = ZKillBarColour,
                ZKillShowBar = ZKillShowBar,
                ZKillSound = ZKillSound,
                AlertPopupMinimap = AlertPopupMinimap,
                AlertPopupMinimapShowSystemText = AlertPopupMinimapShowSystemText,
                AlertPopupShowMinimapBorder = AlertPopupShowMinimapBorder,
                AlertPopupMinimapBorderColor = AlertPopupMinimapBorderColor,
                AlertPopupMinimapLineColorNormal = AlertPopupMinimapLineColorNormal,
                AlertPopupMinimapLineColorRoute = AlertPopupMinimapLineColorRoute,
                AlertPopupMinimapSystemColorNormal = AlertPopupMinimapSystemColorNormal,
                AlertPopupMinimapSystemColorRoute = AlertPopupMinimapSystemColorRoute,
                AlertPopupMinimapSystemColorFrom = AlertPopupMinimapSystemColorFrom,
                AlertPopupMinimapSystemColorTo = AlertPopupMinimapSystemColorTo,
                AlertPopupMinimapSystemColorText = AlertPopupMinimapSystemColorText,
                IntegrationPort = IntegrationPort,
                MainWindowColorText = MainWindowColorText,
                MainWindowBackgroundColorText = MainWindowBackgroundColorText,
                MainWindowLogBackgroundColorText = MainWindowLogBackgroundColorText,
                MainWindowLogColorText = MainWindowLogColorText,
            };

            c.AlertPopupMinimapPosition = new Point(AlertPopupMinimapPosition.X, AlertPopupMinimapPosition.Y);
            c.AlertPopupMinimapSize = new Point(AlertPopupMinimapSize.X, AlertPopupMinimapSize.Y);

            c.ignoreStrings = new List<Regex>();
            c.ignoreSystems = new List<int>();
            c.AlertConfiguration = new List<AlertConfig>();
            c.Logfiles = new List<LogfileConfig>();
            c.CSSData = new List<CSSData>();

            foreach (var s in ignoreStrings)
            {
                c.ignoreStrings.Add(s);
            }
            foreach (var s in ignoreSystems)
            {
                c.ignoreSystems.Add(s);
            }
            foreach (var a in AlertConfiguration)
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
            foreach (var l in Logfiles)
            {
                c.Logfiles.Add(l);
            }
            foreach (var css in CSSData)
            {
                c.CSSData.Add(css);
            }

            c.InitialPosition = new Point(InitialPosition.X, InitialPosition.Y);
            c.WindowSize = new Point(WindowSize.X, WindowSize.Y);
            c.DPSGraphWindowSize = new Point(DPSGraphWindowSize.X, DPSGraphWindowSize.Y);
            c.DPSGraphWindowSizeAdjust = new Point(DPSGraphWindowSizeAdjust.X, DPSGraphWindowSizeAdjust.Y);

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
                c.TickMiliseconds = 590;
                c.HowManyTicksBeforeCleardown = 8;
                c.HowManyTicksBeforeAlert = 3;
                c.HowManyTicksBeforeDPSGraphUpdate = 4;
                c.HomeSystem = "K7D-II";
                c.InitialPosition = new Point(0, 0);
                c.WindowSize = new Point(760, 404);
                c.DPSGraphWindowSize = new Point(250, 100);
                c.DPSGraphWindowSizeAdjust = new Point(10, 15);
                c.DPSGraphLegends = false;
                c.StayOnTop = true;
                c.DPSGraphStayOnTop = true;
                c.DPSGraphTransparent = true;
                c.DPSGraphLineSize = 2;
                c.AlertTransparent = false;
                c.DPSShowWindow = true;
                c.DPSSaveCSV = false;
                c.DPSAllToons = false;
                c.IgnoreAsTooManyJumps = 12;
                c.ExpireAlertSeconds = 500;
                c.ExpireCombatSeconds = 1200;
                c.ExpireISKSeconds = 5400; // 90 mins
                c.ExpireZKillSeconds = 900; // 15 mins
                c.GraphCombatSeconds = 300;
                c.GraphCombatMaxPoints = 30;
                c.GraphCombatMinPointSeconds = 10;

                c.AlertPopupSeconds = 8;
                c.AlertPopupOpacity = 99;
                c.AlertPopupOpacityStep = 5;
                c.AlertPopupLocation = PopupLocationType.BottomLeft;
                c.DPSLocation = PopupLocationType.TopRight;
                c.AlertPopupBackground = "bg_paulmart2";

                c.ShowZKillAlerts = true;

                c.AlertPopupSystemNameColour = Color.Lime;
                c.AlertPopupRegionNameColour = Color.FromArgb(125, 255, 255);
                c.AlertPopupContextInfoColour = Color.Yellow;
                c.AlertPopupIntelLineColour = Color.White;
                c.AlertPopupAlertTextColour = Color.White;

                c.ShowLastUnexpiredAlerts = 7;

                c.ZKillAlertText = "{victim} died to {killer} in {system}";
                c.ZKillContextText = "{victim} {victimship}\nKilled by {killer}";
                c.ZKillMenuText = "{killer} killed {victim} in {system} {value}";
                c.ZKillCircleColour = Color.Red;
                c.ZKillIskColour = Color.White;
                c.ZKillBarColour = Color.LimeGreen;
                c.ZKillShowBar = true;
                c.ZKillSound = "quiet_click";

                c.AlertPopupMinimap = true;
                c.AlertPopupShowMinimapBorder = false;
                c.AlertPopupMinimapBorderColor = Color.SteelBlue;
                c.AlertPopupMinimapLineColorNormal = Color.Gainsboro;
                c.AlertPopupMinimapLineColorRoute = Color.Tomato;
                c.AlertPopupMinimapSystemColorNormal = Color.Gainsboro;
                c.AlertPopupMinimapSystemColorRoute = Color.Pink;
                c.AlertPopupMinimapSystemColorFrom = Color.Red;
                c.AlertPopupMinimapSystemColorTo = Color.Green;
                c.AlertPopupMinimapShowSystemText = true;
                c.AlertPopupMinimapSystemColorText = Color.LemonChiffon;

                c.AlertPopupMinimapPosition = new Point(30, 0);
                c.AlertPopupMinimapSize = new Point(232, 150);

                c.IntegrationPort = 65432;

                c.MainWindowColorText = Color.LightGoldenrodYellow;
                c.MainWindowBackgroundColorText = System.Drawing.ColorTranslator.FromHtml("#0f0f11");
                c.MainWindowLogBackgroundColorText = Color.Red;
                c.MainWindowLogColorText = Color.Black;
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
                    TextDisplay = "{jumps}->{fromsystem} : {route}",
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
                    TextDisplay = "{jumps}->{fromsystem} : {route}",
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
                    TextDisplay = "{jumps}->{fromsystem} : {route}",
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
                    TextDisplay = "{jumps}->{fromsystem} : {route}",
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
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Decloak",
                    Enabled = true,
                    AlertType = AlertConfigType.Decloak,
                    TextDisplay = "Decloak by {context}",
                    MenuDisplay = "Decloak by {context}",
                    SoundFile = "tuba",
                });
            }

            if (c.Logfiles == null)
            {
                c.Logfiles = new List<LogfileConfig> { };
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Catch Intel",
                    Logfile = "catch.imperium",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Fountain Intel",
                    Logfile = "ftn.imperium",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Period Basis Intel",
                    Logfile = "period.imperium",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Querious Intel",
                    Logfile = "querious.imperium",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Delve Intel",
                    Logfile = "delve.imperium",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = false,
                    FriendlyName = "Cloud Ring Intel",
                    Logfile = "cr.imperium",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Paragon Soul Intel",
                    Logfile = "paragon.imperium",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Querious/Catch Intel",
                    Logfile = "Quer_Catch.imperium",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
                c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Izakbar DEBUG",
                    Logfile = "izakbar.debug",
                    LogType = Logfile.LogFileType.ChatIntel,
                });
            }

            if (c.CSSData== null)
            {
                c.CSSData = new List<CSSData> { };
                c.CSSData.Add(new CSSData()
                {
                    Name = "OBS.ISK",
                    FriendlyName = "OBS ISK Value",
                    CSS = @"
.iskcontainer {
  width: 100%;
  position: relative;
}
.iskperhour {
  border: 3px solid transparent;
  border-radius: 15px;
  outline: 2px solid Gold;

  display: inline-block;
  position: absolute;

  color: green;
  font-size: 24px;

  margin: auto;
}
",
                });
                c.CSSData.Add(new CSSData()
                {
                    Name = "OBS.LatestAlert",
                    FriendlyName = "OBS Latest Alert",
                    CSS = @"
.alert {
  border: 3px solid transparent;
  border-radius: 15px;
  outline: 2px solid Gold;

  display: inline-block;
  position: relative;
}
",
                });
                c.CSSData.Add(new CSSData()
                {
                    Name = "OBS.ZkillList",
                    FriendlyName = "OBS Zkill Listing",
                    CSS = @"
.zkillkill {
  display: inline-grid;
  position: relative;
  background-color: transparent;
  width:100px
}
.zkill-ship {
  width:100px;
}
.zkill-victim {
  color: Gold;
  position: absolute;
  bottom: 7px;
  font-size: 11px;
  text-align: center; width: 100%;
}
.zkill-value {
  color: LimeGreen;
  position: absolute;
  top: 3px;
  left: 3px;
  font-size: 18px;
}
img.rounded-corners {
  border-radius: 10%;
}
.zkill {
  border: 3px solid transparent;
  border-radius: 15px;
  outline: 2px solid Gold;

  display: inline-block;
  position: relative;
}
",
                });
            }
            return (c);
        }

        public void Save(string filename)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        public int TickMiliseconds { get; set; }
        public int HowManyTicksBeforeCleardown { get; set; }
        public int HowManyTicksBeforeAlert { get; set; }
        public int HowManyTicksBeforeDPSGraphUpdate { get; set; }
        public string HomeSystem { get; set; }
        public List<Regex> ignoreStrings { get; set; }
        public List<int> ignoreSystems { get; set; }
        public Point InitialPosition { get; set; }
        public Point WindowSize { get; set; }
        public Point DPSGraphWindowSize { get; set; }
        public Point DPSGraphWindowSizeAdjust { get; set; }
        public bool StayOnTop { get; set; }
        public bool DPSGraphStayOnTop { get; set; }
        public bool DPSGraphLegends { get; set; }
        public bool DPSGraphTransparent { get; set; }
        public bool AlertTransparent { get; set; }
        public bool DPSShowWindow { get; set; }
        public bool DPSAllToons { get; set; }
        public bool DPSSaveCSV { get; set; }

        public bool ShowZKillAlerts { get; set; }
        public int IgnoreAsTooManyJumps { get; set; }
        public int ExpireAlertSeconds { get; set; }
        public int ExpireCombatSeconds { get; set; }
        public int ExpireISKSeconds { get; set; }
        public int ExpireZKillSeconds { get; set; }
        public int GraphCombatSeconds { get; set; }
        public int GraphCombatMaxPoints { get; set; }
        public int GraphCombatMinPointSeconds { get; set; }
        public List<AlertConfig> AlertConfiguration { get; set; }
        public List<LogfileConfig> Logfiles { get; set; }
        public List<CSSData> CSSData { get; set; }
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

        public string ZKillAlertText { get; set; }
        public string ZKillContextText { get; set; }
        public string ZKillMenuText { get; set; }
        public string ZKillSound { get; set; }
        public Color ZKillCircleColour { get; set; }
        public Color ZKillIskColour { get; set; }
        public Color ZKillBarColour { get; set; }
        public bool ZKillShowBar { get; set; }

        public int DPSGraphLineSize { get; set; }
        public int ShowLastUnexpiredAlerts { get; set; }

        public bool AlertPopupMinimap { get; set; }
        public bool AlertPopupShowMinimapBorder { get; set; }
        public bool AlertPopupMinimapShowSystemText { get; set; }
        public Color AlertPopupMinimapBorderColor { get; set; }
        public Color AlertPopupMinimapLineColorNormal { get; set; }
        public Color AlertPopupMinimapLineColorRoute { get; set; }
        public Color AlertPopupMinimapSystemColorNormal { get; set; }
        public Color AlertPopupMinimapSystemColorRoute { get; set; }
        public Color AlertPopupMinimapSystemColorFrom { get; set; }
        public Color AlertPopupMinimapSystemColorTo { get; set; }
        public Color AlertPopupMinimapSystemColorText { get; set; }

        public Point AlertPopupMinimapPosition { get; set; }
        public Point AlertPopupMinimapSize { get; set; }
        public int IntegrationPort { get; set; }

        public Color MainWindowColorText { get; set; }
        public Color MainWindowBackgroundColorText { get; set; }
        public Color MainWindowLogBackgroundColorText { get; set; }
        public Color MainWindowLogColorText { get; set; }
    }
}