using Cyotek.Windows.Forms;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Quesadilla.Classes
{
    public class Config
    {

        public Config Duplicate()
        {
            Config c = new Config()
            {
                AlertPopupLocation = AlertPopupLocation,
                DPSLocation = DPSLocation,
                InitialPosition = InitialPosition,
                ZKillSound = ZKillSound,
            };

            // copy as many config items as possible
            foreach(var p in typeof(Config).GetProperties().Where(o => o.PropertyType == typeof(Color) || o.PropertyType == typeof(int) || o.PropertyType == typeof(string) || o.PropertyType == typeof(bool)))
            {
                var propertyType = typeof(Config).GetProperty(p.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.ExactBinding);
                var value = propertyType.GetValue(this);
                propertyType.SetValue(c, value);
            }

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

                // Find/set default values from DefaultValue attribute
                foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(c))
                {
                    var propertyType = typeof(Config).GetProperty(p.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.ExactBinding);
                    DefaultValueAttribute myAttribute = (DefaultValueAttribute)p.Attributes[typeof(DefaultValueAttribute)];
                    if ( myAttribute != null)
                    {
                        propertyType.SetValue(c, myAttribute.Value);
                    }
                }

                c.InitialPosition = new Point(0, 0);
                c.WindowSize = new Point(760, 404);
                c.DPSGraphWindowSize = new Point(250, 100);
                c.DPSGraphWindowSizeAdjust = new Point(10, 15);

                c.AlertPopupMinimapPosition = new Point(34, 3);
                c.AlertPopupMinimapSize = new Point(193, 128);

                c.AlertPopupLocation = PopupLocationType.BottomLeft;
                c.DPSLocation = PopupLocationType.TopRight;
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
        [DisplayName("Staging system")]
        [Description("When alerts are generated relative to your home/staging system - this is the value used")]
        [DefaultValue(typeof(string), "K7D-II")]
        public string HomeSystem { get; set; }

        [Category("General")]
        [DisplayName("Show ZKill alerts")]
        [Description("Show ZKill events close to your alert setup")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowZKillAlerts { get; set; }

        [Category("General")]
        [DisplayName("Too many jumps away")]
        [Description("How many jumps away should be considered too far to be involved with")]
        [DefaultValue(typeof(int), "12")]
        public int IgnoreAsTooManyJumps { get; set; }

        [Category("General")]
        [DisplayName("Show Last Alerts")]
        [Description("How many unexpired alerts to show in tray menu")]
        [DefaultValue(typeof(int), "7")]
        public int ShowLastUnexpiredAlerts { get; set; }

        [Category("Tick Timing")]
        [DisplayName("Milliseconds for main tick")]
        [ReadOnly(true)]
        [Description("")]
        [DefaultValue(typeof(int), "120")]
        public int TickMiliseconds { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeCleardown")]
        [Description("")]
        [DefaultValue(typeof(int), "30")]
        public int HowManyTicksBeforeCleardown { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeAlert")]
        [Description("")]
        [DefaultValue(typeof(int), "1")]
        public int HowManyTicksBeforeAlert { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeDPSGraphUpdate")]
        [Description("")]
        [DefaultValue(typeof(int), "6")]
        public int HowManyTicksBeforeDPSGraphUpdate { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeLogfileCheck")]
        [Description("")]
        [DefaultValue(typeof(int), "2")]
        public int HowManyTicksBeforeLogfileCheck { get; set; }

        [Category("Main window")]
        public Point InitialPosition { get; set; }

        [Category("Main window")]
        public Point WindowSize { get; set; }

        [Category("Main window")]
        [DisplayName("Stay On Top")]
        [Description("Should the main window stay on top of other windows")]
        [DefaultValue(typeof(bool), "true")]
        public bool StayOnTop { get; set; }

        [Category("Main window")]
        [DisplayName("MainWindowTextColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "LightGoldenrodYellow")]
        public Color MainWindowTextColour { get; set; }

        [Category("Main window")]
        [DisplayName("MainWindowBackgroundColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "#0f0f11")]
        public Color MainWindowBackgroundColour { get; set; }

        [Category("Main window")]
        [DisplayName("MainWindowLogBackgroundColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "#0f0f11")]
        public Color MainWindowLogBackgroundColour { get; set; }
        
        [Category("Main window")]
        [DisplayName("MainWindowLogTextColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "LightGoldenrodYellow")]
        public Color MainWindowLogTextColour { get; set; }

        [Category("Main window")]
        [DisplayName("ShowMinimap")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowMinimap { get; set; }

        [Category("Main window")]
        [DisplayName("Show Minimap Border")]
        [Description("")]
        [DefaultValue(typeof(bool), "false")]
        public bool ShowMinimapBorder { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapShowSystemText")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool MinimapShowSystemText { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapBorderColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "SteelBlue")]
        public Color MinimapBorderColour { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapLineColourNormal")]
        [Description("")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color MinimapLineColourNormal { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapLineColourRoute")]
        [Description("")]
        [DefaultValue(typeof(Color), "Tomato")]
        public Color MinimapLineColourRoute { get; set; }
        
        [Category("Main window")]
        [DisplayName("MinimapSystemColourNormal")]
        [Description("")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color MinimapSystemColourNormal { get; set; }
        
        [Category("Main window")]
        [DisplayName("MinimapSystemColourRoute")]
        [Description("")]
        [DefaultValue(typeof(Color), "Pink")]
        public Color MinimapSystemColourRoute { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapSystemColourFrom")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color MinimapSystemColourFrom { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapSystemColourTo")]
        [Description("")]
        [DefaultValue(typeof(Color), "Green")]
        public Color MinimapSystemColourTo { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapSystemColourText")]
        [Description("")]
        [DefaultValue(typeof(Color), "LemonChiffon")]
        public Color MinimapSystemColourText { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapZKillColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color MinimapZKillColour { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapIntelColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Orange")]
        public Color MinimapIntelColour { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapMMouseoverSystemColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "OrangeRed")]
        public Color MinimapMMouseoverSystemColour { get; set; }

        [Category("Main window")]
        [DisplayName("MainWindowMouseoverSystemColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Orchid")]
        public Color MainWindowMouseoverSystemColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapMMouseoverSystemColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "OrangeRed")]
        public Color AlertPopupMinimapMMouseoverSystemColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertTransparent")]
        [Description("")]
        [DefaultValue(typeof(bool), "false")]
        public bool AlertTransparent { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapAltPositionColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Aqua")]
        public Color MinimapAltPositionColour { get; set; }

        [Category("Main window")]
        [DisplayName("MinimapHeight")]
        [Description("")]
        [DefaultValue(typeof(int), "200")]
        public int MinimapHeight { get; set; }

        [Category("Main window")]
        [DisplayName("Use SDE X/Y")]
        [Description("Selecting true will show minimap using the SDE values for X/Y. Selecting False will use Dotlan layout")]
        [DefaultValue(typeof(bool), "false")]
        public bool MinimapRawXY { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap Jumps")]
        [Description("How many jumps to show in the minimap away from starting system")]
        [DefaultValue(typeof(int), "3")]
        public int MinimapJumps { get; set; }

        [Category("Main window")]
        [DisplayName("Current character colour")]
        [Description("The colour to use for the currently active eveonline character on the main window")]
        [DefaultValue(typeof(Color), "MediumTurquoise")]
        public Color CurrentCharacterColour { get; set; }

        [Category("Main window")]
        [DisplayName("ISK/hr Colour")]
        [Description("The colour to use for the ISK/HR show on the main window")]
        [DefaultValue(typeof(Color), "Aqua")]
        public Color ISKColour { get; set; }

        [Category("Main window")]
        [DisplayName("non-expired Alerts/Kills Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "LightPink")]
        public Color AlertsOutstandingColour { get; set; }

        [Category("Main window")]
        [DisplayName("EveScout Information Colour")]
        [DefaultValue(typeof(Color), "LimeGreen")]
        public Color EvescoutColour { get; set; }

        [Category("Main window")]
        [DisplayName("Show Extra Information")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool MainWindowShowExtraInformation { get; set; }

        [Category("Main window")]
        [DisplayName("Extra Information Colour")]
        [DefaultValue(typeof(Color), "LimeGreen")]
        public Color MainWindowExtraInformationColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPS Graph location")]
        [Description("Location to display DPS graph when required")]
        public PopupLocationType DPSLocation { get; set; }

        [Category("DPS")]
        public Point DPSGraphWindowSize { get; set; }

        [Category("DPS")]
        public Point DPSGraphWindowSizeAdjust { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphStayOnTop")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool DPSGraphStayOnTop { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLegends")]
        [Description("")]
        [DefaultValue(typeof(bool), "false")]
        public bool DPSGraphLegends { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphTransparent")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool DPSGraphTransparent { get; set; }

        [Category("DPS")]
        [DisplayName("DPSShowWindow")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool DPSShowWindow { get; set; }

        [Category("DPS")]
        [DisplayName("DPSAllToons")]
        [Description("")]
        [DefaultValue(typeof(bool), "false")]
        public bool DPSAllToons { get; set; }

        [Category("DPS")]
        [DisplayName("DPSSaveCSV")]
        [Description("")]
        [DefaultValue(typeof(bool), "false")]
        public bool DPSSaveCSV { get; set; }

        [Category("DPS")]
        [DisplayName("GraphCombatSeconds")]
        [Description("")]
        [DefaultValue(typeof(int), "300")]
        public int GraphCombatSeconds { get; set; }

        [Category("DPS")]
        [DisplayName("GraphCombatMaxPoints")]
        [Description("")]
        [DefaultValue(typeof(int), "30")]
        public int GraphCombatMaxPoints { get; set; }

        [Category("DPS")]
        [DisplayName("GraphCombatMinPointSeconds")]
        [Description("")]
        [DefaultValue(typeof(int), "10")]
        public int GraphCombatMinPointSeconds { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLineSize")]
        [Description("")]
        [DefaultValue(typeof(int), "2")]
        public int DPSGraphLineSize { get; set; }


        [Category("DPS")]
        [DisplayName("DPSGraphDPSInColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color DPSGraphDPSInColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphDPSOutColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Green")]
        public Color DPSGraphDPSOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphNeutInColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Orange")]
        public Color DPSGraphNeutInColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphNeutOutColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "YellowGreen")]
        public Color DPSGraphNeutOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLogiShieldsInColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Aqua")]
        public Color DPSGraphLogiShieldsInColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLogiShieldsOutColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "CornflowerBlue")]
        public Color DPSGraphLogiShieldsOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLogiArmorInColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Fuchsia")]
        public Color DPSGraphLogiArmorInColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLogiArmorOutColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "DarkMagenta")]
        public Color DPSGraphLogiArmorOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLogiHullInColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Silver")]
        public Color DPSGraphLogiHullInColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLogiHullOutColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "SlateGray")]
        public Color DPSGraphLogiHullOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLogiCapacitorInColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "LightYellow")]
        public Color DPSGraphLogiCapacitorInColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPSGraphLogiCapacitorOutColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "DarkOrange")]
        public Color DPSGraphLogiCapacitorOutColour { get; set; }

        [Category("Alert")]
        [DisplayName("Popup Seconds")]
        [Description("How long to show alerts for")]
        [DefaultValue(typeof(int), "8")]
        public int AlertPopupSeconds { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupOpacity")]
        [Description("")]
        [DefaultValue(typeof(int), "99")]
        public int AlertPopupOpacity { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupOpacityStep")]
        [Description("")]
        [DefaultValue(typeof(int), "5")]
        public int AlertPopupOpacityStep { get; set; }

        [Category("Alert")]
        public PopupLocationType AlertPopupLocation { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupBackground")]
        [Description("")]
        [DefaultValue(typeof(string), "bg_paulmart2")]
        public string AlertPopupBackground { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupSystemNameColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Lime")]
        public Color AlertPopupSystemNameColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupRegionNameColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Cyan")]
        public Color AlertPopupRegionNameColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupContextInfoColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Yellow")]
        public Color AlertPopupContextInfoColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupIntelLineColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "White")]
        public Color AlertPopupIntelLineColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupAlertTextColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "White")]
        public Color AlertPopupAlertTextColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupShowMinimap")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool AlertPopupShowMinimap { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupShowMinimapBorder")]
        [Description("")]
        [DefaultValue(typeof(bool), "false")]
        public bool AlertPopupShowMinimapBorder { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapShowSystemText")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool AlertPopupMinimapShowSystemText { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapBorderColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "SteelBlue")]
        public Color AlertPopupMinimapBorderColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapLineColourNormal")]
        [Description("")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color AlertPopupMinimapLineColourNormal { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapLineColourRoute")]
        [Description("")]
        [DefaultValue(typeof(Color), "Tomato")]
        public Color AlertPopupMinimapLineColourRoute { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapSystemColourNormal")]
        [Description("")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color AlertPopupMinimapSystemColourNormal { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapSystemColourRoute")]
        [Description("")]
        [DefaultValue(typeof(Color), "Pink")]
        public Color AlertPopupMinimapSystemColourRoute { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapSystemColourFrom")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color AlertPopupMinimapSystemColourFrom { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapSystemColourTo")]
        [Description("")]
        [DefaultValue(typeof(Color), "Green")]
        public Color AlertPopupMinimapSystemColourTo { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapSystemColourText")]
        [Description("")]
        [DefaultValue(typeof(Color), "LemonChiffon")]
        public Color AlertPopupMinimapSystemColourText { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapZKillColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color AlertPopupMinimapZKillColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapIntelColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Orange")]
        public Color AlertPopupMinimapIntelColour { get; set; }

        [Category("Alert")]
        [DisplayName("AlertPopupMinimapAltPositionColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Aqua")]
        public Color AlertPopupMinimapAltPositionColour { get; set; }

        [Category("Alert")]
        [DisplayName("Use SDE X/Y")]
        [Description("Selecting true will show minimap using the SDE values for X/Y. Selecting False will use Dotlan layout")]
        [DefaultValue(typeof(bool), "false")]
        public bool AlertPopupMinimapRawXY { get; set; }

        [Category("Alert")]
        public Point AlertPopupMinimapPosition { get; set; }

        [Category("Alert")]
        public Point AlertPopupMinimapSize { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillAlertText")]
        [Description("")]
        [DefaultValue(typeof(string), "{victim} died to {killer} in {system}")]
        public string ZKillAlertText { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillContextText")]
        [Description("")]
        [DefaultValue(typeof(string), "{victim} {victimship}\nKilled by {killer}")]
        public string ZKillContextText { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillMenuText")]
        [Description("")]
        [DefaultValue(typeof(string), "{killer} killed {victim} in {system} {value}")]
        public string ZKillMenuText { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillSound")]
        [Description("")]
        [DefaultValue(typeof(string), "quiet_click")]
        public string ZKillSound { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillCircleColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color ZKillCircleColour { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillIskColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "White")]
        public Color ZKillIskColour { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillBarColour")]
        [Description("")]
        [DefaultValue(typeof(Color), "LimeGreen")]
        public Color ZKillBarColour { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillShowBar")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool ZKillShowBar { get; set; }

        [Category("General")]
        [DisplayName("Alert expiry seconds")]
        [Description("How many seconds to consider an alert as old and no longer need to track")]
        [DefaultValue(typeof(int), "300")]
        [ReadOnly(true)]
        public int ExpireAlertSeconds { get; set; }
        
        [Category("General")]
        [DisplayName("Combat expiry seconds")]
        [Description("How many seconds to consider combat entry as old and no longer need to track")]
        [DefaultValue(typeof(int), "1200")]
        [ReadOnly(true)]
        public int ExpireCombatSeconds { get; set; }

        [Category("General")]
        [DisplayName("ISK earned expiry seconds")]
        [Description("How many seconds to consider ISK bounty entry as old and no longer need to track")]
        [DefaultValue(typeof(int), "5400")]
        [ReadOnly(true)]
        public int ExpireISKSeconds { get; set; }

        [Category("General")]
        [DisplayName("ZKill expiry seconds")]
        [Description("How many seconds to consider zkill entry as old and no longer need to track")]
        [DefaultValue(typeof(int), "600")]
        [ReadOnly(true)]
        public int ExpireZKillSeconds { get; set; }

        [Category("General")]
        [DisplayName("OBS Integration port")]
        [Description("TCP/IP Port number to bind to for OBS integration")]
        [DefaultValue(typeof(int), "65432")]
        public int IntegrationPort { get; set; }

        public List<AlertConfig> AlertConfiguration { get; set; }
        public List<LogfileConfig> Logfiles { get; set; }
        public List<CSSData> CSSData { get; set; }

        public List<Regex> ignoreStrings { get; set; }
        public List<int> ignoreSystems { get; set; }
    }
}