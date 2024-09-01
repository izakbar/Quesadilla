using EVEStandard.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Dotlan;
using Quesadilla.Classes.ESIData;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.Processes;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.Resources;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Classes.ZKill;
using Serilog;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography.Pkcs;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebPages;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using Windows.ApplicationModel.VoiceCommands;
using static Quesadilla.Classes.Processes.ProcessMonitor;
using static Quesadilla.Extensions.StringExtensions;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Quesadilla
{
    public partial class Quesadilla : Form
    {

        private List<String> _FactionRats = new List<string>()
        {
            "Dark Blood",
            "Dread Gurista",
            "True Sansha",
            "Sentient",
            "Shadow Serpentis",
            "Domination",
            " Titan",
            " Dreadnought",

            " Hauler",
            " Ferrier",
            " Gatherer",
            " Harvester",
            " Loader",
            " Bulker",
            " Carrier",
            " Convoy",
            " Courier",
            " Trailer",
            " Transporter",
            " Trucker",
        };

        public Bitmap latestAlertBitmap = null;
        public DateTime latestAlertTimestamp = DateTime.Now;

        private readonly Regex _CombatRegexMissesYouCompletely = new Regex("(?<combatEntity>.*)\\smisses\\syou\\scompletely", RegexOptions.Compiled);
        private readonly Regex _CombatRegexCombatData = new Regex("^<color=0x(?<combatColour>........)><b>(?<combatAmount>.*)</b>\\s*<color=0x........><font size=10>\\s*(?<combatContext>.*)\\s*</font>\\s*<b><color=0xffffffff>(?<combatEntity>.*)</b>(<font size=10>|<color=0x77ffffff>)*\\s-\\s(?<combatText>.*)", RegexOptions.Compiled);

        private readonly Regex _MiningRegexRoidExploded = new Regex("(?<miningDevice>.*)\\sdeactivates as it finds the resource it was harvesting\\s(?<miningHowItEnded>.*)", RegexOptions.Compiled);
        private readonly Regex _MiningRegexCargoFull = new Regex("Your\\s*(?<miningDevice>.*)\\s*has completed operations. Ship.s cargo hold is full.*", RegexOptions.Compiled);
        private readonly Regex _MiningRegexAmounts = new Regex("^<color=0x........>\\s*You mined\\s*<font size=..><color=0x........>(?<miningAmount>.*)<color=0x........><font size=..>\\s*units* of\\s*<color=0x........><font size=..>\\s*(?<miningContext>.*)\\s*<color=0x........><font size=..>\\swith a lost residue of\\s*<font size=..><color=0x........>(?<miningResidue>.*)<color=0x........><font size=..>\\s*units*", RegexOptions.Compiled);

        private readonly Regex _DecloakRegexMobileObservatory = new Regex("cloak deactivates due to a pulse from a Mobile Observatory", RegexOptions.Compiled);
        private readonly Regex _DecloakRegexProximity = new Regex("Your cloak deactivates due to proximity to (?<decloakProximity>.*)", RegexOptions.Compiled);

        private readonly Regex _ISKRegexISKData = new Regex("^<font size=12><b><color=0x........>\\s*(?<iskAmount>.*) ISK\\s*</b><color=0x........> added to next bounty payout", RegexOptions.Compiled);

        private Queue<LogEntry> _newIntelQueue = new Queue<LogEntry>();

        private int _intelSizeMax = 200;
        private int _intelSizeMin = 48;
        private int _windowExtraHeight = 60;

        private bool shouldUpdateMinimap = false;
        private bool shouldUpdatePlanetInfo = false;

        private MenuItem menuConfiguredAlerts = new MenuItem("Configured Alerts");
        private MenuItem menuBackgrounds = new MenuItem("Popup Background");
        private MenuItem menuLatestAlerts = new MenuItem("Latest Alerts");
        private MenuItem menuLogWatchers = new MenuItem("Logfiles");
        private MenuItem menuConfiguration = new MenuItem("Configuration");
        private MenuItem menuConfigurationWindow = new MenuItem("Edit Configuration");

        private MenuItem menuConfigurationAlwaysOnTop = new MenuItem("Always on top");

        private MenuItem menuConfigurationDPSShowDPS = new MenuItem("Show DPS");
        private MenuItem menuConfigurationDPSShowZkill = new MenuItem("Show ZKill Alerts");
        private MenuItem menuConfigurationDPSStayOnTop = new MenuItem("DPS Always on top");
        private MenuItem menuConfigurationDPSTransparent = new MenuItem("DPS Transparent");
        private MenuItem menuConfigurationAlertTransparent = new MenuItem("Alert Transparent");
        private MenuItem menuConfigurationDPSSaveCSV = new MenuItem("DPS Save CSV");
        private MenuItem menuConfigurationDPSShowAll = new MenuItem("DPS Combine");

        private EveScoutManager EveScoutProcessor;
        private DotlanManager DotlanProcessor;

        public bool AlertShowing = false;

        private bool ChatServerStatus = true;
        private bool InCombat = false;
        private bool CombatAdded = false;

        private string combatNeutOut = "ff7fffff";

        private bool ConfigurationWindowOpen = false;

        private List<PlanetInfoDisplay> CurrentPlanetInfos = new List<PlanetInfoDisplay>();

        public Dictionary<string, int> TickCounts = new Dictionary<string, int>();

        private Thread splashScreenThread;
        private QuesadillaSplashScreen splashScreen;
        public QuesadillaDPSGraph DPSGraphWindow = null;

        private Color OriginalBackgroundColour;
        private int mapScale = 100;

        private List<ProcessInfo> knowProcesses = new List<ProcessInfo>();
        public string CurrentActiveToon;

        // TODO - config things - really need to go into config structure later

        private NotifyIcon trayIcon;

        private DateTime LastFactionAlert = DateTime.Now;

        public Rect currentEveWindowRect = new Rect();
        public Point currentEveClientPoint = new Point(0, 0);

        private string ZKillRedisID = "Quesadilla_" + Guid.NewGuid().ToString();

        private List<MinimapSystem> MinimapSystems = new List<MinimapSystem>();
        private List<MinimapSystem> mouseOverMinimapSystems = new List<MinimapSystem>();

        private int activityLine = 0;
        private int ActivityLineInc = 1;
        private int ActivityLineMax = 1;

        AnimationState AnimationStateKills;
        AnimationState AnimationStateIntel;
        AnimationState AnimationStateAlts;

        private bool ISKPerHourTotal = false;
        private bool MiningPerHourTotal = false;

        public Quesadilla()
        {
            splashScreenThread = new Thread(new ThreadStart(ShowSplashScreen));
            splashScreenThread.Start();
            Thread.Sleep(2000);

            OriginalBackgroundColour = BackColor;

            InitializeComponent();
            Program.InitializeResources();

            this.pMinimapImage.MouseWheel += pMinimapImage_MouseWheel;


            // Initialize tray
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.q,
                ContextMenu = new ContextMenu(),
                Visible = true
            };

            trayIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                    menuConfiguration,
                    menuLatestAlerts,
                    new MenuItem("-"),
                    new MenuItem("Exit", new EventHandler(trayIconMenuClose)),
            });

            trayIcon.MouseDoubleClick += trayIconDoubleClick;

            TickCounts["SinceLastCleardown"] = 0;
            TickCounts["SinceLastAlert"] = 0;
            TickCounts["SinceLastDPSGraphUpdate"] = 0;
            TickCounts["LogfileCheck"] = 0;
            TickCounts["RefreshProcesses"] = 0;
            TickCounts["AnimationRefresh"] = 0;
            TickCounts["ISKPerHour"] = 0;
            TickCounts["MiningPerHour"] = 0;
            TickCounts["MiningDetail"] = 0;

            AnimationStateKills = new AnimationState(state:0, direction:-1, maximum:36, pulse:false);
            AnimationStateIntel = new AnimationState(state:1, direction:1, maximum:18, pulse:false); 
            AnimationStateAlts = new AnimationState(state:2, direction:-1, maximum:12, pulse:false);

            menuConfiguration.MenuItems.Add(menuConfigurationWindow = new MenuItem("Edit Configuration", trayIconMenuConfigurationClick));
            menuConfiguration.MenuItems.Add(new MenuItem("-"));

            menuConfiguration.MenuItems.Add(menuConfigurationAlwaysOnTop = new MenuItem("Always on top", trayIconMenuAlwaysOnTop) { Checked = Program.MainConfiguration.StayOnTop, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowDPS = new MenuItem("Show DPS", trayIconMenuDPSShowWindow) { Checked = Program.MainConfiguration.DPSShowWindow, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowZkill = new MenuItem("Show ZKill Alerts", trayIconMenuShowZKillAlerts) { Checked = Program.MainConfiguration.ShowZKillAlerts, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSStayOnTop = new MenuItem("DPS Always on top", trayIconMenuDPSAlwaysOnTop) { Checked = Program.MainConfiguration.DPSGraphStayOnTop, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSTransparent = new MenuItem("DPS Transparent", trayIconMenuDPSTransparent) { Checked = Program.MainConfiguration.DPSGraphTransparent, });
            menuConfiguration.MenuItems.Add(menuConfigurationAlertTransparent = new MenuItem("Alert Transparent", trayIconMenuAlertTransparent) { Checked = Program.MainConfiguration.AlertTransparent, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSSaveCSV = new MenuItem("DPS Save CSV", trayIconMenuDPSSaveCSV) { Checked = Program.MainConfiguration.DPSSaveCSV, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowAll = new MenuItem("DPS Combine", trayIconMenuDPSAllToons) { Checked = Program.MainConfiguration.DPSAllToons, });

            menuConfiguration.MenuItems.Add(menuBackgrounds);
            menuConfiguration.MenuItems.Add(menuConfiguredAlerts);
            menuConfiguration.MenuItems.Add(menuLogWatchers);

            // stop main tick timer!
            mainTimer.Enabled = false;

            ReloadConfiguration();

            EveScoutProcessor = new EveScoutManager();
            EveScoutProcessor.EveScoutEventHandler += new EventHandler<EveScoutEventArgs>(this.EveScout_ProcessNewData);

            DotlanProcessor = new DotlanManager();
            DotlanProcessor.DotlanEventHandler += new EventHandler<DotlanEventArgs>(this.Dotlan_ProcessNewData);

            Program.SetupLogfileWatchers();

            SetupLogfileEvents();

            RebuildLogfilesMenu();
            Program.StartLogfileWatchers();

            // start main tick timer
            mainTimer.Enabled = true;

            Program.ZKillHandler.ZKillFeed.ZKillEventHandler += new EventHandler<ZKillEventArgs>(this.ZKill_ProcessNewData);
            Program.ESIHandler.ESIEventHandler += new EventHandler<ESIEventArgs>(this.ESI_ProcessNewData);

            ActivityLineInc = 1;
            ActivityLineMax = Program.MainConfiguration.HeartBeatIndicatorWorkingText.Length - 1;

            // get processes
            UpdateEveProcesses(true);

        }

        private void EveScoutProcessor_EveScoutEventHandler(object sender, EveScoutEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ShowSplashScreen()
        {
            Application.Run(splashScreen = new QuesadillaSplashScreen());
        }
        private void RebuildLogfilesMenu()
        {
            menuLogWatchers.MenuItems.Clear();
            foreach (var watcher in Program.logfileWatchers)
            {
                MenuItem menuLogfile = new MenuItem(watcher.FriendlyName, trayIconMenuLogfileClick);
                menuLogfile.Checked = watcher.Enabled;
                menuLogWatchers.MenuItems.Add(menuLogfile);
            }
        }
        private void RebuildLatestAlertsMenu()
        {
            menuLatestAlerts.MenuItems.Clear();
            bool first = true;
            DateTime now = DateTime.Now;

            foreach (var alert in Program.ActiveAlerts.Reverse().Where(o => o.ShowAlert).Take(Program.MainConfiguration.ShowLastUnexpiredAlerts).ToList())
            {
                DateTime timeAdded = alert.Entry != null ? alert.Entry.TimeAdded : alert.Kill.TimeAdded;
                TimeSpan timeAgo = now - timeAdded;
                var timeAgoString = timeAgo.ToString();

                string menuText = timeAdded.ToString("HH:mm") + " " + (alert.Path != null ? alert.Path.ToSystemName : "") + " - " + alert.MenuText;
                string TrayString = alert.Kill == null ? (timeAdded.ToString("HH:mm") + " " + alert.Entry.LineContent) : menuText;
                if (TrayString.Length > 63)
                {
                    TrayString = TrayString.Substring(0, 63);
                }
                if (first)
                {
                    first = !first;
                    trayIcon.Text = TrayString;
                }
                MenuItem menuAlert = new MenuItem(menuText, trayIconMenuShowHistoricAlert) { Tag = alert };
                menuLatestAlerts.MenuItems.Add(menuAlert);
            }
        }
        private void RebuildAlertsMenu()
        {
            menuConfiguredAlerts.MenuItems.Clear();
            foreach (var alertConfigItem in Program.MainConfiguration.AlertConfiguration)
            {
                MenuItem menuAlert = new MenuItem(alertConfigItem.FriendlyName, trayIconMenuAlertClick);
                menuAlert.Checked = alertConfigItem.Enabled;
                menuConfiguredAlerts.MenuItems.Add(menuAlert);
            }
        }
        private void RebuildBackgroundsMenu()
        {
            menuBackgrounds.MenuItems.Clear();
            foreach (var background in Program.Backgrounds.OrderBy(o => o.BackgroundName).ToList())
            {
                MenuItem menuBackground = new MenuItem(background.BackgroundName, trayIconMenuBackgroundClick);
                if (Program.MainConfiguration.AlertPopupBackground == background.BackgroundName) menuBackground.Checked = true;
                menuBackgrounds.MenuItems.Add(menuBackground);
            }
        }
        public void ReloadConfiguration()
        {
            BackColor = Program.MainConfiguration.MainWindowBackgroundColour;
            ForeColor = Program.MainConfiguration.MainWindowTextColour;
            intelLog.ForeColor = Program.MainConfiguration.MainWindowLogTextColour;
            intelLog.BackColor = Program.MainConfiguration.MainWindowLogBackgroundColour;

            if (ISKPerHourTotal == true )
            {
                lblISK.ForeColor = Program.MainConfiguration.ISKTotalColour;
            }
            else
            {
                lblISK.ForeColor = Program.MainConfiguration.ISKColour;
            }

            if (MiningPerHourTotal == true)
            {
                lblMining.ForeColor = Program.MainConfiguration.MiningTotalColour;
            }
            else
            {
                lblMining.ForeColor = Program.MainConfiguration.MiningColour;
            }

            lblAlertsOutstanding.ForeColor = Program.MainConfiguration.AlertsOutstandingColour;
            lblCurrentClient.ForeColor = Program.MainConfiguration.CurrentCharacterColour;
            lblStatus.ForeColor = Program.MainConfiguration.EvescoutColour;
            lblEtxraInformation.ForeColor = Program.MainConfiguration.MainWindowExtraInformationColour;

            lblSystemName.ForeColor = Program.MainConfiguration.MainWindowMouseoverSystemColour;

            pMinimapImage.Height = Program.MainConfiguration.MinimapHeight;
            pMinimapImage.Width = (int)((double)Program.MainConfiguration.MinimapHeight * 1.3);

            working.Visible = Program.MainConfiguration.HeartBeatIndicatorVisible;
            working.ForeColor = Program.MainConfiguration.HeartBeatIndicatorColour;

            RebuildAlertsMenu();
            RebuildBackgroundsMenu();
            RebuildLatestAlertsMenu();

            if (Program.MainConfiguration.InitialPosition != null)
            {
                this.Location = Program.MainConfiguration.InitialPosition;
                if (this.Location.X > Screen.FromControl(this).Bounds.Width || this.Location.X < Screen.FromControl(this).Bounds.X ||
                    this.Location.Y > Screen.FromControl(this).Bounds.Height || this.Location.Y < Screen.FromControl(this).Bounds.Y
                    )
                {
                    this.Location = new System.Drawing.Point(0, 0);
                }

                //Width = Configuration.WindowSize.X >= 100 ? Configuration.WindowSize.X : 100;
                //Height = Configuration.WindowSize.Y >= 100 ? Configuration.WindowSize.Y : 100;
            }

            TopMost = Program.MainConfiguration.StayOnTop;
            menuConfigurationAlwaysOnTop.Checked = Program.MainConfiguration.StayOnTop;
            menuConfigurationDPSShowDPS.Checked = Program.MainConfiguration.DPSShowWindow;
            menuConfigurationDPSStayOnTop.Checked = Program.MainConfiguration.DPSGraphStayOnTop;
            menuConfigurationDPSTransparent.Checked = Program.MainConfiguration.DPSGraphTransparent;
            menuConfigurationDPSSaveCSV.Checked = Program.MainConfiguration.DPSSaveCSV;
            menuConfigurationDPSShowAll.Checked = Program.MainConfiguration.DPSAllToons;

            Program.Universe.HomeSolarSystemID = Program.Universe.Names[Program.MainConfiguration.HomeSystem];

            RecalculateWindowHeight();

            mainTimer.Interval = Program.MainConfiguration.TickMiliseconds;

            mapScale = Program.MainConfiguration.MainWindowMapScale;
            if (mapScale == 0) mapScale = 100;
        }
        private void Quesadilla_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            lblCurrentClient.Text = "";
            lblISK.Text = "";
            lblMining.Text = "";
            lblAlertsOutstanding.Text = "";
            lblEtxraInformation.Text = "";
            lblSystemName.Text = "";

            this.Invalidate();

            ReloadConfiguration();
            buttonIntelSize_Click(null, null);

            splashScreenThread.Interrupt();
            Thread.Sleep(100);
            splashScreenThread.Abort();
            Thread.Sleep(500);
            splashScreen = null;

            this.Text = "Quesadilla v" + Program.version.ToString();

            Program.ESIHandler.Resume();
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {

            activityLine += ActivityLineInc;
            if (activityLine > ActivityLineMax)
            {
                activityLine = 0;
            }
            working.Text = Program.MainConfiguration.HeartBeatIndicatorWorkingText[activityLine].ToString();

            if (++TickCounts["LogfileCheck"] > Program.MainConfiguration.HowManyTicksBeforeLogfileCheck)
            {
                Program.LogfileWatchersProcessing();
                TickCounts["LogfileCheck"] = 0;
            }

            if (shouldUpdateMinimap)
            {
                UpdateMinimap();
            }
            if (shouldUpdatePlanetInfo)
            {
                UpdatePlanetInfo();
            }

            // dequeue Alerts
            if (++TickCounts["SinceLastAlert"] > Program.MainConfiguration.HowManyTicksBeforeAlert && Program.NewAlerts.Count > 0)
            {

                AlertQueueItem NewAlert = null;
                Program.NewAlerts.TryDequeue(out NewAlert);

                Program.EnqueueActiveAlert(NewAlert);

                if (NewAlert.ShowAlert)
                {
                    Log.Error(NewAlert.AlertText);
                    shouldUpdateMinimap = true;
                    shouldUpdatePlanetInfo = true;

                    ShowAlert(NewAlert);

                    SoundPlayer sound = Program.LoadSoundFromFile(NewAlert.SoundFile);
                    if (sound != null) sound.Play();
                    RebuildLatestAlertsMenu();
                }

                TickCounts["SinceLastAlert"] = 0;
            }

            if (++TickCounts["ISKPerHour"] > Program.MainConfiguration.HowManyTicksBeforeSwitchISKPerHour)
            {
                ISKPerHourTotal = !ISKPerHourTotal;
                TickCounts["ISKPerHour"] = 0;

                // if not configured - then reset to false;
                if (Program.MainConfiguration.ShowISKTotal == false)
                {
                    ISKPerHourTotal = false;
                }
                else
                {
                    UpdateISK();
                }
            }

            if (++TickCounts["MiningDetail"] > Program.MainConfiguration.HowManyTicksMiningCSVOutput)
            {
                TickCounts["MiningDetail"] = 0;
                UpdateMiningCSV();
            }

            if (++TickCounts["MiningPerHour"] > Program.MainConfiguration.HowManyTicksBeforeSwitchMiningPerHour)
            {
                MiningPerHourTotal = !MiningPerHourTotal;
                TickCounts["MiningPerHour"] = 0;

                // if not configured - then reset to false;
                if (Program.MainConfiguration.ShowMiningTotal == false)
                {
                    MiningPerHourTotal = false;
                }
                else
                {
                    UpdateMining();
                }
            }

            // do cleardown/rebuild of alerts menu - if number of ticks passed
            if (++TickCounts["SinceLastCleardown"] > Program.MainConfiguration.HowManyTicksBeforeCleardown)
            {

                int alertsExpired = 0;
                int zkillsExpired = 0;
                // Show number of recent alerts
                if (Program.ActiveAlerts.Count > 0)
                {

                    alertsExpired = Program.ExpireAlerts();
                    if (alertsExpired > 0)
                    {
                        RebuildLatestAlertsMenu();
                    }
                    lblAlertsOutstanding.Text = Program.ActiveAlerts.Count.ToString();
                }
                else
                {
                    lblAlertsOutstanding.Text = "";
                }

                if (Program.ZKillHandler.ZKillKills.Count > 0)
                {
                    lblAlertsOutstanding.Text += "/" + Program.ZKillHandler.ZKillKills.Count.ToString();
                }

                Program.ExpireCombat();
                Program.ExpireISK();
                Program.ExpireMining();

                zkillsExpired = Program.ZKillHandler.ExpireZKillQueue();
                if (zkillsExpired + alertsExpired > 0)
                {
                    UpdateMinimap();
                }

                UpdateISK();
                UpdateMining();

                TickCounts["SinceLastCleardown"] = 0;
            }

            bool refreshProcessList = false;
            if (++TickCounts["RefreshProcesses"] > Program.MainConfiguration.HowManyTicksRefreshProcesses)
            {
                refreshProcessList = true;
                TickCounts["RefreshProcesses"] = 0;
            }
            UpdateEveProcesses(refreshProcessList);

            //            pCombat.Visible = InCombat;

            if (++TickCounts["SinceLastDPSGraphUpdate"] > Program.MainConfiguration.HowManyTicksBeforeDPSGraphUpdate && CombatAdded)
            {
                if (Program.MainConfiguration.DPSShowWindow && DPSGraphWindow == null)
                {
                    DPSGraphWindow = new QuesadillaDPSGraph(this);
                    DPSGraphWindow.Show();
                }

                if (DPSGraphWindow != null)
                {
                    DPSGraphWindow.UpdateDPSGraph();
                }
                CombatAdded = false;
                TickCounts["SinceLastDPSGraphUpdate"] = 0;
            }

            if (++TickCounts["AnimationRefresh"] > Program.MainConfiguration.HowManyTicksMapAnimation)
            {
                AnimationStateKills.Advance();
                AnimationStateIntel.Advance();
                AnimationStateAlts.Advance();

                UpdateMinimap();
                TickCounts["AnimationRefresh"] = 0;
            }
        }

        private bool checkJumpsRange(AlertConfig alert, PathInfo path)
        {
            bool checkJumpsRangeComparison(int jumps, AlertConfigJumpsComparison comparison, int? comparisonNumber)
            {
                if (comparison == AlertConfigJumpsComparison.None) return true;
                if (comparisonNumber is null) return false;

                switch (comparison)
                {
                    case AlertConfigJumpsComparison.Equal:
                        if (jumps == comparisonNumber) return true;
                        break;
                    case AlertConfigJumpsComparison.LessThan:
                        if (jumps < comparisonNumber) return true;
                        break;
                    case AlertConfigJumpsComparison.LessThanEqual:
                        if (jumps <= comparisonNumber) return true;
                        break;
                    case AlertConfigJumpsComparison.GreaterThan:
                        if (jumps > comparisonNumber) return true;
                        break;
                    case AlertConfigJumpsComparison.GreaterThanEqual:
                        if (jumps >= comparisonNumber) return true;
                        break;
                    default:
                        return false;
                        break;
                }
                return false;
            }
            bool shouldAlert = false;
            if (!alert.Enabled) return false;

            if (path == null) return false;

            shouldAlert =
                checkJumpsRangeComparison(path.TotalJumps, alert.Jumps1Comparison, alert.Jumps1Number) &&
                checkJumpsRangeComparison(path.TotalJumps, alert.Jumps2Comparison, alert.Jumps2Number) &&
                path.TotalJumps < Program.MainConfiguration.IgnoreAsTooManyJumps;

            return shouldAlert;
        }
        private string generateAlertText(LogEntry entry, PathInfo path, AlertConfig alert, string characterName, int type, string context)
        {
            // type == 0 - Alert Text, 1 == Menu text
            string returnString = "";
            switch (type)
            {
                case 0:
                    returnString = alert.TextDisplay;
                    break;
                case 1:
                    returnString = alert.MenuDisplay;
                    break;
            }

            if (path != null)
            {
                returnString = returnString.Replace("{jumps}", path.TotalJumps.ToString());
                returnString = returnString.Replace("{route}", Program.Universe.PathToString(path));
                returnString = returnString.Replace("{tosystem}", Program.Universe.SolarSystems[path.ToSystemId].Name);
                returnString = returnString.Replace("{region}", Program.Universe.SolarSystems[path.ToSystemId].RegionName);
                returnString = returnString.Replace("{fromsystem}", Program.Universe.SolarSystems[path.FromSystemId].Name);
            }
            returnString = returnString.Replace("{character}", characterName);
            returnString = returnString.Replace("{text}", entry.LineContent);
            returnString = returnString.Replace("{context}", context);
            returnString = returnString.Replace("{reporter}", entry.PlayerName);
            return returnString;
        }
        private string generateZKillAlertText(ZKillKill kill, PathInfo path, string zkilltext, string characterName)
        {
            // type == 0 - Alert Text, 1 == Menu text
            string returnString = zkilltext;

            if (path != null)
            {
                returnString = returnString.Replace("{jumps}", path.TotalJumps.ToString());
                returnString = returnString.Replace("{route}", Program.Universe.PathToString(path));
                returnString = returnString.Replace("{tosystem}", Program.Universe.SolarSystems[path.ToSystemId].Name);
                returnString = returnString.Replace("{region}", Program.Universe.SolarSystems[path.ToSystemId].RegionName);
                returnString = returnString.Replace("{fromsystem}", Program.Universe.SolarSystems[path.FromSystemId].Name);
            }
            returnString = returnString.Replace("{character}", characterName);
            returnString = returnString.Replace("{text}", kill.href);
            returnString = returnString.Replace("{value}", kill.TotalValue);
            returnString = returnString.Replace("{victim}", kill.VictimCharacterName);
            returnString = returnString.Replace("{system}", Program.Universe.SolarSystems.ContainsKey((int)kill.SolarSystemId) ? Program.Universe.SolarSystems[(int)kill.SolarSystemId].Name : kill.SolarSystemId.ToString());
            returnString = returnString.Replace("{killer}", kill.KillerCharacterId == 0 ? "NPC" : Program.Universe.GetCharacterName((int)kill.KillerCharacterId));
            returnString = returnString.Replace("{victimship}", Program.Universe.ItemTypes.ContainsKey((int)kill.VictimShipTypeId) ? Program.Universe.ItemTypes[(int)kill.VictimShipTypeId].Name : "Unknown:" + kill.VictimShipTypeId.ToString());

            returnString = returnString.Replace("{killership}", Program.Universe.ItemTypes.ContainsKey((int)kill.KillerShipTypeId) ? Program.Universe.ItemTypes[(int)kill.KillerShipTypeId].Name : kill.KillerShipTypeId.ToString());
            returnString = returnString.Replace("{killerweapon}", kill.KillerWeaponTypeId.ToString());
            return returnString;
        }
        private void logFile_ProcessNewData(object sender, ProcessNewDataEventArgs e)
        {
            LogEntry entry = e.LogEntry;
            int alertCount = 0;

            if (entry == null || entry.PlayerName == "EVE System")
                return;
            if (entry.EntryType != LogEntryType.ChatEvent && entry.EntryType != LogEntryType.UnknownChatLogEvent && entry.EntryType != LogEntryType.UnknownGameLogEvent)
            {
                if (Program.CharacterList.Where(o => o.CharacterName == entry.CharacterName).ToList().Count == 0)
                    Program.AddNewCharacter(entry.CharacterName);
                switch (entry.EntryType)
                {
                    case LogEntryType.OpenChatLogEvent:
                        //if (!this._conf.DisplayOpenFileAlerts) break;
                        Log.Debug(e.LogPrefix + " > Chat log opened: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.NewChatLogEvent:
                        //if (!this._conf.DisplayNewFileAlerts) break;
                        Log.Debug(e.LogPrefix + " > New chat log detected: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.OpenGameLogEvent:
                        //if (!this._conf.DisplayOpenFileAlerts) break;
                        Log.Debug(e.LogPrefix + " > Game log opened: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.NewGameLogEvent:
                        //if (!this._conf.DisplayNewFileAlerts) break;
                        Log.Debug(e.LogPrefix + " > New game log detected: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                }
            }
            else
            {
                if (!entry.ParseSuccess)
                    return;
                HashSet<int> intSet = new HashSet<int>();
                if (!Program.MainConfiguration.ignoreStrings.Any<Regex>((Func<Regex, bool>)(ignoreString => ignoreString.IsMatch(entry.LineContent))) && entry.LogType == LogFileType.ChatIntel)
                {
                    // try and find systems mentioned in intel
                    foreach (SolarSystem solarSystem in
                        Program.Universe.SolarSystems.Values.Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => tempSystem.MatchNameRegex(entry.LineContent)))
                        .Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => !Program.MainConfiguration.ignoreSystems.Contains(Program.Universe.Names[tempSystem.Name])))
                        )
                        intSet.Add(Program.Universe.Names[solarSystem.Name]);
                }

                if (intSet.Count > 0)
                {

                    AlertQueueItem insertedAlert = null;

                    Log.Information(entry.LineContent);
                    entry.MatchedIds = intSet;
                    foreach (int toSystemId in intSet)
                    {
                        int homeSystemId = Program.Universe.HomeSolarSystemID;

                        // Cache paths - if not already stored
                        var homeSystemPathInfo = Program.Universe.FindAndCachePath(homeSystemId, toSystemId);
                        if (homeSystemPathInfo != null)
                        {
                            Log.Debug("Jumps from HOME:" + Program.Universe.SolarSystems[homeSystemId].Name + " to " + Program.Universe.SolarSystems[toSystemId].Name + " is " + homeSystemPathInfo.TotalJumps);
                        }

                        foreach (var c in Program.CharacterList.Where(o => o.LocationId > 0))
                        {
                            var CurrectLocationPathInfo = Program.Universe.FindAndCachePath(c.LocationId, toSystemId);
                            if (CurrectLocationPathInfo != null)
                            {
                                Log.Debug("Jumps from " + c.CharacterName + ":" + Program.Universe.SolarSystems[c.LocationId].Name + " to " + Program.Universe.SolarSystems[toSystemId].Name + " is " + CurrectLocationPathInfo.TotalJumps);
                            }
                        }

                        // Range alerts
                        foreach (AlertConfig alert in Program.MainConfiguration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
                        {
                            if (!tempTrigger.Enabled)
                                return false;
                            return tempTrigger.AlertType == AlertConfigType.Range;
                        })))
                        {
                            bool sendAlert = false;
                            PathInfo path = new PathInfo();
                            string alertText = "";

                            switch (alert.AlertRangeType)
                            {
                                case AlertConfigRangeType.Home:
                                    path = homeSystemPathInfo;
                                    sendAlert = checkJumpsRange(alert, path);
                                    if (sendAlert)
                                    {
                                        alertText = generateAlertText(entry, path, alert, "", 0, "");
                                        Log.Debug("ALERT - " + alertText);
                                        insertedAlert = new AlertQueueItem()
                                        {
                                            AlertText = alertText,
                                            MenuText = generateAlertText(entry, path, alert, "", 1, ""),
                                            Entry = entry,
                                            Path = path,
                                            Context = homeSystemPathInfo.FromSystemName,
                                            SoundFile = alert.SoundFile,
                                            FriendlyName = alert.FriendlyName,
                                            ShowAlert = true,
                                        };
                                        Program.EnqueueNewAlert(insertedAlert, Program.NewAlerts);
                                        alertCount++;
                                    }
                                    break;

                                case AlertConfigRangeType.System:
                                    // check against specific system
                                    path = Program.Universe.FindAndCachePath(Program.Universe.Names[alert.TriggerText], toSystemId);
                                    sendAlert = checkJumpsRange(alert, path);
                                    if (sendAlert)
                                    {
                                        alertText = generateAlertText(entry, path, alert, "", 0, "");
                                        Log.Debug("ALERT - " + alertText);
                                        insertedAlert = new AlertQueueItem()
                                        {
                                            AlertText = alertText,
                                            MenuText = generateAlertText(entry, path, alert, "", 1, ""),
                                            Entry = entry,
                                            Path = path,
                                            Context = path.FromSystemName,
                                            SoundFile = alert.SoundFile,
                                            FriendlyName = alert.FriendlyName,
                                            ShowAlert = true,
                                        };
                                        Program.EnqueueNewAlert(insertedAlert, Program.NewAlerts);
                                        alertCount++;
                                    }
                                    break;
                                case AlertConfigRangeType.OneCharacter:
                                case AlertConfigRangeType.OneOnlineCharacter:
                                    // check against one specific character
                                    foreach (var x in Program.CharacterList.Where(o => o.CharacterName == alert.TriggerText))
                                    {
                                        if (x.LocationId != 0)
                                        {
                                            path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);
                                            sendAlert = checkJumpsRange(alert, path);
                                            if (sendAlert)
                                            {
                                                alertText = generateAlertText(entry, path, alert, x.CharacterName, 0, "");
                                                Log.Debug("ALERT - " + alertText);
                                                if (
                                                    alert.AlertRangeType == AlertConfigRangeType.OneCharacter ||
                                                    alert.AlertRangeType == AlertConfigRangeType.OneOnlineCharacter && x.IsClientActive
                                                    )
                                                {
                                                    insertedAlert = new AlertQueueItem()
                                                    {
                                                        AlertText = alertText,
                                                        MenuText = generateAlertText(entry, path, alert, x.CharacterName, 1, ""),
                                                        Entry = entry,
                                                        Path = path,
                                                        Context = x.CharacterName,
                                                        SoundFile = alert.SoundFile,
                                                        FriendlyName = alert.FriendlyName,
                                                        ShowAlert = true,
                                                    };
                                                    Program.EnqueueNewAlert(insertedAlert, Program.NewAlerts);

                                                    alertCount++;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case AlertConfigRangeType.AllCharacters:
                                case AlertConfigRangeType.AllOnlineCharacters:
                                    // check against all characters
                                    foreach (var x in Program.CharacterList)
                                    {
                                        if (x.LocationId != 0)
                                        {
                                            path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);
                                            sendAlert = checkJumpsRange(alert, path);
                                            if (sendAlert)
                                            {
                                                alertText = generateAlertText(entry, path, alert, x.CharacterName, 0, "");
                                                Log.Debug("ALERT - " + alertText);
                                                if (
                                                    alert.AlertRangeType == AlertConfigRangeType.AllCharacters ||
                                                    alert.AlertRangeType == AlertConfigRangeType.AllOnlineCharacters && x.IsClientActive
                                                    )
                                                {
                                                    insertedAlert = new AlertQueueItem()
                                                    {
                                                        AlertText = alertText,
                                                        MenuText = generateAlertText(entry, path, alert, x.CharacterName, 1, ""),
                                                        Entry = entry,
                                                        Path = path,
                                                        Context = x.CharacterName,
                                                        SoundFile = alert.SoundFile,
                                                        FriendlyName = alert.FriendlyName,
                                                        ShowAlert = true,
                                                    };
                                                    Program.EnqueueNewAlert(insertedAlert, Program.NewAlerts);

                                                    alertCount++;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case AlertConfigRangeType.None:
                                    // must be custom text
                                    if (entry.LineContent.Contains(alert.TriggerText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        foreach (var x in Program.CharacterList)
                                        {
                                            path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);

                                            alertText = generateAlertText(entry, path, alert, alert.TriggerText, 0, alert.TriggerText);
                                            Log.Debug("ALERT - " + alertText);
                                            insertedAlert = new AlertQueueItem()
                                            {
                                                AlertText = alertText,
                                                MenuText = generateAlertText(entry, path, alert, alert.TriggerText, 1, alert.TriggerText),
                                                Entry = entry,
                                                Path = path,
                                                Context = alert.TriggerText,
                                                SoundFile = alert.SoundFile,
                                                FriendlyName = alert.FriendlyName,
                                                ShowAlert = true,
                                            };
                                            Program.EnqueueNewAlert(insertedAlert, Program.NewAlerts);

                                            alertCount++;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    int systemId1 = -1;
                    foreach (int systemId2 in intSet)
                    {
                        if (insertedAlert == null)
                        {
                            insertedAlert = new AlertQueueItem()
                            {
                                AlertText = "",
                                MenuText = "",
                                Entry = entry,
                                Path = Program.Universe.FindAndCachePath(systemId2, systemId2),
                                Context = "",
                                SoundFile = "",
                                FriendlyName = "",
                                ShowAlert = false,
                            };
                            Program.EnqueueNewAlert(insertedAlert, Program.NewAlerts);
                        }

                        //Program.Universe.AddAlert(systemId2, insertedAlert);
                        systemId1 = systemId2;
                    }
                    //                    if (this.CheckZoomTo(entry.LogPrefix)) this.ZoomTo(systemId1);
                }

                // check against custom alerts (no system name needed) - only if no alerts generated
                if (alertCount == 0)
                {
                    foreach (AlertConfig alert in Program.MainConfiguration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
                    {
                        if (!tempTrigger.Enabled)
                            return false;
                        return tempTrigger.AlertType == AlertConfigType.Text;
                    })))
                    {
                        if (entry.LineContent.Contains(alert.TriggerText, StringComparison.OrdinalIgnoreCase))
                        {

                            string alertText = generateAlertText(entry, null, alert, alert.TriggerText, 0, alert.TriggerText);
                            Log.Debug("ALERT - " + alertText);
                            Program.EnqueueNewAlert(
                                new AlertQueueItem()
                                {
                                    AlertText = alertText,
                                    MenuText = generateAlertText(entry, null, alert, alert.TriggerText, 1, alert.TriggerText),
                                    Entry = entry,
                                    Path = null,
                                    Context = alert.TriggerText,
                                    SoundFile = alert.SoundFile,
                                    FriendlyName = alert.FriendlyName,
                                    ShowAlert = true,
                                }, Program.NewAlerts
                                );
                            alertCount++;
                        }
                    }
                }
            }
        }

        private void _localWatcher_SystemChange(object sender, ProcessSystemChangeEventArgs e)
        {
            int foundSystemId = -1;
            string foundSystemName = "Unknown";

            int nativeId;
            if (int.TryParse(e.SystemName, out nativeId))
            {
                using (IEnumerator<KeyValuePair<int, SolarSystem>> enumerator = Program.Universe.SolarSystems.Select(tempSolarSystem => new
                {
                    tempSolarSystem = tempSolarSystem,
                    tempSystem = tempSolarSystem.Value
                }).Where(_param1 => _param1.tempSystem.Id == nativeId).Select(_param1 => _param1.tempSolarSystem).GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        foundSystemId = enumerator.Current.Key;
                        foundSystemName = enumerator.Current.Value.Name;
                    }
                }
            }
            else
            {
                using (IEnumerator<KeyValuePair<int, SolarSystem>> enumerator = Program.Universe.SolarSystems.Select(tempSolarSystem => new
                {
                    tempSolarSystem = tempSolarSystem,
                    tempSystem = tempSolarSystem.Value
                }).Where(_param1 => _param1.tempSystem.MatchNameRegex(e.SystemName)).Select(_param1 => _param1.tempSolarSystem).GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        foundSystemId = enumerator.Current.Key;
                        foundSystemName = enumerator.Current.Value.Name;
                    }
                }
            }
            string characterName = e.CharName.Trim();

            if (!string.IsNullOrEmpty(characterName) && foundSystemId != -1)
            {
                var c = Program.CharacterList.Where(o => o.CharacterName == characterName).FirstOrDefault();
                if (c != null)
                {
                    c.LocationId = foundSystemId;
                    c.Location = foundSystemName;
                }
                else
                {
                    c = new Character()
                    {
                        CharacterName = characterName,
                        LocationId = foundSystemId,
                        Location = foundSystemName,
                    };
                    Program.CharacterList.Add(c);
                }

                // add to list of regions to check on dotlan
                string region = Program.Universe.SolarSystems[foundSystemId].RegionName;
                if (!DotlanProcessor.RegionsToCheck.Contains(region))
                {
                    DotlanProcessor.RegionsToCheck.Add(region);
                    DotlanProcessor.RunWorkerIfPossible();
                }

                CheckEveScoutConnection(c);

                GetSystemInfo(foundSystemId);

                // TODO                if (this._conf.CameraFollowCharacter && this.CentreOnCharacter.Text == key) this.ZoomTo(num1);
            }

            SetupStatusBar();

            if (foundSystemId == -1)
                return;
            Log.Information("System Change: " + characterName + " (" + foundSystemName + ")");


            shouldUpdatePlanetInfo = true;
            shouldUpdateMinimap = true;
        }

        private void GetSystemInfo(int systemid)
        {
            // check if we need to pull data from ESI for system (planets/moons/belts)
            if (Program.Universe.SolarSystems[systemid].Planets == null)
            {
                Program.ESIHandler.EnqueueRequestSystem(systemid);
            }
        }

        private void _localWatcher_ChatServerChange(object sender, ProcessChatServerChangeEventArgs e)
        {
            Log.Information("Chat Server: " + (e.ChatServerUp == true ? "Up" : "Down"));
            ChatServerStatus = e.ChatServerUp;
            ChatServerDown.Visible = !e.ChatServerUp;
        }

        private void Quesadilla_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.MainConfiguration.InitialPosition = Location;
            Program.MainConfiguration.WindowSize = new System.Drawing.Point(Width, Height);
            Program.MainConfiguration.StayOnTop = TopMost;
            Program.Universe.WriteESICharacters(Program.CHARACTER_CACHE_FILE);
        }


        private void trayIconMenuClose(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void trayIconMenuAlwaysOnTop(object sender, EventArgs e)
        {
            Program.MainConfiguration.StayOnTop = !Program.MainConfiguration.StayOnTop;
            TopMost = Program.MainConfiguration.StayOnTop;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSShowWindow(object sender, EventArgs e)
        {
            Program.MainConfiguration.DPSShowWindow = !Program.MainConfiguration.DPSShowWindow;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
            if (DPSGraphWindow != null)
            {
                if (!((MenuItem)sender).Checked)
                {
                    DPSGraphWindow.Close();
                    DPSGraphWindow = null;

                    DPSGraphWindow = new QuesadillaDPSGraph(this);
                    DPSGraphWindow.Show();
                }
            }
        }
        private void trayIconMenuShowZKillAlerts(object sender, EventArgs e)
        {
            Program.MainConfiguration.ShowZKillAlerts = !Program.MainConfiguration.ShowZKillAlerts;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSAlwaysOnTop(object sender, EventArgs e)
        {
            Program.MainConfiguration.DPSGraphStayOnTop = !Program.MainConfiguration.DPSGraphStayOnTop;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
            if (DPSGraphWindow != null) DPSGraphWindow.TopMost = Program.MainConfiguration.DPSGraphStayOnTop;
        }
        private void trayIconMenuDPSSaveCSV(object sender, EventArgs e)
        {
            Program.MainConfiguration.DPSSaveCSV = !Program.MainConfiguration.DPSSaveCSV;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSAllToons(object sender, EventArgs e)
        {
            Program.MainConfiguration.DPSAllToons = !Program.MainConfiguration.DPSAllToons;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;

            if (InCombat && DPSGraphWindow != null && Program.MainConfiguration.DPSShowWindow)
            {
                DPSGraphWindow.Close();
                DPSGraphWindow = null;
            }
        }

        private void trayIconMenuDPSTransparent(object sender, EventArgs e)
        {
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
            Program.MainConfiguration.DPSGraphTransparent = ((MenuItem)sender).Checked;

            if (DPSGraphWindow != null)
            {
                switch (((MenuItem)sender).Checked)
                {
                    case true:
                        DPSGraphWindow.TransparencyKey = DPSGraphWindow.BackColor = DPSGraphWindow.TransparentColour;
                        DPSGraphWindow.Text = "";
                        // the color key to transparent, choose a color that you don't use
                        break;
                    case false:
                        DPSGraphWindow.TransparencyKey = DPSGraphWindow.TransparentColour;
                        DPSGraphWindow.BackColor = OriginalBackgroundColour;
                        DPSGraphWindow.Text = "Queadilla DPS";
                        break;
                }
            }
        }
        private void trayIconMenuAlertTransparent(object sender, EventArgs e)
        {
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
            Program.MainConfiguration.AlertTransparent = ((MenuItem)sender).Checked;
        }


        private void trayIconMenuConfiguration(object sender, EventArgs e)
        {
            //
        }

        private void trayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
        private void trayIconMenuAlertClick(object sender, EventArgs e)
        {
            // enable / disable config item
            MenuItem m = (MenuItem)sender;
            var configAlert = Program.MainConfiguration.AlertConfiguration.Find(o => o.FriendlyName == m.Text);
            if (configAlert != null)
            {
                m.Checked = !m.Checked;
                configAlert.Enabled = !configAlert.Enabled;
            }
        }

        private void trayIconDoubleClick(object sender, EventArgs e)
        {
            if (Program.ActiveAlerts.Where(o => o.ShowAlert).Count() > 0)
            {
                var alert = Program.ActiveAlerts.Where(o => o.ShowAlert).Reverse().Take(1).ToList().First();
                //var NewAlertWindow = new QuesadillaAlertPopup(alert, Program.MainConfiguration, Program.LoadImageFromFile(Program.MainConfiguration.AlertPopupBackground), this);
                //NewAlertWindow.Show(this);
                ShowAlert(alert);
            }
        }

        private void trayIconMenuConfigurationClick(object sender, EventArgs e)
        {
            if (!ConfigurationWindowOpen)
            {

                Program.MainConfiguration.InitialPosition = Location;
                Program.MainConfiguration.WindowSize = new System.Drawing.Point(Width, Height);
                Program.MainConfiguration.StayOnTop = TopMost;

                var ConfigurationWindow = new QuesadillaConfiguration(Program.MainConfiguration, menuConfigurationWindow, this);
                ((MenuItem)sender).Enabled = !((MenuItem)sender).Enabled;
                ConfigurationWindow.Show(this);
            }
        }

        private void trayIconMenuLogfileClick(object sender, EventArgs e)
        {
            // enable / disable config item
            MenuItem m = (MenuItem)sender;
            var watcher = Program.logfileWatchers.Find(o => o.FriendlyName == m.Text);
            //Configuration.AlertConfiguration.Find(o => o.FriendlyName == m.Text);
            if (watcher != null)
            {
                m.Checked = !m.Checked;
                watcher.Enabled = !watcher.Enabled;
            }
        }
        private void trayIconMenuBackgroundClick(object sender, EventArgs e)
        {
            // enable clicked background
            MenuItem m = (MenuItem)sender;
            foreach (MenuItem z in menuBackgrounds.MenuItems)
            {
                z.Checked = false;
            }
            m.Checked = true;
            Program.MainConfiguration.AlertPopupBackground = m.Text;
        }
        private void logFile_ProcessCombat(object sender, ProcessCombatEventArgs e)
        {

            string combatEntity = "";

            //Log.Information("COMBAT:" + e.CombatEvent);
            if (e.Entry != null)
            {
                if (InCombat == false && Program.MainConfiguration.DPSShowWindow)
                {
                    if (DPSGraphWindow == null)
                    {
                        DPSGraphWindow = new QuesadillaDPSGraph(this);
                    }
                    DPSGraphWindow.Show();
                }
                if (InCombat == false)
                {
                    InCombat = true;
                    this.Invalidate();
                }
                else
                {
                    InCombat = true;
                }

                if (!Program.Combat.ContainsKey(e.Entry.CharacterName))
                {
                    Program.Combat.Add(e.Entry.CharacterName, new List<CombatLine>());
                }
                //Log.Information(e.Entry.LineContent);

                Match match = _CombatRegexMissesYouCompletely.Match(e.Entry.LineContent);
                if (match.Success)
                {
                    Program.Combat[e.Entry.CharacterName].Add(new CombatLine()
                    {
                        TimeAdded = e.Entry.TimeAdded,
                        Entity = match.Groups["combatEntity"].ToString(),
                    });
                }
                match = _CombatRegexCombatData.Match(e.Entry.LineContent);
                if (match.Success)
                {
                    int dmgAmount = 0;
                    int neutAmount = 0;
                    int capAmount = 0;
                    int shieldAmount = 0;
                    int armorAmount = 0;
                    int hullAmount = 0;

                    var amount = match.Groups["combatAmount"].ToString();
                    bool received = true;
                    if (amount.Contains("GJ"))
                    {
                        neutAmount = Int32.Parse(amount.Replace(" GJ", "").Trim());
                        if (match.Groups["combatColour"].ToString().Equals(combatNeutOut))
                        {
                            received = false;
                        }
                    }
                    else
                    {

                        if (match.Groups["combatContext"].ToString() == "to")
                        {
                            received = false;
                        }

                        if (match.Groups["combatContext"].ToString() == "to" || match.Groups["combatContext"].ToString() == "from")
                        {
                            dmgAmount = Int32.Parse(amount.Trim());
                        }
                        else
                        {
                            if (match.Groups["combatContext"].ToString().StartsWith("remote shield boosted "))
                            {
                                shieldAmount = Int32.Parse(amount.Trim());
                            }
                            if (match.Groups["combatContext"].ToString().StartsWith("remote armor boosted "))
                            {
                                armorAmount = Int32.Parse(amount.Trim());
                            }
                            if (match.Groups["combatContext"].ToString().StartsWith("remote hull boosted "))
                            {
                                hullAmount = Int32.Parse(amount.Trim());
                            }
                            if (match.Groups["combatContext"].ToString().StartsWith("remote capacitor transmitted "))
                            {
                                capAmount = Int32.Parse(amount.Trim());
                            }

                            if (match.Groups["combatContext"].ToString().Trim().EndsWith("to"))
                            {
                                received = false;
                            }
                        }
                    }

                    Program.Combat[e.Entry.CharacterName].Add(new CombatLine()
                    {
                        TimeAdded = e.Entry.TimeAdded,
                        Entity = match.Groups["combatEntity"].ToString(),
                        Text = match.Groups["combatText"].ToString(),
                        NeutAmount = neutAmount,
                        DamageAmount = dmgAmount,
                        CapAmount = capAmount,
                        ShieldAmount = shieldAmount,
                        ArmorAmount = armorAmount,
                        HullAmount = hullAmount,
                        Received = received,
                    });

                    // check if faction 
                    foreach (AlertConfig alert in Program.MainConfiguration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
                    {
                        if (!tempTrigger.Enabled)
                            return false;
                        return tempTrigger.AlertType == AlertConfigType.FactionSpawn;
                    })))
                    {
                        // active alert config for faction spawn


                        if (_FactionRats.Any(s => match.Groups["combatEntity"].ToString().Contains(s)))
                        {
                            string alertText = generateAlertText(e.Entry, null, alert, "Faction Spawn", 0, match.Groups["combatEntity"].ToString());
                            PathInfo path = null;
                            var c = Program.CharacterList.Find(o => o.CharacterName.Equals(e.CharacterName));

                            bool sendAlert = true;

                            if (c != null)
                            {
                                path = new PathInfo()
                                {
                                    FromSystemId = c.LocationId,
                                    ToSystemId = c.LocationId,
                                    FromSystemName = c.Location,
                                    ToSystemName = c.Location,
                                    ToSystemRegionName = Program.Universe.SolarSystems[c.LocationId].RegionName,
                                    PathSystems = new List<int>() { c.LocationId },
                                };

                                if (c.LastFactionAlert.AddSeconds(alert.CustomTriggerEverySeconds == null ? 0 : (double)alert.CustomTriggerEverySeconds) > DateTime.Now) { sendAlert = false; }
                            }
                            else
                            {
                                if (LastFactionAlert.AddSeconds(alert.CustomTriggerEverySeconds == null ? 0 : (double)alert.CustomTriggerEverySeconds) > DateTime.Now) { sendAlert = false; }
                            }

                            if (sendAlert)
                            {
                                LastFactionAlert = DateTime.Now;
                                if (c != null) { c.LastFactionAlert = LastFactionAlert; }
                                Log.Debug("ALERT - " + alertText);
                                Program.EnqueueNewAlert(
                                    new AlertQueueItem()
                                    {
                                        AlertText = alertText,
                                        MenuText = generateAlertText(e.Entry, null, alert, "Faction spawn", 1, match.Groups["combatEntity"].ToString()),
                                        Entry = e.Entry,
                                        Path = path,
                                        Context = match.Groups["combatEntity"].ToString(),
                                        SoundFile = alert.SoundFile,
                                        FriendlyName = alert.FriendlyName,
                                        isFaction = true,
                                        ShowAlert = true,
                                    }, Program.NewAlerts
                                    );
                            }
                        }
                    }
                }
                CombatAdded = true;
            }
            else
            {
                InCombat = false;
                this.Invalidate();
                if (DPSGraphWindow != null)
                {
                    DPSGraphWindow.Close();
                    DPSGraphWindow = null;
                };

            }
        }
        private void logFile_ProcessISK(object sender, ProcessISKEventArgs e)
        {
            //Log.Information("COMBAT:" + e.CombatEvent);
            if (e.Entry != null)
            {
                if (!Program.ISK.ContainsKey(e.Entry.CharacterName))
                {
                    Program.ISK.Add(e.Entry.CharacterName, new List<IskLine>());
                }

                Match match = _ISKRegexISKData.Match(e.Entry.LineContent);
                if (match.Success)
                {
                    double iskAmount = 0;

                    var amount = match.Groups["iskAmount"].ToString();
                    iskAmount = Double.Parse(amount.Trim());

                    Program.ISK[e.Entry.CharacterName].Add(new IskLine()
                    {
                        TimeAdded = e.Entry.TimeAdded,
                        IskAmount = iskAmount,
                    });
                }
            }
        }
        private void logFile_ProcessMining(object sender, ProcessMiningEventArgs e)
        {
            //Log.Information("COMBAT:" + e.CombatEvent);
            if (e.Entry != null)
            {
                if (!Program.Mining.ContainsKey(e.Entry.CharacterName))
                {
                    Program.Mining.Add(e.Entry.CharacterName, new List<MiningLine>());
                }


                // is this actual mining data
                Match match = _MiningRegexAmounts.Match(e.Entry.LineContent);
                if (match.Success)
                {
                    var minedAmount = match.Groups["miningAmount"].ToString();
                    int mined = Int32.Parse(minedAmount.Trim());

                    var minedWasted = match.Groups["miningResidue"].ToString();
                    int wasted = Int32.Parse(minedWasted.Trim());

                    Program.Mining[e.Entry.CharacterName].Add(new MiningLine()
                    {
                        TimeAdded = e.Entry.TimeAdded,
                        MiningItem = match.Groups["miningContext"].ToString(),
                        MiningAmount = mined,
                        WastageAmount = wasted,
                    });
                }

                // is event that roid popped
                match = _MiningRegexRoidExploded.Match(e.Entry.LineContent);
                if (match.Success)
                {

                    foreach (AlertConfig alert in Program.MainConfiguration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
                    {
                        if (!tempTrigger.Enabled)
                            return false;
                        return tempTrigger.AlertType == AlertConfigType.MiningEndedRoidPopped;
                    })))
                    {

                        Log.Information("Mining: Rock expired");
                        var alertText = generateAlertText(e.Entry, null, alert, e.Entry.CharacterName, 0, match.Groups["miningHowItEnded"].ToString());
                        PathInfo path = null;

                        Program.EnqueueNewAlert(
                            new AlertQueueItem()
                            {
                                AlertText = alertText,
                                MenuText = alertText,
                                Entry = e.Entry,
                                Path = path,
                                Context = alertText,
                                SoundFile = alert.SoundFile,
                                FriendlyName = alert.FriendlyName + " - " + e.Entry.CharacterName,
                                isMiningOver = true,
                                ShowAlert = true,
                            }, Program.NewAlerts
                            );
                    }
                }

                // is event cargo full ?
                match = _MiningRegexCargoFull.Match(e.Entry.LineContent);
                if (match.Success)
                {

                    foreach (AlertConfig alert in Program.MainConfiguration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
                    {
                        if (!tempTrigger.Enabled)
                            return false;
                        return tempTrigger.AlertType == AlertConfigType.MiningEndedFull;
                    })))
                    {

                        Log.Information("Mining: cargo full");
                        var alertText = generateAlertText(e.Entry, null, alert, e.Entry.CharacterName, 0, match.Groups["miningDevice"].ToString() + " stopped - Cargo full");
                        PathInfo path = null;

                        Program.EnqueueNewAlert(
                            new AlertQueueItem()
                            {
                                AlertText = alertText,
                                MenuText = e.Entry.CharacterName + alertText,
                                Entry = e.Entry,
                                Path = path,
                                Context = alertText,
                                SoundFile = alert.SoundFile,
                                FriendlyName = alert.FriendlyName + " - " + e.Entry.CharacterName,
                                isMiningOver = true,
                                ShowAlert = true,
                            }, Program.NewAlerts
                            );
                    }
                }

            }
        }

        private void logFile_ProcessDecloak(object sender, ProcessDecloakEventArgs e)
        {
            //Log.Information("COMBAT:" + e.CombatEvent);
            if (e.Entry != null)
            {

                PathInfo path = null;
                var c = Program.CharacterList.Find(o => o.CharacterName.Equals(e.CharacterName));
                string alertText = "";

                foreach (AlertConfig alert in Program.MainConfiguration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
                {
                    if (!tempTrigger.Enabled)
                        return false;
                    return tempTrigger.AlertType == AlertConfigType.Decloak;
                })))
                {

                    Match match = _DecloakRegexMobileObservatory.Match(e.Entry.LineContent);
                    if (match.Success)
                    {
                        Log.Information("DECLOAK: mobile observatory");
                        alertText = generateAlertText(e.Entry, null, alert, "Decloaked pilot", 0, "mobile observatory");
                    }

                    match = _DecloakRegexProximity.Match(e.Entry.LineContent);
                    if (match.Success)
                    {
                        alertText = generateAlertText(e.Entry, null, alert, "Decloaked pilot", 0, match.Groups["decloakProximity"].ToString());
                        Log.Information("DECLOAK: proximity to " + alertText);
                    }

                    Program.EnqueueNewAlert(
                    new AlertQueueItem()
                    {
                        AlertText = alertText,
                        MenuText = c.CharacterName + alertText,
                        Entry = e.Entry,
                        Path = path,
                        Context = alertText,
                        SoundFile = alert.SoundFile,
                        FriendlyName = alert.FriendlyName,
                        isDecloak = true,
                        ShowAlert = true,
                    }, Program.NewAlerts
                    );
                }
            }
        }

        private void RecalculateWindowHeight()
        {
            Height = intelLog.Height + _windowExtraHeight + (Program.MainConfiguration.ShowMinimap ? pMinimapImage.Height : 20);

            lblISK.Top = lblAlertsOutstanding.Top; ;
            lblISK.Left = Width - (lblISK.Width + 15);

            lblMining.Top = lblAlertsOutstanding.Top; ;
            lblMining.Left = Width - (lblISK.Width + lblMining.Width + 20);

            if (Program.MainConfiguration.ShowMinimap)
            {
                //lblISK.Left = pMinimapImage.Left + 10 + pMinimapImage.Width;
                //lblISK.Top = pMinimapImage.Top + pMinimapImage.Height - 1 - lblISK.Height;

                lblEtxraInformation.Top = lblAlertsOutstanding.Top + lblAlertsOutstanding.Height + 5;
                lblEtxraInformation.Height = pMinimapImage.Height - lblISK.Height - 5 - (Program.MainConfiguration.PlanetInfoViewSize * 2);
                lblEtxraInformation.Left = pMinimapImage.Left + pMinimapImage.Width + 2;
            }
            else
            {
                //lblISK.Left = pMinimapImage.Left + 10 + pMinimapImage.Width;
                //lblISK.Top = lblAlertsOutstanding.Top + lblAlertsOutstanding.Height + 5;
                lblSystemName.Top = lblEtxraInformation.Top + lblEtxraInformation.Height - lblSystemName.Height - 5;
            }

            lblStatus.Top = intelLog.Top - lblStatus.Height - 5;
            lblSystemName.Top = lblStatus.Top;
            buttonIntelSize.Top = intelLog.Top + 2;
            buttonIntelSize.Left = intelLog.Width - buttonIntelSize.Width - 2;

            //ActivityLineInc = (Width) / Program.MainConfiguration.HeartBeatIndicatorDivisions;
            //ActivityLineMax = (Width);
        }
        private void buttonIntelSize_Click(object sender, EventArgs e)
        {
            if (intelLog.Height == _intelSizeMin)
            {
                // max it big
                intelLog.Height = _intelSizeMax;
                buttonIntelSize.Text = "▼";
            }
            else
            {
                // make it small
                intelLog.Height = _intelSizeMin;
                buttonIntelSize.Text = "▲";
            }

            RecalculateWindowHeight();

            intelLog.ScrollToCaret();
        }

        private void chartDPS_Click(object sender, EventArgs e)
        {

        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Set the form click-through

                //                cp.ExStyle |= 
                //                    0x80000 /* WS_EX_LAYERED */ | 
                //                    0x20 /* WS_EX_TRANSPARENT */
                //                    ;
                return cp;
            }
        }

        private void intelLog_TextChanged(object sender, EventArgs e)
        {

        }

        private void UpdateEveProcesses(bool refreshProcesses)
        {
            if (refreshProcesses)
            {
                Program.ProcessMonitorHandler.GetUpdatedProcesses();
            }

            string previousToon = CurrentActiveToon;

            string foregroundWindowTitle = null;
            var ptr = Program.ProcessMonitorHandler.GetForegroundWindowHandle();

            var PrevRect = new Rect()
            {
                Top = currentEveWindowRect.Top,
                Bottom = currentEveWindowRect.Bottom,
                Left = currentEveWindowRect.Left,
                Right = currentEveWindowRect.Right,
            };

            var result = Program.ProcessMonitorHandler.GetWindowDimensions(ptr, ref currentEveWindowRect, ref currentEveClientPoint);

            if (ptr != null)
            {
                foregroundWindowTitle = Program.ProcessMonitorHandler.GetTitleOfKnownProcess(ptr);
                if (foregroundWindowTitle != null)
                {
                    //lblCurrentClient.Visible = true;
                    CurrentActiveToon = foregroundWindowTitle.Replace("EVE - ", "");
                    lblCurrentClient.Text = CurrentActiveToon;

                    SetupStatusBar();

                    // sort out DPS Graph
                    if (previousToon == null || previousToon == "" || previousToon != CurrentActiveToon)
                    {
                        if (Program.MainConfiguration.DPSShowWindow && DPSGraphWindow != null)
                        {
                            if (CurrentActiveToon == null || CurrentActiveToon == "")
                            {
                                DPSGraphWindow.Hide();
                            }
                            else
                            {
                                CombatAdded = true;
                                DPSGraphWindow.UpdateDPSGraph();
                                DPSGraphWindow.Show();
                            }
                        }
                        UpdateISK();
                        shouldUpdateMinimap = true;
                        shouldUpdatePlanetInfo = true;
                    }

                    if (DPSGraphWindow != null &&
                        (
                        PrevRect.Top != currentEveWindowRect.Top || PrevRect.Bottom != currentEveWindowRect.Bottom ||
                        PrevRect.Left != currentEveWindowRect.Left || PrevRect.Right != currentEveWindowRect.Right
                        )
                        )
                    {
                        DPSGraphWindow.SetScreenPosition();
                    }
                }
                else
                {
                    if (Program.MainConfiguration.DPSShowWindow && DPSGraphWindow != null)
                    {
                        DPSGraphWindow.Hide();
                    }
                }
            }

            if (foregroundWindowTitle == null)
            {
                //lblCurrentClient.Visible = false;
                CurrentActiveToon = "";
                lblCurrentClient.Text = CurrentActiveToon;
                SetupStatusBar();
                shouldUpdateMinimap = true;
            }

            // just double check thru charachter list active clients to make sure there is still an active client.
            foreach (var c in Program.CharacterList)
            {
                c.IsClientActive = Program.ProcessMonitorHandler.IsCharStillActive(c.CharacterName);
                c.IsCurrentlActiveClient = (c.CharacterName == CurrentActiveToon ? true : false);
            }
        }

        private void ESI_ProcessNewData(object sender, ESIEventArgs e)
        {
            if (e.RequestType == ESIRequestType.System)
            {
                try
                {
                    Invalidate();
                }
                catch (Exception ex)
                {
                }
                shouldUpdatePlanetInfo = true;
            }
        }

        private void ZKill_ProcessNewData(object sender, ZKillEventArgs e)
        {
            string systemName = "---";
            string regionName = "---";

            ConcurrentQueue<AlertQueueItem> ZKillAlerts = new ConcurrentQueue<AlertQueueItem>();

            var victimShipName = Program.Universe.ItemTypes.ContainsKey((int)e.Kill.VictimShipTypeId) ? Program.Universe.ItemTypes[(int)e.Kill.VictimShipTypeId].Name : "Unknown:" + e.Kill.VictimShipTypeId.ToString();
            e.Kill.VictimShipName = victimShipName;

            if (Program.Universe.SolarSystems.ContainsKey((int)e.Kill.SolarSystemId))
            {
                systemName = Program.Universe.SolarSystems[(int)e.Kill.SolarSystemId].Name;
                regionName = Program.Universe.SolarSystems[(int)e.Kill.SolarSystemId].RegionName;
                e.Kill.SolarSystemName = systemName;
                e.Kill.RegionName = regionName;

                Program.ZKillHandler.EnqueueZKillAlert(e.Kill, Program.Universe.SolarSystems[(int)e.Kill.SolarSystemId]);

                //GetSystemInfo((int)e.Kill.SolarSystemId);
            }

            Log.Information($"ZKILL Event handler {e.Kill.ID} {regionName}:{systemName} {victimShipName} {Program.Universe.GetCharacterName((int)e.Kill.VictimCharacterId)}");
            e.Kill.Context = generateZKillAlertText(e.Kill, null, Program.MainConfiguration.ZKillContextText, "");

            // check if zkill system matches any range alerts
            // and that report zkill flag is set
            if (!Program.MainConfiguration.ShowZKillAlerts) return;

            var toSystemId = (int)e.Kill.SolarSystemId;

            int homeSystemId = Program.Universe.HomeSolarSystemID;
            var homeSystemPathInfo = Program.Universe.FindAndCachePath(homeSystemId, toSystemId);
            if (homeSystemPathInfo != null)
            {
                Log.Debug("Jumps from HOME:" + Program.Universe.SolarSystems[homeSystemId].Name + " to " + Program.Universe.SolarSystems[toSystemId].Name + " is " + homeSystemPathInfo.TotalJumps);
            }
            foreach (var c in Program.CharacterList.Where(o => o.LocationId > 0))
            {
                var CurrectLocationPathInfo = Program.Universe.FindAndCachePath(c.LocationId, toSystemId);
                if (CurrectLocationPathInfo != null)
                {
                    Log.Debug("Jumps from " + c.CharacterName + ":" + Program.Universe.SolarSystems[c.LocationId].Name + " to " + Program.Universe.SolarSystems[toSystemId].Name + " is " + CurrectLocationPathInfo.TotalJumps);
                }
            }

            // Range alerts
            foreach (AlertConfig alert in Program.MainConfiguration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
            {
                if (!tempTrigger.Enabled)
                    return false;
                return tempTrigger.AlertType == AlertConfigType.Range;
            })))
            {
                bool sendAlert = false;
                PathInfo path = new PathInfo();
                string alertText = "";

                switch (alert.AlertRangeType)
                {
                    case AlertConfigRangeType.Home:
                        path = homeSystemPathInfo;
                        sendAlert = checkJumpsRange(alert, path);
                        if (sendAlert)
                        {
                            alertText = generateZKillAlertText(e.Kill, path, Program.MainConfiguration.ZKillAlertText, "");
                            Log.Information("ALERT - " + alertText);
                            e.Kill.Alert = true;
                            Program.EnqueueNewAlert(
                                new AlertQueueItem()
                                {
                                    AlertText = alertText,
                                    MenuText = generateZKillAlertText(e.Kill, path, Program.MainConfiguration.ZKillMenuText, ""),
                                    Kill = e.Kill,
                                    Path = path,
                                    Context = homeSystemPathInfo.FromSystemName,
                                    SoundFile = Program.MainConfiguration.ZKillSound,
                                    FriendlyName = "zkill",
                                    ShowAlert = true,
                                }, ZKillAlerts
                                );
                        }
                        break;

                    case AlertConfigRangeType.System:
                        // check against specific system
                        path = Program.Universe.FindAndCachePath(Program.Universe.Names[alert.TriggerText], toSystemId);
                        sendAlert = checkJumpsRange(alert, path);
                        if (sendAlert)
                        {
                            alertText = generateZKillAlertText(e.Kill, path, Program.MainConfiguration.ZKillAlertText, "");
                            Log.Information("ALERT - " + alertText);
                            e.Kill.Alert = true;
                            Program.EnqueueNewAlert(
                                new AlertQueueItem()
                                {
                                    AlertText = alertText,
                                    MenuText = generateZKillAlertText(e.Kill, path, Program.MainConfiguration.ZKillMenuText, ""),
                                    Kill = e.Kill,
                                    Path = path,
                                    Context = path.FromSystemName,
                                    SoundFile = Program.MainConfiguration.ZKillSound,
                                    FriendlyName = "zkill",
                                    ShowAlert = true,
                                }, ZKillAlerts
                                );
                        }
                        break;
                    case AlertConfigRangeType.OneCharacter:
                    case AlertConfigRangeType.OneOnlineCharacter:
                        // check against one specific character
                        foreach (var x in Program.CharacterList.Where(o => o.CharacterName == alert.TriggerText))
                        {
                            if (x.LocationId != 0)
                            {
                                path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);
                                sendAlert = checkJumpsRange(alert, path);
                                if (sendAlert)
                                {
                                    alertText = generateZKillAlertText(e.Kill, path, Program.MainConfiguration.ZKillAlertText, x.CharacterName);
                                    Log.Information("ALERT - " + alertText);
                                    if (
                                        alert.AlertRangeType == AlertConfigRangeType.OneCharacter ||
                                        alert.AlertRangeType == AlertConfigRangeType.OneOnlineCharacter && x.IsClientActive
                                        )
                                    {
                                        e.Kill.Alert = true;
                                        Program.EnqueueNewAlert(
                                        new AlertQueueItem()
                                        {
                                            AlertText = alertText,
                                            MenuText = generateZKillAlertText(e.Kill, path, Program.MainConfiguration.ZKillMenuText, x.CharacterName),
                                            Kill = e.Kill,
                                            Path = path,
                                            Context = x.CharacterName,
                                            SoundFile = Program.MainConfiguration.ZKillSound,
                                            FriendlyName = alert.FriendlyName,
                                            ShowAlert = true,
                                        }, ZKillAlerts
                                        );
                                    }
                                }
                            }
                        }
                        break;
                    case AlertConfigRangeType.AllCharacters:
                    case AlertConfigRangeType.AllOnlineCharacters:
                        // check against all characters
                        foreach (var x in Program.CharacterList)
                        {
                            if (x.LocationId != 0)
                            {
                                path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);
                                sendAlert = checkJumpsRange(alert, path);
                                if (sendAlert)
                                {
                                    alertText = generateZKillAlertText(e.Kill, path, Program.MainConfiguration.ZKillAlertText, x.CharacterName);
                                    Log.Information("ALERT - " + alertText);
                                    if (
                                        alert.AlertRangeType == AlertConfigRangeType.AllCharacters ||
                                        alert.AlertRangeType == AlertConfigRangeType.AllOnlineCharacters && x.IsClientActive
                                        )
                                    {
                                        e.Kill.Alert = true;
                                        Program.EnqueueNewAlert(
                                        new AlertQueueItem()
                                        {
                                            AlertText = alertText,
                                            MenuText = generateZKillAlertText(e.Kill, path, Program.MainConfiguration.ZKillMenuText, x.CharacterName),
                                            Kill = e.Kill,
                                            Path = path,
                                            Context = x.CharacterName,
                                            SoundFile = Program.MainConfiguration.ZKillSound,
                                            FriendlyName = "zkill",
                                            ShowAlert = true,
                                        }, ZKillAlerts
                                        );
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (ZKillAlerts.Count > 0)
                {
                    // Add new alert(s) to NewAlerts
                    while (ZKillAlerts.Count > 0)
                    {
                        AlertQueueItem item = null;
                        ZKillAlerts.TryDequeue(out item);
                        if (item != null)
                        {
                            Program.EnqueueNewAlert(item, Program.NewAlerts);
                        }
                    }
                }
                shouldUpdateMinimap = sendAlert;
            }
        }
        public void SetupStatusBar()
        {
            StringBuilder status = new StringBuilder();

            if (CurrentActiveToon != null && CurrentActiveToon != "")
            {
                var c = Program.CharacterList.Where(o => o.CharacterName == CurrentActiveToon).FirstOrDefault();
                if (c != null)
                {
                    if (c.ClosestThera != null)
                    {
                        status.Append("  Thera <- ");
                        status.Append(c.ClosestTheraPath.ToSystemName);
                        status.Append(" (");
                        status.Append(c.ClosestTheraPath.TotalJumps);
                        status.Append(") - Sig:");
                        status.Append((c.ClosestThera.FromSystemId == c.ClosestTheraPath.ToSystemId) ? c.ClosestThera.FromSignature : c.ClosestThera.ToSignature);
                    }

                    if (c.ClosestTurnur != null)
                    {
                        status.Append("  Turnur <- ");
                        status.Append(c.ClosestTurnurPath.ToSystemName);
                        status.Append(" (");
                        status.Append(c.ClosestTurnurPath.TotalJumps);
                        status.Append(") - Sig:");
                        status.Append((c.ClosestTurnur.FromSystemId == c.ClosestTurnurPath.ToSystemId) ? c.ClosestTurnur.FromSignature : c.ClosestTurnur.ToSignature);
                    }
                }
            }
            else
            {
                status.Append("No active Character");
            }

            lblStatus.Text = status.ToString().Trim();
        }

        private void CheckEveScoutConnection(Character c)
        {

            if (c == null || c.LocationId == 0) return;

            var ClosestThera = EveScoutProcessor.FindClosest(Program.Universe, c.LocationId, "Thera");

            var ClosestTurnur = EveScoutProcessor.FindClosest(Program.Universe, c.LocationId, "Turnur");
            PathInfo closePath;

            if (ClosestThera != null)
            {
                closePath = Program.Universe.FindAndCachePath(c.LocationId, (ClosestThera.FromSystemId == EveScoutManager.SYSTEMID_THERA) ? (int)ClosestThera.ToSystemId : (int)ClosestThera.FromSystemId);
                if (closePath != null)
                {
                    Log.Information($"EveScout: Closest Thera {ClosestThera.ToSystemId}:{ClosestThera.FromSystemId} {closePath.TotalJumps} {closePath.ToSystemName}");
                    c.ClosestThera = ClosestThera;
                    c.ClosestTheraPath = closePath;
                }
                else
                {
                    c.ClosestThera = null;
                    c.ClosestTheraPath = null;
                }
            }
            if (ClosestTurnur != null)
            {
                closePath = Program.Universe.FindAndCachePath(c.LocationId, (ClosestTurnur.FromSystemId == EveScoutManager.SYSTEMID_TURNUR) ? (int)ClosestTurnur.ToSystemId : (int)ClosestTurnur.FromSystemId);
                if (closePath != null)
                {
                    Log.Information($"EveScout: Closest Turnur {ClosestTurnur.ToSystemId}:{ClosestTurnur.FromSystemId} {closePath.TotalJumps} {closePath.ToSystemName}");
                    c.ClosestTurnur = ClosestTurnur;
                    c.ClosestTurnurPath = closePath;
                }
                else
                {
                    c.ClosestTurnur = null;
                    c.ClosestTurnurPath = null;
                }
            }
        }
        private void EveScout_ProcessNewData(object sender, EveScoutEventArgs e)
        {
            foreach (var c in Program.CharacterList.Where(o => o.IsClientActive))
            {
                CheckEveScoutConnection(c);
            }
        }
        private void Dotlan_ProcessNewData(object sender, DotlanEventArgs e)
        {
            Log.Information($"Processing dotlan data for {e.RegionName}");

            // if update is map data
            if (!e.MapContent.IsNullOrEmpty())
            {
                Log.Information($"Processing dotlan map data for {e.RegionName}");
                XmlDocument xmldoc = new XmlDocument
                {
                    XmlResolver = null
                };
                XmlNodeList dotlanXYNodes = null;
                XmlNodeList dotlanIncursionNodes = null;
                xmldoc.LoadXml(e.MapContent);
                dotlanXYNodes = xmldoc.SelectNodes(@"//*[@id='sysuse']/*");
                dotlanIncursionNodes = xmldoc.SelectNodes(@"//*[@id='glow']/*");

                double dotlanX = 0;
                double dotlanY = 0;

                foreach (XmlNode xn in dotlanXYNodes)
                {
                    long systemID = long.Parse(xn.Attributes["id"].Value.Replace("sys", ""));
                    float x = float.Parse(xn.Attributes["x"].Value);
                    float y = float.Parse(xn.Attributes["y"].Value);

                    string link = xn.Attributes["xlink:href"].Value.Replace("#", "");
                    XmlNode dotlanDEFNodes = xmldoc.SelectSingleNode($@"//*[@id='{link}']/*");

                    var s = Program.Universe.SolarSystems[(int)systemID];
                    s.DotlanX = x;
                    s.DotlanY = y;

                    s.IceField = false;
                    XmlNodeList iceFieldNode = xmldoc.SelectNodes($@"//*[@id='{link}']/*/*[@id='{link.Replace("def", "ice")}']");
                    if (iceFieldNode != null && iceFieldNode.Count > 0)
                    {
                        s.IceField = true;
                    }

                    // default to no incursion
                    s.Incursion = false;
                    XmlNode incursionNode = xmldoc.SelectSingleNode($@"//*[@id='glow']/*[@id='{link.Replace("def", "inc")}']");
                    if (incursionNode != null)
                    {
                        s.Incursion = true;
                    }
                }
            }

            // if update is delta data 
            if (!e.DeltaContent.IsNullOrEmpty())
            {
                Log.Information($"Processing dotlan delta data for {e.RegionName}");
                Dictionary<string, string> killdDeltadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.DeltaContent);
                foreach (var kvp in killdDeltadata)
                {
                    int systemId = int.Parse(kvp.Key);
                    int killDelta = int.Parse(kvp.Value);

                    if (Program.Universe.SolarSystems.ContainsKey(systemId))
                    {
                        Program.Universe.SolarSystems[systemId].NPCKillsDeltaLastHour = killDelta;
                    }
                }
            }

            // pull more information from dotlan data
            // save Solarsystem data ?

        }

        private void trayIconMenuShowHistoricAlert(object sender, EventArgs e)
        {
            AlertQueueItem alert = (AlertQueueItem)((MenuItem)sender).Tag;
            //var NewAlertWindow = new QuesadillaAlertPopup(alert, Program.MainConfiguration, Program.LoadImageFromFile(Program.MainConfiguration.AlertPopupBackground), this);
            //NewAlertWindow.Show(this);
            ShowAlert(alert);
        }

        private void SetupLogfileEvents()
        {
            // setup event handlers
            foreach (LogWatcher watcher in Program.logfileWatchers)
            {
                if (watcher._logFileType == LogFileType.Local)
                {
                    watcher.SystemChange += new EventHandler<ProcessSystemChangeEventArgs>(this._localWatcher_SystemChange);
                    watcher.ChatServerChange += new EventHandler<ProcessChatServerChangeEventArgs>(
                        this._localWatcher_ChatServerChange);
                }
                else if (watcher._logFileType == LogFileType.Game)
                {
                    watcher.ProcessCombat += new EventHandler<ProcessCombatEventArgs>(this.logFile_ProcessCombat);
                    watcher.ProcessISK += new EventHandler<ProcessISKEventArgs>(this.logFile_ProcessISK);
                    watcher.ProcessMining += new EventHandler<ProcessMiningEventArgs>(this.logFile_ProcessMining);
                    watcher.ProcessDecloak += new EventHandler<ProcessDecloakEventArgs>(this.logFile_ProcessDecloak);
                }
                else
                {
                    watcher.ProcessNewData += new EventHandler<ProcessNewDataEventArgs>(this.logFile_ProcessNewData);
                }
            }
        }
        private void UpdatePlanetInfo()
        {
            shouldUpdatePlanetInfo = false;

            var c = Program.CharacterList.Where(o => o.CharacterName == CurrentActiveToon).FirstOrDefault();
            if (c != null)
            {
                try
                {
                    if (Program.Universe.SolarSystems[c.LocationId].Planets != null)
                    {
                        var planetInfo = Program.Universe.SolarSystems[c.LocationId].Planets
                            .GroupBy(o => o.PlanetTypeName.Replace("Planet ", ""))
                            .Select(group => new
                            {
                                type = group.Key,
                                Count = group.Count()
                            })
                            .OrderBy(x => x.type);
                        CurrentPlanetInfos.Clear();
                        foreach (var p in planetInfo)
                        {
                            CurrentPlanetInfos.Add(new PlanetInfoDisplay()
                            {
                                PlanetType = p.type,
                                CurrentMouseOver = false,
                                Planets = string.Join(",",
                                    Program.Universe.SolarSystems[c.LocationId].Planets
                                    .Where(o => o.PlanetTypeName == $"Planet {p.type}")
                                    .Select(o => o.PlanetName.Substring(o.PlanetName.LastIndexOf(' ') + 1))
                                    .ToList()),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void UpdateMinimap()
        {
            shouldUpdateMinimap = false;

            if (CurrentActiveToon.IsNullOrEmpty())
            {
                pMinimapImage.Visible = false;
                this.Invalidate();
                return;
            }

            UpdateISK();

            var c = Program.CharacterList.Where(o => o.CharacterName == CurrentActiveToon).FirstOrDefault();

            PathInfo _dummyPath = null;
            if (c != null)
            {
                _dummyPath = new PathInfo()
                {
                    FromSystemName = c.Location,
                    FromSystemId = c.LocationId,
                    ToSystemName = c.Location,
                    ToSystemId = c.LocationId,
                    ToSystemRegionName = "",
                    PathSystems = new List<int> { c.LocationId },
                };
            }

            if (Program.MainConfiguration.ShowMinimap)
            {
                string extraInformation = "";
                Image minimapImage = ImageGenerator.ClipMiniMap(new Bitmap(pMinimapImage.Width, pMinimapImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb), _dummyPath,
                    Program.MainConfiguration.MinimapJumps, mapScale, Program.MainConfiguration.MinimapRawXY, ref extraInformation, ref MinimapSystems,
                    Program.MainConfiguration.MinimapShowSystemText,
                    Program.MainConfiguration.ShowMinimapBorder, Program.MainConfiguration.MinimapBorderColour,
                    Program.MainConfiguration.MinimapLineColourNormal,
                    Program.MainConfiguration.MinimapLineColourRoute,
                    Program.MainConfiguration.MinimapSystemColourNormal,
                    Program.MainConfiguration.MinimapSystemColourRoute,
                    Program.MainConfiguration.MinimapSystemColourFrom,
                    Program.MainConfiguration.MinimapSystemColourTo,
                    Program.MainConfiguration.MinimapSystemColourText,
                    Program.MainConfiguration.MinimapZKillColour,
                    Program.MainConfiguration.MinimapIntelColour,
                    Program.MainConfiguration.MinimapAltPositionColour,
                    Program.MainConfiguration.MinimapIncursionColour,
                    Program.MainConfiguration.MinimapIceColour,
                    AnimationStateIntel.Value(),
                    AnimationStateKills.Value(),
                    AnimationStateAlts.Value(),
                    false
                    );
                Bitmap minimapBitmap = new Bitmap(minimapImage);
                minimapBitmap.MakeTransparent(minimapBitmap.GetPixel(0, 0));
                pMinimapImage.Image = minimapBitmap;
                lblEtxraInformation.Text = extraInformation;
                pMinimapImage.Visible = true;
                this.Invalidate();
            }
            else
            {
                lblEtxraInformation.Text = "";
                pMinimapImage.Visible = false;
                this.Invalidate();
            }
        }
        private void pMinimapImage_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) mapScale += Program.MainConfiguration.MainWindowMouseScrollAmount;
            if (e.Delta < 0) mapScale -= Program.MainConfiguration.MainWindowMouseScrollAmount;
            UpdateMinimap();
        }

        private void Quesadilla_Paint(object sender, PaintEventArgs e)
        {

            ImageGenerator.PaintDrawText(e, lblCurrentClient);
            ImageGenerator.PaintDrawText(e, lblISK);
            ImageGenerator.PaintDrawText(e, lblMining);
            ImageGenerator.PaintDrawText(e, lblAlertsOutstanding);

            if (pMinimapImage.Visible)
            {
                if (Program.MainConfiguration.MainWindowShowExtraInformation) ImageGenerator.PaintDrawText(e, lblEtxraInformation);
                if (mouseOverMinimapSystems.Count() > 0)
                {
                    lblSystemName.Text = "";
                    foreach (var m in mouseOverMinimapSystems)
                    {
                        if (!lblSystemName.Text.IsNullOrEmpty()) lblSystemName.Text += ",";
                        lblSystemName.Text += m.solarSystem.Name + (m.solarSystem.NPCKillsDeltaLastHour == null ? "" : $"  {m.solarSystem.NPCKillsDeltaLastHour}");
                    }
                    ImageGenerator.PaintDrawText(e, lblSystemName);
                }
            }

            // draw red dot
            if (InCombat)
            {
                int combatTop = 0;
                int combatLeft = 0;

                int CIRCLE_RADIUS = 5;

                if (Program.MainConfiguration.ShowMinimap)
                {
                    combatLeft = pMinimapImage.Left + 10;
                    combatTop = pMinimapImage.Top + pMinimapImage.Height - 3 - 10;
                }
                else
                {
                    combatLeft = lblAlertsOutstanding.Left + lblAlertsOutstanding.Width - 10;
                    combatTop = lblAlertsOutstanding.Top + lblAlertsOutstanding.Height + 8;
                }

                Brush systemBrush = new SolidBrush(Color.Red);
                e.Graphics.FillEllipse(systemBrush, combatLeft - CIRCLE_RADIUS, combatTop - CIRCLE_RADIUS, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
            }


            if (Program.MainConfiguration.ShowPlanetInformation && CurrentActiveToon != null && CurrentActiveToon != "")
            {
                var c = Program.CharacterList.Where(o => o.CharacterName == CurrentActiveToon).FirstOrDefault();
                if (c != null)
                {
                    try
                    {
                        if (Program.Universe.SolarSystems[c.LocationId].Planets != null)
                        {
                            if (CurrentPlanetInfos.Count() == 0)
                            {
                                UpdatePlanetInfo();
                            }

                            var planetInfo = Program.Universe.SolarSystems[c.LocationId].Planets
                                .GroupBy(o => o.PlanetTypeName.Replace("Planet ", ""))
                                .Select(group => new
                                {
                                    type = group.Key,
                                    Count = group.Count()
                                })
                                .OrderBy(x => x.type);

                            int currentX = lblEtxraInformation.Left;
                            int currentY = lblStatus.Top - (Program.MainConfiguration.PlanetInfoViewSize);
                            foreach (var p in planetInfo)
                            {
                                Brush systemBrush;
                                Color dotColour = Color.Red;
                                switch (p.type)
                                {
                                    case "(Storm)": dotColour = Program.MainConfiguration.PlanetInfoStormColour; break;
                                    case "(Barren)": dotColour = Program.MainConfiguration.PlanetInfoBarrenColour; break;
                                    case "(Temperate)": dotColour = Program.MainConfiguration.PlanetInfoTemperateColour; break;
                                    case "(Lava)": dotColour = Program.MainConfiguration.PlanetInfoLavaColour; break;
                                    case "(Plasma)": dotColour = Program.MainConfiguration.PlanetInfoPlasmaColour; break;
                                    case "(Gas)": dotColour = Program.MainConfiguration.PlanetInfoGasColour; break;
                                    case "(Ice)": dotColour = Program.MainConfiguration.PlanetInfoIceColour; break;
                                    case "(Oceanic)": dotColour = Program.MainConfiguration.PlanetInfoOceanicColour; break;
                                    default: dotColour = Color.Red; break;
                                }
                                systemBrush = new SolidBrush(dotColour);
                                e.Graphics.FillEllipse(systemBrush, currentX - Program.MainConfiguration.PlanetInfoViewSize, currentY - Program.MainConfiguration.PlanetInfoViewSize, Program.MainConfiguration.PlanetInfoViewSize * 2, Program.MainConfiguration.PlanetInfoViewSize * 2);

                                // set x/y
                                var cpi = CurrentPlanetInfos.Where(o => o.PlanetType == p.type).First();
                                cpi.X = currentX; cpi.Y = currentY;

                                currentX += (Program.MainConfiguration.PlanetInfoViewSize);
                                Font font = new Font("Verdana", Program.MainConfiguration.PlanetInfoFontSize, FontStyle.Regular);
                                Size size = ImageGenerator.PaintDrawText(e, p.Count.ToString(), currentX, currentY - (Program.MainConfiguration.PlanetInfoViewSize), dotColour, font);
                                currentX += size.Width + (Program.MainConfiguration.PlanetInfoViewSize);

                            }

                            foreach (var mouseOverPlanet in CurrentPlanetInfos.Where(o => o.CurrentMouseOver))
                            {
                                Color fontColour = Color.Red;
                                switch (mouseOverPlanet.PlanetType)
                                {
                                    case "(Storm)": fontColour = Program.MainConfiguration.PlanetInfoStormColour; break;
                                    case "(Barren)": fontColour = Program.MainConfiguration.PlanetInfoBarrenColour; break;
                                    case "(Temperate)": fontColour = Program.MainConfiguration.PlanetInfoTemperateColour; break;
                                    case "(Lava)": fontColour = Program.MainConfiguration.PlanetInfoLavaColour; break;
                                    case "(Plasma)": fontColour = Program.MainConfiguration.PlanetInfoPlasmaColour; break;
                                    case "(Gas)": fontColour = Program.MainConfiguration.PlanetInfoGasColour; break;
                                    case "(Ice)": fontColour = Program.MainConfiguration.PlanetInfoIceColour; break;
                                    case "(Oceanic)": fontColour = Program.MainConfiguration.PlanetInfoOceanicColour; break;
                                    default: fontColour = Color.Red; break;
                                }
                                Font font = new Font("Verdana", Program.MainConfiguration.PlanetInfoFontSize, FontStyle.Regular);
                                Size size = ImageGenerator.PaintDrawText(e, mouseOverPlanet.PlanetType + " " + mouseOverPlanet.Planets,
                                    mouseOverPlanet.X - (Program.MainConfiguration.PlanetInfoViewSize),
                                    mouseOverPlanet.Y - ((Program.MainConfiguration.PlanetInfoViewSize * 2) + Program.MainConfiguration.PlanetInfoFontSize),
                                    fontColour, font);
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            /*
            if (Program.MainConfiguration.HeartBeatIndicator)
            {
                Brush lineBrush = new SolidBrush(Program.MainConfiguration.HeartBeatIndicatorColour);
                Pen linePen = new Pen(lineBrush);

                e.Graphics.DrawLine(linePen, new Point(0, 0), new Point(0, activityLine));
            }
            */
        }

        private void UpdateISK()
        {

            if (ISKPerHourTotal == false)
            {
                if (CurrentActiveToon != null && Program.ISK.ContainsKey(CurrentActiveToon))
                {
                    lblISK.Text = Program.GetIskLastHour(CurrentActiveToon);
                    lblISK.ForeColor = Program.MainConfiguration.ISKColour;
                    this.Invalidate();
                    //lblISK.Visible = true;
                }
                else
                {
                    //lblISK.Visible = false;
                    lblISK.Text = "";
                }
            } else
            {
                lblISK.Text = Program.GetIskLastHour("");
                lblISK.ForeColor = Program.MainConfiguration.ISKTotalColour;
            }
        }
        private void UpdateMining()
        {

            if (MiningPerHourTotal == false)
            {
                if (CurrentActiveToon != null && Program.Mining.ContainsKey(CurrentActiveToon))
                {
                    lblMining.Text = Program.GetMiningLastHour(CurrentActiveToon);
                    lblMining.ForeColor = Program.MainConfiguration.MiningColour;
                    this.Invalidate();
                    //lblISK.Visible = true;
                }
                else
                {
                    //lblISK.Visible = false;
                    lblMining.Text = "";
                }
            }
            else
            {
                lblMining.Text = Program.GetMiningLastHour("");
                lblMining.ForeColor = Program.MainConfiguration.MiningTotalColour;
            }
        }

        private void UpdateMiningCSV()
        {
            var miningDetail = Program.GetMiningLastHourDetail("");
            try
            {
                using (var writer = new StreamWriter("miningdata.csv"))
                {
                    writer.WriteLine("Character,MinedItem,Amount,Wastage");
                    foreach (var m in miningDetail)
                    {
                        foreach (var md in m.Value)
                        {
                            writer.WriteLine($"{m.Key},{md.MiningItem},{md.MiningAmount},{md.WastageAmount}");
                        }
                    }
                }
            } 
            catch (Exception ex)
            {

            }

        }

        private void lblEtxraInformation_Click(object sender, EventArgs e)
        {

        }

        private void pMinimapImage_MouseMove(object sender, MouseEventArgs e)
        {
            mouseOverMinimapSystems = Program.Universe.MouseOverSystem(e.X, e.Y, MinimapSystems);
            if (mouseOverMinimapSystems.Count() > 0)
            {
                foreach (var s in mouseOverMinimapSystems)
                {
                    GetSystemInfo((int)s.solarSystem.Id);
                }
                this.Invalidate();
            }
        }

        private void pMinimapImage_Click(object sender, EventArgs e)
        {

        }
        private void ShowAlert(AlertQueueItem alert)
        {
            if (Program.MainConfiguration.ShowAlertFancyNotification) ShowAlertWindow(alert);
            if (Program.MainConfiguration.ShowAlertWindowsNotification) ShowAlertToast(alert);
        }
        private void ShowAlertWindow(AlertQueueItem alert)
        {
            // try to prevent new alert stealing focus
            var ptr = Program.ProcessMonitorHandler.GetForegroundWindowHandle();
            var NewAlertWindow = new QuesadillaAlertPopup(alert, Program.MainConfiguration, Program.LoadImageFromFile(Program.MainConfiguration.AlertPopupBackground), this);
            NewAlertWindow.Show(this);
            var ptr2 = Program.ProcessMonitorHandler.SetForegroundWindowHandle(ptr);
        }

        private void ShowAlertToast(AlertQueueItem alert)
        {

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                // https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
                System.Diagnostics.Process.Start(args["url"]);
            };

            var t = new ToastContentBuilder();

            if (alert.Path != null)
            {
                t.AddButton(new ToastButton()
                    .SetContent($"View {alert.Path.ToSystemName}")
                    .AddArgument("action", "viewURL")
                    .AddArgument("url", "https://evemaps.dotlan.net/map/" + alert.Path.ToSystemRegionName.Replace(" ", "_") + "/" + alert.Path.ToSystemName));

                t.AddText($"System: {alert.Path.ToSystemName} ({alert.Path.ToSystemRegionName})");
            }

            if (alert.Kill != null)
            {
                t.AddButton(new ToastButton()
                    .SetContent($"View Zkill")
                    .AddArgument("action", "viewURL")
                    .AddArgument("url", "https://zkillboard.com/kill/" + alert.Kill.ID));

                var shipUrl = $"https://images.evetech.net/types/{alert.Kill.VictimShipTypeId}/render?size=128";
                shipUrl = "https://quesadilla.prioryofthelemon.co.uk/wp/wp-content/uploads/2024/07/Quesadilla-1.png";

                Uri img = new Uri(shipUrl);
                t.AddAppLogoOverride(new Uri(shipUrl), ToastGenericAppLogoCrop.Circle);
                t.AddInlineImage(img, alert.Kill.VictimShipName);
            }

            string contextInfo = "";
            if (alert.Path != null && !(alert.isFaction || alert.isDecloak))
            {
                contextInfo = alert.Path.TotalJumps.ToString() + " jump" + (alert.Path.TotalJumps == 1 ? "" : "s") + "-> " + alert.Context;
            }
            else
            {
                if (alert.isFaction)
                {
                    contextInfo = alert.Context;
                }
                else if (alert.isDecloak)
                {
                    contextInfo = alert.Context;
                }
                else

                {
                    contextInfo = alert.FriendlyName;
                }
            }

            t.AddText(contextInfo);

            string intelLine = "";
            if (alert.isFaction)
            {
                intelLine = "Faction Spawn";
                t.AddHeader("faction", "Faction spawn", "");
            }
            else if (alert.Kill != null)
            {
                t.AddHeader("zkill", "Zkill alert", "");
                intelLine = alert.Kill.Context.Replace("\n", " - ");
            }
            else
            {
                t.AddHeader("intel", "Intel alert", "");
                intelLine = alert.Entry.LineContent;
            }
            t.AddText(intelLine);

            t.Show(toast =>
            {
                toast.ExpirationTime = DateTime.Now.AddSeconds(Program.MainConfiguration.AlertPopupSeconds);
            });

        }

        private void Quesadilla_MouseMove(object sender, MouseEventArgs e)
        {
            var WasMouseOver = CurrentPlanetInfos.Where(o => o.CurrentMouseOver == true).Count() > 0 ? true : false;
            foreach (var p in CurrentPlanetInfos)
            {
                if (
                    (p.X >= e.X - Program.MainConfiguration.PlanetInfoViewSize && p.X <= e.X + Program.MainConfiguration.PlanetInfoViewSize) &&
                    (p.Y >= e.Y - Program.MainConfiguration.PlanetInfoViewSize && p.Y <= e.Y + Program.MainConfiguration.PlanetInfoViewSize))
                {
                    p.CurrentMouseOver = true;
                }
                else
                {
                    p.CurrentMouseOver = false;
                }
            }
            var IsMouseOver = CurrentPlanetInfos.Where(o => o.CurrentMouseOver == true).Count() > 0 ? true : false;
            if (WasMouseOver || IsMouseOver)
            {
                this.Invalidate();
            }
        }

        private void pMinimapImage_Paint(object sender, PaintEventArgs e)
        {
            if (pMinimapImage.Visible)
            {
                if (Program.MainConfiguration.MainWindowShowExtraInformation) ImageGenerator.PaintDrawText(e, lblEtxraInformation);
                if (mouseOverMinimapSystems.Count() > 0)
                {
                    foreach (var m in mouseOverMinimapSystems)
                    {
                        ImageGenerator.DrawSystemName(e.Graphics, m.X + pMinimapImage.Left, m.Y + pMinimapImage.Top, m.solarSystem.Name, Program.MainConfiguration.MinimapMouseoverSystemColour, 10);
                    }
                }
            }
        }
    }
}
