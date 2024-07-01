using Cyotek.Windows.Forms;
using Newtonsoft.Json;
using Quesadilla.Classes.Attributes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Design;
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
                HowManyTicksBeforeLogfileCheck = HowManyTicksBeforeLogfileCheck,
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
                AlertPopupShowMinimap = AlertPopupShowMinimap,
                AlertPopupMinimapShowSystemText = AlertPopupMinimapShowSystemText,
                AlertPopupShowMinimapBorder = AlertPopupShowMinimapBorder,
                AlertPopupMinimapBorderColour = AlertPopupMinimapBorderColour,
                AlertPopupMinimapLineColourNormal = AlertPopupMinimapLineColourNormal,
                AlertPopupMinimapLineColourRoute = AlertPopupMinimapLineColourRoute,
                AlertPopupMinimapSystemColourNormal = AlertPopupMinimapSystemColourNormal,
                AlertPopupMinimapSystemColourRoute = AlertPopupMinimapSystemColourRoute,
                AlertPopupMinimapSystemColourFrom = AlertPopupMinimapSystemColourFrom,
                AlertPopupMinimapSystemColourTo = AlertPopupMinimapSystemColourTo,
                AlertPopupMinimapSystemColourText = AlertPopupMinimapSystemColourText,
                AlertPopupMinimapZKillColour = AlertPopupMinimapZKillColour,
                AlertPopupMinimapIntelColour = AlertPopupMinimapIntelColour,
                AlertPopupMinimapAltPositionColour = AlertPopupMinimapAltPositionColour,
                AlertPopupMinimapRawXY = AlertPopupMinimapRawXY,
                IntegrationPort = IntegrationPort,
                MainWindowTextColour = MainWindowTextColour,
                MainWindowBackgroundColour = MainWindowBackgroundColour,
                MainWindowLogBackgroundColour = MainWindowLogBackgroundColour,
                MainWindowLogTextColour = MainWindowLogTextColour,
                ShowMinimap = ShowMinimap,
                MinimapShowSystemText = MinimapShowSystemText,
                ShowMinimapBorder = ShowMinimapBorder,
                MinimapBorderColour = MinimapBorderColour,
                MinimapLineColourNormal = MinimapLineColourNormal,
                MinimapLineColourRoute = MinimapLineColourRoute,
                MinimapSystemColourNormal = MinimapSystemColourNormal,
                MinimapSystemColourRoute = MinimapSystemColourRoute,
                MinimapSystemColourFrom = MinimapSystemColourFrom,
                MinimapSystemColourTo = MinimapSystemColourTo,
                MinimapSystemColourText = MinimapSystemColourText,
                MinimapZKillColour = AlertPopupMinimapZKillColour,
                MinimapIntelColour = AlertPopupMinimapIntelColour,
                MinimapAltPositionColour = MinimapAltPositionColour,
                MinimapRawXY = MinimapRawXY,

                MinimapJumps = MinimapJumps,

                CurrentCharacterColour = CurrentCharacterColour,
                ISKColour = ISKColour,
                AlertsOutstandingColour = AlertsOutstandingColour,
                EvescoutColour = EvescoutColour,

                MinimapHeight = MinimapHeight,

                MainWindowExtraInformationColour = MainWindowExtraInformationColour,
                MainWindowShowExtraInformation = MainWindowShowExtraInformation,

                AlertPopupMinimapMMouseoverSystemColour = AlertPopupMinimapMMouseoverSystemColour,
                MinimapMMouseoverSystemColour = MinimapMMouseoverSystemColour,
                MainWindowMouseoverSystemColour = MainWindowMouseoverSystemColour,

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
                c.TickMiliseconds = 150;
                c.HowManyTicksBeforeCleardown = 30;
                c.HowManyTicksBeforeAlert = 2;
                c.HowManyTicksBeforeDPSGraphUpdate = 8;
                c.HowManyTicksBeforeLogfileCheck = 3;
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
                c.ExpireAlertSeconds = 300; // 5 mins
                c.ExpireCombatSeconds = 1200; // 20 mins
                c.ExpireISKSeconds = 5400; // 90 mins
                c.ExpireZKillSeconds = 600; // 10 mins
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

                c.AlertPopupShowMinimap = true;
                c.AlertPopupShowMinimapBorder = false;
                c.AlertPopupMinimapBorderColour = Color.SteelBlue;
                c.AlertPopupMinimapLineColourNormal = Color.Gainsboro;
                c.AlertPopupMinimapLineColourRoute = Color.Tomato;
                c.AlertPopupMinimapSystemColourNormal = Color.Gainsboro;
                c.AlertPopupMinimapSystemColourRoute = Color.Pink;
                c.AlertPopupMinimapSystemColourFrom = Color.Red;
                c.AlertPopupMinimapSystemColourTo = Color.Green;
                c.AlertPopupMinimapShowSystemText = true;
                c.AlertPopupMinimapSystemColourText = Color.LemonChiffon;

                c.AlertPopupMinimapZKillColour = Color.Red;
                c.AlertPopupMinimapIntelColour = Color.Orange;
                c.AlertPopupMinimapAltPositionColour = Color.Aqua;

                c.AlertPopupMinimapPosition = new Point(34, 3);
                c.AlertPopupMinimapSize = new Point(193, 128);

                c.AlertPopupMinimapRawXY = false;

                c.IntegrationPort = 65432;

                c.MainWindowTextColour = Color.LightGoldenrodYellow;
                c.MainWindowBackgroundColour = System.Drawing.ColorTranslator.FromHtml("#0f0f11");
                c.MainWindowLogBackgroundColour = System.Drawing.ColorTranslator.FromHtml("#0f0f11");
                c.MainWindowLogTextColour = Color.LightGoldenrodYellow;

                c.ShowMinimap = true;
                c.ShowMinimapBorder = false;
                c.MinimapBorderColour = Color.SteelBlue;
                c.MinimapLineColourNormal = Color.Gainsboro;
                c.MinimapLineColourRoute = Color.Tomato;
                c.MinimapSystemColourNormal = Color.Gainsboro;
                c.MinimapSystemColourRoute = Color.Pink;
                c.MinimapSystemColourFrom = Color.Red;
                c.MinimapSystemColourTo = Color.Green;
                c.MinimapShowSystemText = true;
                c.MinimapSystemColourText = Color.LemonChiffon;

                c.MinimapMMouseoverSystemColour = Color.OrangeRed;
                c.AlertPopupMinimapMMouseoverSystemColour = Color.OrangeRed;
                c.MainWindowMouseoverSystemColour = Color.Orchid;

                c.MinimapZKillColour = Color.Red;
                c.MinimapIntelColour = Color.Orange;
                c.MinimapAltPositionColour = Color.Aqua;

                c.MinimapRawXY = false;
                c.MinimapJumps = 3;
                c.CurrentCharacterColour = Color.MediumTurquoise;
                c.ISKColour = Color.Aqua;
                c.AlertsOutstandingColour = Color.LightPink;
                c.EvescoutColour = Color.LimeGreen;

                c.MainWindowExtraInformationColour = Color.LimeGreen;
                c.MainWindowShowExtraInformation = true;

                c.MinimapHeight = 200;
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

        [Category("General")]
        public string HomeSystem { get; set; }
        [Category("General")]
        public bool ShowZKillAlerts { get; set; }
        [Category("General")]
        public int IgnoreAsTooManyJumps { get; set; }
        [Category("General")]
        public int ShowLastUnexpiredAlerts { get; set; }

        [Category("Tick Timing")]
        [DisplayName("Milliseconds for main tick")]
        [ReadOnly(true)]
        public int TickMiliseconds { get; set; }
        [Category("Tick Timing")]
        [ReadOnly(true)]
        public int HowManyTicksBeforeCleardown { get; set; }
        [Category("Tick Timing")]
        [ReadOnly(true)]
        public int HowManyTicksBeforeAlert { get; set; }
        [Category("Tick Timing")]
        [ReadOnly(true)]
        public int HowManyTicksBeforeDPSGraphUpdate { get; set; }
        [Category("Tick Timing")]
        [ReadOnly(true)]
        public int HowManyTicksBeforeLogfileCheck { get; set; }

        [Category("Main window")]
        public Point InitialPosition { get; set; }
        [Category("Main window")]
        public Point WindowSize { get; set; }
        [Category("Main window")]
        public bool StayOnTop { get; set; }
        [Category("Main window")]
        public Color MainWindowTextColour { get; set; }
        [Category("Main window")]
        public Color MainWindowBackgroundColour { get; set; }
        [Category("Main window")]
        public Color MainWindowLogBackgroundColour { get; set; }
        [Category("Main window")]
        public Color MainWindowLogTextColour { get; set; }

        [Category("Main window")]
        public bool ShowMinimap { get; set; }
        [Category("Main window")]
        public bool ShowMinimapBorder { get; set; }
        [Category("Main window")]
        public bool MinimapShowSystemText { get; set; }
        [Category("Main window")]
        public Color MinimapBorderColour { get; set; }
        [Category("Main window")]
        public Color MinimapLineColourNormal { get; set; }
        [Category("Main window")]
        public Color MinimapLineColourRoute { get; set; }
        [Category("Main window")]
        public Color MinimapSystemColourNormal { get; set; }
        [Category("Main window")]
        public Color MinimapSystemColourRoute { get; set; }
        [Category("Main window")]
        public Color MinimapSystemColourFrom { get; set; }
        [Category("Main window")]
        public Color MinimapSystemColourTo { get; set; }
        [Category("Main window")]
        public Color MinimapSystemColourText { get; set; }
        [Category("Main window")]
        public Color MinimapZKillColour { get; set; }
        [Category("Main window")]
        public Color MinimapIntelColour { get; set; }
        [Category("Main window")]
        public Color MinimapMMouseoverSystemColour { get; set; }
        [Category("Main window")]
        public Color MainWindowMouseoverSystemColour { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapMMouseoverSystemColour { get; set; }
        [Category("Alert")]
        public bool AlertTransparent { get; set; }

        [Category("Main window")]
        public Color MinimapAltPositionColour { get; set; }
        [Category("Main window")]
        public int MinimapHeight { get; set; }
        [Category("Main window")]
        public bool MinimapRawXY { get; set; }
        [Category("Main window")]
        public int MinimapJumps { get; set; }

        [Category("Main window")]
        [DisplayName("Current character colour")]
        [Description("The colour to use for the currently active eveonline character on the main window")]
        public Color CurrentCharacterColour { get; set; }

        [Category("Main window")]
        [DisplayName("ISK/hr Colour")]
        [Description("The colour to use for the ISK/HR show on the main window")]
        public Color ISKColour { get; set; }

        [Category("Main window")]
        [DisplayName("non-expired Alerts/Kills Colour")]
        public Color AlertsOutstandingColour { get; set; }

        [Category("Main window")]
        [DisplayName("EveScout Information Colour")]
        public Color EvescoutColour { get; set; }

        [Category("Main window")]
        [DisplayName("Show Extra Information")]
        public bool MainWindowShowExtraInformation { get; set; }
        [Category("Main window")]
        [DisplayName("Extra Information Colour")]
        public Color MainWindowExtraInformationColour { get; set; }

        [Category("DPS")]
        public Point DPSGraphWindowSize { get; set; }
        [Category("DPS")]
        public Point DPSGraphWindowSizeAdjust { get; set; }
        [Category("DPS")]
        public bool DPSGraphStayOnTop { get; set; }
        [Category("DPS")]
        public bool DPSGraphLegends { get; set; }
        [Category("DPS")]
        public bool DPSGraphTransparent { get; set; }
        [Category("DPS")]
        public bool DPSShowWindow { get; set; }
        [Category("DPS")]
        public bool DPSAllToons { get; set; }
        [Category("DPS")]
        public bool DPSSaveCSV { get; set; }
        [Category("DPS")]
        public int GraphCombatSeconds { get; set; }
        [Category("DPS")]
        public int GraphCombatMaxPoints { get; set; }
        [Category("DPS")]
        public int GraphCombatMinPointSeconds { get; set; }
        [Category("DPS")]
        public int DPSGraphLineSize { get; set; }

        [Category("Alert")]
        public int AlertPopupSeconds { get; set; }
        [Category("Alert")]
        [Range(0, 100)]
        public int AlertPopupOpacity { get; set; }

        [Category("Alert")]
        [Range(0, 100)]
        public int AlertPopupOpacityStep { get; set; }
        [Category("Alert")]
        public PopupLocationType AlertPopupLocation { get; set; }
        [Category("Alert")]
        public PopupLocationType DPSLocation { get; set; }
        [Category("Alert")]
        public string AlertPopupBackground { get; set; }
        [Category("Alert")]
        public Color AlertPopupSystemNameColour { get; set; }
        [Category("Alert")]
        public Color AlertPopupRegionNameColour { get; set; }
        [Category("Alert")]
        public Color AlertPopupContextInfoColour { get; set; }
        [Category("Alert")]
        public Color AlertPopupIntelLineColour { get; set; }
        [Category("Alert")]
        public Color AlertPopupAlertTextColour { get; set; }
        [Category("Alert")]
        public bool AlertPopupShowMinimap { get; set; }
        [Category("Alert")]
        public bool AlertPopupShowMinimapBorder { get; set; }
        [Category("Alert")]
        public bool AlertPopupMinimapShowSystemText { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapBorderColour { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapLineColourNormal { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapLineColourRoute { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapSystemColourNormal { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapSystemColourRoute { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapSystemColourFrom { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapSystemColourTo { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapSystemColourText { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapZKillColour { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapIntelColour { get; set; }
        [Category("Alert")]
        public Color AlertPopupMinimapAltPositionColour { get; set; }
        [Category("Alert")]
        public bool AlertPopupMinimapRawXY { get; set; }

        [Category("Alert")]
        public Point AlertPopupMinimapPosition { get; set; }
        [Category("Alert")]
        public Point AlertPopupMinimapSize { get; set; }

        [Category("Alert")]
        public string ZKillAlertText { get; set; }
        [Category("Alert")]
        public string ZKillContextText { get; set; }
        [Category("Alert")]
        public string ZKillMenuText { get; set; }
        [Category("Alert")]
        public string ZKillSound { get; set; }
        [Category("Alert")]
        public Color ZKillCircleColour { get; set; }
        [Category("Alert")]
        public Color ZKillIskColour { get; set; }
        [Category("Alert")]
        public Color ZKillBarColour { get; set; }
        [Category("Alert")]
        public bool ZKillShowBar { get; set; }


        [Category("General")]
        public int ExpireAlertSeconds { get; set; }
        [Category("General")]
        public int ExpireCombatSeconds { get; set; }
        [Category("General")]
        public int ExpireISKSeconds { get; set; }

        [Category("General")]
        [DisplayName("ZKill expiry seconds")]
        [Description("How many seconds to consider zkill entry as old and no longer need to track")]
        public int ExpireZKillSeconds { get; set; }

        [Category("General")]
        [DisplayName("OBS Integration port")]
        [Description("TCP/IP Port number to bind to for OBS integration")]
        public int IntegrationPort { get; set; }

        public List<AlertConfig> AlertConfiguration { get; set; }
        public List<LogfileConfig> Logfiles { get; set; }
        public List<CSSData> CSSData { get; set; }

        public List<Regex> ignoreStrings { get; set; }
        public List<int> ignoreSystems { get; set; }
    }
}