using EVEStandard.Models;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static Quesadilla.Classes.Processes.ProcessMonitor;
using static Quesadilla.Extensions.StringExtensions;
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

        private readonly Regex _DecloakRegexMobileObservatory = new Regex("cloak deactivates due to a pulse from a Mobile Observatory", RegexOptions.Compiled);
        private readonly Regex _DecloakRegexProximity = new Regex("Your cloak deactivates due to proximity to (?<decloakProximity>.*)", RegexOptions.Compiled);

        private readonly Regex _ISKRegexISKData = new Regex("^<font size=12><b><color=0x........>\\s*(?<iskAmount>.*) ISK\\s*</b><color=0x........> added to next bounty payout", RegexOptions.Compiled);

        private Queue<LogEntry> _newIntelQueue = new Queue<LogEntry>();

        public UniverseManager _universe = new UniverseManager();
        public Config Configuration = new Config();
        private List<LogWatcher> watchers = new List<LogWatcher>();

        private Queue<AlertQueueItem> NewAlerts = new Queue<AlertQueueItem>();
        private Queue<AlertQueueItem> ActiveAlerts = new Queue<AlertQueueItem>();

        private List<QuesadillaSound> Sounds = new List<QuesadillaSound>();
        public List<QuesadillaBackground> Backgrounds = new List<QuesadillaBackground>();

        public string configuration_filename = "Quesadilla.config.json";
        private int _intelSizeMax = 200;
        private int _intelSizeMin = 48;
        private int _windowExtraHeight = 80;

        private const string CHARACTER_CACHE_FILE = "Quesadilla.CharacterCache.Json";

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

        private ZKillRedisQ ZKillFeed;
        private List<ZKillKill> ZKillKills = new List<ZKillKill>();

        private EveScoutManager EveScoutProcessor;

        public bool AlertShowing = false;

        private bool ChatServerStatus = true;
        private bool InCombat = false;
        private bool CombatAdded = false;

        private string combatNeutOut = "ff7fffff";

        private bool ConfigurationWindowOpen = false;

        public Dictionary<string, int> TickCounts = new Dictionary<string, int>();

        private Thread splashScreenThread;
        private QuesadillaSplashScreen splashScreen;
        public QuesadillaDPSGraph DPSGraphWindow = null;

        private Color OriginalBackgroundColour;

        public Dictionary<string, List<CombatLine>> _combat = new Dictionary<string, List<CombatLine>>();
        public Dictionary<string, List<IskLine>> _ISK = new Dictionary<string, List<IskLine>>();

        private List<ProcessInfo> knowProcesses = new List<ProcessInfo>();
        public string CurrentActiveToon;

        // TODO - config things - really need to go into config structure later
        public List<Character> CharacterList { get; set; }

        private NotifyIcon trayIcon;

        private ProcessMonitor _processMonitor = new ProcessMonitor();

        private DateTime LastFactionAlert = DateTime.Now;

        public Rect currentEveWindowRect = new Rect();
        public Point currentEveClientPoint = new Point(0, 0);

        private string ZKillRedisID = "Quesadilla_" + Guid.NewGuid().ToString();

        public Quesadilla()
        {
            splashScreenThread = new Thread(new ThreadStart(ShowSplashScreen));
            splashScreenThread.Start();
            Thread.Sleep(2000);

            OriginalBackgroundColour = BackColor;

            InitializeComponent();
            InitializeResources();

            Configuration = Config.Load(configuration_filename);

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

            menuConfiguration.MenuItems.Add(menuConfigurationWindow = new MenuItem("Edit Configuration", trayIconMenuConfigurationClick));
            menuConfiguration.MenuItems.Add(new MenuItem("-"));

            menuConfiguration.MenuItems.Add(menuConfigurationAlwaysOnTop = new MenuItem("Always on top", trayIconMenuAlwaysOnTop) { Checked = Configuration.StayOnTop, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowDPS = new MenuItem("Show DPS", trayIconMenuDPSShowWindow) { Checked = Configuration.DPSShowWindow, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowZkill = new MenuItem("Show ZKill Alerts", trayIconMenuShowZKillAlerts) { Checked = Configuration.ShowZKillAlerts, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSStayOnTop = new MenuItem("DPS Always on top", trayIconMenuDPSAlwaysOnTop) { Checked = Configuration.DPSGraphStayOnTop, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSTransparent = new MenuItem("DPS Transparent", trayIconMenuDPSTransparent) { Checked = Configuration.DPSGraphTransparent, });
            menuConfiguration.MenuItems.Add(menuConfigurationAlertTransparent = new MenuItem("Alert Transparent", trayIconMenuAlertTransparent) { Checked = Configuration.AlertTransparent, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSSaveCSV = new MenuItem("DPS Save CSV", trayIconMenuDPSSaveCSV) { Checked = Configuration.DPSSaveCSV, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowAll = new MenuItem("DPS Combine", trayIconMenuDPSAllToons) { Checked = Configuration.DPSAllToons, });

            menuConfiguration.MenuItems.Add(menuBackgrounds);
            menuConfiguration.MenuItems.Add(menuConfiguredAlerts);
            menuConfiguration.MenuItems.Add(menuLogWatchers);

            // stop main tick timer!
            mainTimer.Enabled = false;

            CharacterList = new List<Character>();
            _universe.ReadSolarSystems("Quesadilla.Systems.Json");
            _universe.ReadItemTypes("Quesadilla.ItemTypes.Json");
            _universe.ReadESICharacters(CHARACTER_CACHE_FILE);
            _universe.SetupESI();

            ReloadConfiguration();

            EveScoutProcessor = new EveScoutManager();
            EveScoutProcessor.EveScoutEventHandler += new EventHandler<EveScoutEventArgs>(this.EveScout_ProcessNewData);

            watchers.Add(new LogWatcher("catch", Classes.Logfile.LogFileType.ChatIntel, "Catch Intel"));
            watchers.Add(new LogWatcher("ftn", Classes.Logfile.LogFileType.ChatIntel, "Fountain Intel"));
            watchers.Add(new LogWatcher("period", Classes.Logfile.LogFileType.ChatIntel, "Period Basis Intel"));

            watchers.Add(new LogWatcher("querious", Classes.Logfile.LogFileType.ChatIntel, "Querious Intel"));
            watchers.Add(new LogWatcher("delve", Classes.Logfile.LogFileType.ChatIntel, "Delve Intel"));

            //            watchers.Add(new LogWatcher("cr", Classes.Logfile.LogFileType.ChatIntel, "Cloud Ring Intel"));
            watchers.Add(new LogWatcher("paragon", Classes.Logfile.LogFileType.ChatIntel, "Paragon Soul Intel"));

            watchers.Add(new LogWatcher("Quer_Catch", Classes.Logfile.LogFileType.ChatIntel, "Querious/Catch Intel"));

watchers.Add(new LogWatcher("izakbar", Classes.Logfile.LogFileType.ChatIntel, "Izakbar debug"));

            watchers.Add(new LogWatcher(null, Classes.Logfile.LogFileType.Local, "Local chat"));
            watchers.Add(new LogWatcher(null, Classes.Logfile.LogFileType.Game, "Gamelogs"));

            RebuildLogfilesMenu();

            // setup event handlers
            foreach (LogWatcher watcher in watchers)
            {
                if (watcher._logFileType == LogFileType.Local)
                {
                    watcher.SystemChange += new EventHandler<ProcessSystemChangeEventArgs>(this._localWatcher_SystemChange);
                    watcher.ChatServerChange += new EventHandler<ProcessChatServerChangeEventArgs>(this._localWatcher_ChatServerChange);
                }
                else if (watcher._logFileType == LogFileType.Game)
                {
                    watcher.ProcessCombat += new EventHandler<ProcessCombatEventArgs>(this.logFile_ProcessCombat);
                    watcher.ProcessISK += new EventHandler<ProcessISKEventArgs>(this.logFile_ProcessISK);
                    watcher.ProcessDecloak += new EventHandler<ProcessDecloakEventArgs>(this.logFile_ProcessDecloak);
                }
                else
                {
                    watcher.ProcessNewData += new EventHandler<ProcessNewDataEventArgs>(this.logFile_ProcessNewData);
                }
            }

            // start watchers
            foreach (LogWatcher watcher in watchers)
            {
                watcher.StartWatch();
            }

            // start main tick timer
            mainTimer.Enabled = true;

            ZKillFeed = new ZKillRedisQ();
            ZKillFeed.ZKillEventHandler += new EventHandler<ZKillEventArgs>(this.ZKill_ProcessNewData);

        }

        private void EveScoutProcessor_EveScoutEventHandler(object sender, EveScoutEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ShowSplashScreen()
        {
            Application.Run(splashScreen = new QuesadillaSplashScreen());
        }

        private void InitializeResources()
        {
            // find all resources - put sounds into sound array, backgrounds in background
            ResourceManager rm = Properties.Resources.ResourceManager;
            ResourceSet rs = rm.GetResourceSet(new CultureInfo("en-US"), true, true);
            if (rs != null)
            {
                foreach (var r in rs.Cast<DictionaryEntry>())
                {
                    string name = r.Key.ToString();
                    if (r.Value is Image)
                    {
                        // if background - store in backgrounds
                        if (name.StartsWith("background") || name.StartsWith("bg_"))
                        {
                            Backgrounds.Add(new QuesadillaBackground()
                            {
                                BackgroundName = name,
                                background = (Bitmap)r.Value,
                            });
                        }
                    }
                    if (r.Value is UnmanagedMemoryStream)
                    {
                        // sound file
                        Sounds.Add(new QuesadillaSound()
                        {
                            SoundName = r.Key.ToString(),
                            stream = (UnmanagedMemoryStream)r.Value,
                        });
                    }
                }
            }
        }
        private void RebuildLogfilesMenu()
        {
            menuLogWatchers.MenuItems.Clear();
            foreach (var watcher in watchers)
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

            foreach (var alert in ActiveAlerts.Reverse().Take(Configuration.ShowLastUnexpiredAlerts).ToList())
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
                MenuItem menuAlert = new MenuItem(menuText, trayIconMenuShowHistoricAlert){ Tag = alert};
                menuLatestAlerts.MenuItems.Add(menuAlert);
            }
        }
        private void RebuildAlertsMenu()
        {
            menuConfiguredAlerts.MenuItems.Clear();
            foreach (var alertConfigItem in Configuration.AlertConfiguration)
            {
                MenuItem menuAlert = new MenuItem(alertConfigItem.FriendlyName, trayIconMenuAlertClick);
                menuAlert.Checked = alertConfigItem.Enabled;
                menuConfiguredAlerts.MenuItems.Add(menuAlert);
            }
        }
        private void RebuildBackgroundsMenu()
        {
            menuBackgrounds.MenuItems.Clear();
            foreach (var background in Backgrounds.OrderBy(o => o.BackgroundName).ToList())
            {
                MenuItem menuBackground = new MenuItem(background.BackgroundName, trayIconMenuBackgroundClick);
                if (Configuration.AlertPopupBackground == background.BackgroundName) menuBackground.Checked = true;
                menuBackgrounds.MenuItems.Add(menuBackground);
            }
        }
        public void ReloadConfiguration()
        {
            RebuildAlertsMenu();
            RebuildBackgroundsMenu();
            RebuildLatestAlertsMenu();

            if (Configuration.InitialPosition != null)
            {
                this.Location = Configuration.InitialPosition;
                if (this.Location.X > Screen.FromControl(this).Bounds.Width || this.Location.X < Screen.FromControl(this).Bounds.X ||
                    this.Location.Y > Screen.FromControl(this).Bounds.Height || this.Location.Y < Screen.FromControl(this).Bounds.Y
                    )
                {
                    this.Location = new System.Drawing.Point(0, 0);
                }

                //Width = Configuration.WindowSize.X >= 100 ? Configuration.WindowSize.X : 100;
                //Height = Configuration.WindowSize.Y >= 100 ? Configuration.WindowSize.Y : 100;
            }

            TopMost = Configuration.StayOnTop;
            menuConfigurationAlwaysOnTop.Checked = Configuration.StayOnTop;
            menuConfigurationDPSShowDPS.Checked = Configuration.DPSShowWindow;
            menuConfigurationDPSStayOnTop.Checked = Configuration.DPSGraphStayOnTop;
            menuConfigurationDPSTransparent.Checked = Configuration.DPSGraphTransparent;
            menuConfigurationDPSSaveCSV.Checked = Configuration.DPSSaveCSV;
            menuConfigurationDPSShowAll.Checked = Configuration.DPSAllToons;

            _universe.HomeSolarSystemID = _universe.Names[Configuration.HomeSystem];

            mainTimer.Interval = Configuration.TickMiliseconds;

        }
        private void Quesadilla_Load(object sender, EventArgs e)
        {
            ReloadConfiguration();
            buttonIntelSize_Click(null, null);

            splashScreenThread.Interrupt();
            Thread.Sleep(100);
            splashScreenThread.Abort();
            Thread.Sleep(500);
            splashScreen = null;

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            this.Text = "Quesadilla v" + version.ToString();

        }
        private SoundPlayer LoadSoundFromFile(string fileName)
        {

            Stream soundStream = null;

            if (Sounds.Find(o => o.SoundName == fileName) != null)
            {
                soundStream = (Stream)Sounds.Find(o => o.SoundName == fileName).stream;
                soundStream.Seek(0, SeekOrigin.Begin);
                SoundPlayer soundPlayer = new SoundPlayer(soundStream);
                soundPlayer.Load();
                return soundPlayer;
            }

            if (File.Exists(fileName))
            {
                using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    try
                    {
                        SoundPlayer soundPlayer = new SoundPlayer((Stream)fileStream);
                        soundPlayer.Load();
                        return soundPlayer;
                    }
                    catch (Exception ex)
                    {
                        return (SoundPlayer)null;
                    }
                }
            }
            return (SoundPlayer)null;
        }
        public Image LoadImageFromFile(string fileName)
        {
            Bitmap imageStream = null;

            if (Backgrounds.Find(o => o.BackgroundName == fileName) != null)
            {
                imageStream = (Bitmap)Backgrounds.Find(o => o.BackgroundName == fileName).background;
                return imageStream;
            }

            if (File.Exists(fileName))
            {
                Image imageFile = Image.FromFile(fileName);
                return imageFile;
            }
            return (Image)null;
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {


            // progress logfiles
            foreach (LogWatcher watcher in watchers)
            {
                watcher.Tick();
            }

            // dequeue Alerts
            TickCounts["SinceLastAlert"]++;
            if (NewAlerts.Count > 0 && TickCounts["SinceLastAlert"] > Configuration.HowManyTicksBeforeAlert)
            {
                var NewAlert = NewAlerts.Dequeue();
                Log.Error(NewAlert.AlertText);

                ActiveAlerts.Enqueue(NewAlert);
                // do any extra processing 


                // try to prevent new alert stealing focus
                var NewAlertWindow = new QuesadillaAlertPopup(NewAlert, Configuration, LoadImageFromFile(Configuration.AlertPopupBackground), this);
                var ptr = this._processMonitor.GetForegroundWindowHandle();
                NewAlertWindow.Show(this);
                var ptr2 = this._processMonitor.SetForegroundWindowHandle(ptr);
                SoundPlayer sound = LoadSoundFromFile(NewAlert.SoundFile);
                if (sound != null) sound.Play();
                RebuildLatestAlertsMenu();

                TickCounts["SinceLastAlert"] = 0;
            }


            // do cleardown/rebuild of alerts menu - if number of ticks passed
            if ( ++TickCounts["SinceLastCleardown"] > Configuration.HowManyTicksBeforeCleardown)
            {
                // Show number of recent alerts
                if (ActiveAlerts.Count > 0)
                {
                    // check if we should expire first entry
                    var a = ActiveAlerts.First();
                    DateTime timeAdded = a.Entry != null ? a.Entry.TimeAdded : a.Kill.TimeAdded;
                    var diffInSeconds = (DateTime.Now - timeAdded).TotalSeconds;
                    if (diffInSeconds > Configuration.ExpireAlertSeconds)
                    {
                        // expired so dequeue
                        a = ActiveAlerts.Dequeue();
                        RebuildLatestAlertsMenu();
                    }
                    alertsOutstanding.Text = ActiveAlerts.Count.ToString();
                }
                else
                {
                    alertsOutstanding.Text = "";
                }

                // expire combats ?
                foreach (var k in _combat.Keys)
                {
                    if (_combat[k].Count > 0)
                    {
                        TimeSpan ts = DateTime.Now - _combat[k].First().TimeAdded;
                        if (ts.TotalSeconds > Configuration.ExpireCombatSeconds)
                        {
                            _combat[k].Remove(_combat[k].First());
                        }
                    }
                }

                // expire ISK ?
                foreach (var k in _ISK.Keys)
                {
                    if (_ISK[k].Count > 0)
                    {
                        TimeSpan ts = DateTime.Now - _ISK[k].First().TimeAdded;
                        if (ts.TotalSeconds > Configuration.ExpireISKSeconds)
                        {
                            _ISK[k].Remove(_ISK[k].First());
                        }
                    }
                }

                // expire zkills ?
                lock (ZKillKills)
                {
                    foreach (var k in ZKillKills)
                    {
                        TimeSpan ts = DateTime.Now - k.TimeAdded;
                        if (ts.TotalSeconds > Configuration.ExpireZKillSeconds)
                        {
                            _universe.SolarSystems[(int)k.SolarSystemId].Kills.Remove(k);
                            k.Expired = true;
                        }
                    }
                    ZKillKills.RemoveAll(o => o.Expired);
                }

                TickCounts["SinceLastCleardown"] = 0;
            }

            UpdateEveProcesses();

            CombatFlag.Visible = InCombat;
            TickCounts["SinceLastDPSGraphUpdate"]++;
            if (CombatAdded && TickCounts["SinceLastDPSGraphUpdate"] > Configuration.HowManyTicksBeforeDPSGraphUpdate )
            {
                if (Configuration.DPSShowWindow && DPSGraphWindow == null)
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

            if (_ISK.ContainsKey(CurrentActiveToon))
            {
                lblISK.Text = GetIskLastHour(CurrentActiveToon);
                lblISK.Visible = true;
            }
            else
            {
                lblISK.Visible = false;
            }
        }

        private void AddNewCharacter(string characterName)
        {
            if (characterName.Trim().Length == 0)
                return;

            this.CharacterList.Add(new Character()
            {
                CharacterName = characterName.Trim(),
                IsClientActive = false,
            });
            Log.Information("New character found: " + characterName);
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
                path.TotalJumps < Configuration.IgnoreAsTooManyJumps;

            return shouldAlert;
        }
        private string PathToString(PathInfo path)
        {
            string route = "";
            if (path != null)
            {
                foreach (int systemId in path.PathSystems)
                {
                    if (route.Length > 0) route += " -> ";
                    route += systemId == path.FromSystemId ? "" : _universe.SolarSystems[systemId].Name;
                }
            }
            return route;
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
                returnString = returnString.Replace("{route}", PathToString(path));
                returnString = returnString.Replace("{tosystem}", _universe.SolarSystems[path.ToSystemId].Name);
                returnString = returnString.Replace("{region}", _universe.SolarSystems[path.ToSystemId].RegionName);
                returnString = returnString.Replace("{fromsystem}", _universe.SolarSystems[path.FromSystemId].Name);
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
                returnString = returnString.Replace("{route}", PathToString(path));
                returnString = returnString.Replace("{tosystem}", _universe.SolarSystems[path.ToSystemId].Name);
                returnString = returnString.Replace("{region}", _universe.SolarSystems[path.ToSystemId].RegionName);
                returnString = returnString.Replace("{fromsystem}", _universe.SolarSystems[path.FromSystemId].Name);
            }
            returnString = returnString.Replace("{character}", characterName);
            returnString = returnString.Replace("{text}", kill.href);
            returnString = returnString.Replace("{value}", kill.TotalValue);
            returnString = returnString.Replace("{victim}", _universe.GetCharacterName((int)kill.VictimCharacterId));
            returnString = returnString.Replace("{system}", _universe.SolarSystems.ContainsKey((int)kill.SolarSystemId) ? _universe.SolarSystems[(int)kill.SolarSystemId].Name : kill.SolarSystemId.ToString());
            returnString = returnString.Replace("{killer}", kill.KillerCharacterId == 0 ? "NPC" : _universe.GetCharacterName((int)kill.KillerCharacterId));
            returnString = returnString.Replace("{victimship}", _universe.ItemTypes.ContainsKey((int)kill.VictimShipTypeId) ? _universe.ItemTypes[(int)kill.VictimShipTypeId].Name : "Unknown:" + kill.VictimShipTypeId.ToString());

            returnString = returnString.Replace("{killership}", _universe.ItemTypes.ContainsKey((int)kill.KillerShipTypeId) ? _universe.ItemTypes[(int)kill.KillerShipTypeId].Name : kill.KillerShipTypeId.ToString());
            returnString = returnString.Replace("{killerweapon}", kill.KillerWeaponTypeId.ToString());
            return returnString;
        }
        private void EnqueueAlert(AlertQueueItem alert)
        {
            // only add to new alerts if not already in new alerts. This should prevent an alert for each character on same logfile entry
            var f = NewAlerts.Where(o =>
            (o.Path != null && alert.Path != null && o.Path.ToSystemId == alert.Path.ToSystemId)
            || (o.AlertText == alert.AlertText)
            ).ToList();
            if (f.Count == 0 || NewAlerts.Count == 0)
            {
                alert.RouteText = PathToString(alert.Path);
                NewAlerts.Enqueue(alert);
            }
            if (f.Count != 0)
            {
                if (f.First().Path != null && alert.Path != null && alert.Path.TotalJumps < f.First().Path.TotalJumps)
                {
                    f.First().Path = alert.Path;
                    f.First().AlertText = alert.AlertText;
                    f.First().Entry = alert.Entry;
                    f.First().Context = alert.Context;
                    f.First().RouteText = alert.RouteText;
                    f.First().SoundFile = alert.SoundFile;
                }
            }
        }
        private void logFile_ProcessNewData(object sender, ProcessNewDataEventArgs e)
        {
            LogEntry entry = e.LogEntry;
            int alertCount = 0;

            if (entry == null || entry.PlayerName == "EVE System")
                return;
            if (entry.EntryType != LogEntryType.ChatEvent && entry.EntryType != LogEntryType.UnknownChatLogEvent && entry.EntryType != LogEntryType.UnknownGameLogEvent)
            {
                if (this.CharacterList.Where(o => o.CharacterName == entry.CharacterName).ToList().Count == 0)
                    this.AddNewCharacter(entry.CharacterName);
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
                if (!Configuration.ignoreStrings.Any<Regex>((Func<Regex, bool>)(ignoreString => ignoreString.IsMatch(entry.LineContent))) && entry.LogType == LogFileType.ChatIntel)
                {
                    // try and find systems mentioned in intel
                    foreach (SolarSystem solarSystem in
                        _universe.SolarSystems.Values.Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => tempSystem.MatchNameRegex(entry.LineContent)))
                        .Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => !Configuration.ignoreSystems.Contains(this._universe.Names[tempSystem.Name])))
                        )
                        intSet.Add(_universe.Names[solarSystem.Name]);
                }

                if (intSet.Count > 0)
                {
                    Log.Information(entry.LineContent);
                    entry.MatchedIds = intSet;
                    foreach (int toSystemId in intSet)
                    {
                        int homeSystemId = _universe.HomeSolarSystemID;

                        // Cache paths - if not already stored
                        var homeSystemPathInfo = _universe.FindAndCachePath(homeSystemId, toSystemId);
                        if (homeSystemPathInfo != null)
                        {
                            Log.Debug("Jumps from HOME:" + _universe.SolarSystems[homeSystemId].Name + " to " + _universe.SolarSystems[toSystemId].Name + " is " + homeSystemPathInfo.TotalJumps);
                        }

                        foreach (var c in CharacterList.Where(o => o.LocationId > 0))
                        {
                            var CurrectLocationPathInfo = _universe.FindAndCachePath(c.LocationId, toSystemId);
                            if (CurrectLocationPathInfo != null)
                            {
                                Log.Debug("Jumps from " + c.CharacterName + ":" + _universe.SolarSystems[c.LocationId].Name + " to " + _universe.SolarSystems[toSystemId].Name + " is " + CurrectLocationPathInfo.TotalJumps);
                            }
                        }

                        // Range alerts
                        foreach (AlertConfig alert in Configuration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
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
                                        EnqueueAlert(
                                            new AlertQueueItem()
                                            {
                                                AlertText = alertText,
                                                MenuText = generateAlertText(entry, path, alert, "", 1, ""),
                                                Entry = entry,
                                                Path = path,
                                                Context = homeSystemPathInfo.FromSystemName,
                                                SoundFile = alert.SoundFile,
                                                FriendlyName = alert.FriendlyName,
                                            }
                                            );
                                        alertCount++;
                                    }
                                    break;

                                case AlertConfigRangeType.System:
                                    // check against specific system
                                    path = _universe.FindAndCachePath(_universe.Names[alert.TriggerText], toSystemId);
                                    sendAlert = checkJumpsRange(alert, path);
                                    if (sendAlert)
                                    {
                                        alertText = generateAlertText(entry, path, alert, "", 0, "");
                                        Log.Debug("ALERT - " + alertText);
                                        EnqueueAlert(
                                            new AlertQueueItem()
                                            {
                                                AlertText = alertText,
                                                MenuText = generateAlertText(entry, path, alert, "", 1, ""),
                                                Entry = entry,
                                                Path = path,
                                                Context = path.FromSystemName,
                                                SoundFile = alert.SoundFile,
                                                FriendlyName = alert.FriendlyName,
                                            }
                                            );
                                        alertCount++;
                                    }
                                    break;
                                case AlertConfigRangeType.OneCharacter:
                                case AlertConfigRangeType.OneOnlineCharacter:
                                    // check against one specific character
                                    foreach (var x in CharacterList.Where(o => o.CharacterName == alert.TriggerText))
                                    {
                                        if (x.LocationId != 0)
                                        {
                                            path = _universe.FindAndCachePath(x.LocationId, toSystemId);
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
                                                    EnqueueAlert(
                                                    new AlertQueueItem()
                                                    {
                                                        AlertText = alertText,
                                                        MenuText = generateAlertText(entry, path, alert, x.CharacterName, 1, ""),
                                                        Entry = entry,
                                                        Path = path,
                                                        Context = x.CharacterName,
                                                        SoundFile = alert.SoundFile,
                                                        FriendlyName = alert.FriendlyName,
                                                    }
                                                    );
                                                    alertCount++;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case AlertConfigRangeType.AllCharacters:
                                case AlertConfigRangeType.AllOnlineCharacters:
                                    // check against all characters
                                    foreach (var x in CharacterList)
                                    {
                                        if (x.LocationId != 0)
                                        {
                                            path = _universe.FindAndCachePath(x.LocationId, toSystemId);
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
                                                    EnqueueAlert(
                                                    new AlertQueueItem()
                                                    {
                                                        AlertText = alertText,
                                                        MenuText = generateAlertText(entry, path, alert, x.CharacterName, 1, ""),
                                                        Entry = entry,
                                                        Path = path,
                                                        Context = x.CharacterName,
                                                        SoundFile = alert.SoundFile,
                                                        FriendlyName = alert.FriendlyName,
                                                    }
                                                    );
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
                                        foreach (var x in CharacterList)
                                        {
                                            path = _universe.FindAndCachePath(x.LocationId, toSystemId);

                                            alertText = generateAlertText(entry, path, alert, alert.TriggerText, 0, alert.TriggerText);
                                            Log.Debug("ALERT - " + alertText);
                                            EnqueueAlert(
                                                new AlertQueueItem()
                                                {
                                                    AlertText = alertText,
                                                    MenuText = generateAlertText(entry, path, alert, alert.TriggerText, 1, alert.TriggerText),
                                                    Entry = entry,
                                                    Path = path,
                                                    Context = alert.TriggerText,
                                                    SoundFile = alert.SoundFile,
                                                    FriendlyName = alert.FriendlyName,
                                                }
                                                );
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
                        _universe.AddAlert(systemId2);
                        systemId1 = systemId2;
                    }
                    //                    if (this.CheckZoomTo(entry.LogPrefix)) this.ZoomTo(systemId1);
                }

                // check against custom alerts (no system name needed) - only if no alerts generated
                if (alertCount == 0)
                {
                    foreach (AlertConfig alert in Configuration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
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
                            EnqueueAlert(
                                new AlertQueueItem()
                                {
                                    AlertText = alertText,
                                    MenuText = generateAlertText(entry, null, alert, alert.TriggerText, 1, alert.TriggerText),
                                    Entry = entry,
                                    Path = null,
                                    Context = alert.TriggerText,
                                    SoundFile = alert.SoundFile,
                                    FriendlyName = alert.FriendlyName,
                                }
                                );
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
                using (IEnumerator<KeyValuePair<int, SolarSystem>> enumerator = this._universe.SolarSystems.Select(tempSolarSystem => new
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
                using (IEnumerator<KeyValuePair<int, SolarSystem>> enumerator = this._universe.SolarSystems.Select(tempSolarSystem => new
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
                var c = CharacterList.Where(o => o.CharacterName == characterName).FirstOrDefault();
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
                    CharacterList.Add(c);
                }

                CheckEveScoutConnection(c);
                // TODO                if (this._conf.CameraFollowCharacter && this.CentreOnCharacter.Text == key) this.ZoomTo(num1);
            }

            SetupStatusBar();

            if (foundSystemId == -1)
                return;
            Log.Information("System Change: " + characterName + " (" + foundSystemName + ")");

        }

        private void _localWatcher_ChatServerChange(object sender, ProcessChatServerChangeEventArgs e)
        {
            Log.Information("Chat Server: " + (e.ChatServerUp == true ? "Up" : "Down"));
            ChatServerStatus = e.ChatServerUp;
            ChatServerDown.Visible = !e.ChatServerUp;
        }

        private void Quesadilla_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.InitialPosition = Location;
            Configuration.WindowSize = new System.Drawing.Point(Width, Height);
            Configuration.StayOnTop = TopMost;
            _universe.WriteESICharacters(CHARACTER_CACHE_FILE);
            Configuration.Save(configuration_filename);
        }


        private void trayIconMenuClose(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void trayIconMenuAlwaysOnTop(object sender, EventArgs e)
        {
            Configuration.StayOnTop = !Configuration.StayOnTop;
            TopMost = Configuration.StayOnTop;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSShowWindow(object sender, EventArgs e)
        {
            Configuration.DPSShowWindow = !Configuration.DPSShowWindow;
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
            Configuration.ShowZKillAlerts = !Configuration.ShowZKillAlerts;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSAlwaysOnTop(object sender, EventArgs e)
        {
            Configuration.DPSGraphStayOnTop = !Configuration.DPSGraphStayOnTop;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
            if (DPSGraphWindow != null) DPSGraphWindow.TopMost = Configuration.DPSGraphStayOnTop;
        }
        private void trayIconMenuDPSSaveCSV(object sender, EventArgs e)
        {
            Configuration.DPSSaveCSV = !Configuration.DPSSaveCSV;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSAllToons(object sender, EventArgs e)
        {
            Configuration.DPSAllToons = !Configuration.DPSAllToons;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;

            if (InCombat && DPSGraphWindow != null && Configuration.DPSShowWindow)
            {
                DPSGraphWindow.Close();
                DPSGraphWindow = null;
            }
        }

        private void trayIconMenuDPSTransparent(object sender, EventArgs e)
        {
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
            Configuration.DPSGraphTransparent = ((MenuItem)sender).Checked;

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
            Configuration.AlertTransparent = ((MenuItem)sender).Checked;
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
            var configAlert = Configuration.AlertConfiguration.Find(o => o.FriendlyName == m.Text);
            if (configAlert != null)
            {
                m.Checked = !m.Checked;
                configAlert.Enabled = !configAlert.Enabled;
            }
        }

        private void trayIconDoubleClick(object sender, EventArgs e)
        {
            if (ActiveAlerts.Count > 0)
            {
                var alert = ActiveAlerts.Reverse().Take(1).ToList().First();
                var NewAlertWindow = new QuesadillaAlertPopup(alert, Configuration, LoadImageFromFile(Configuration.AlertPopupBackground), this);
                NewAlertWindow.Show(this);
            }
        }

        private void trayIconMenuConfigurationClick(object sender, EventArgs e)
        {
            if (!ConfigurationWindowOpen)
            {

                Configuration.InitialPosition = Location;
                Configuration.WindowSize = new System.Drawing.Point(Width, Height);
                Configuration.StayOnTop = TopMost;

                var ConfigurationWindow = new QuesadillaConfiguration(Configuration, menuConfigurationWindow, this);
                ((MenuItem)sender).Enabled = !((MenuItem)sender).Enabled;
                ConfigurationWindow.Show(this);
            }
        }

        private void trayIconMenuLogfileClick(object sender, EventArgs e)
        {
            // enable / disable config item
            MenuItem m = (MenuItem)sender;
            var watcher = watchers.Find(o => o.FriendlyName == m.Text);
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
            Configuration.AlertPopupBackground = m.Text;
        }
        private void logFile_ProcessCombat(object sender, ProcessCombatEventArgs e)
        {

            string combatEntity = "";

            //Log.Information("COMBAT:" + e.CombatEvent);
            if (e.Entry != null)
            {
                if (InCombat == false && Configuration.DPSShowWindow)
                {
                    DPSGraphWindow = new QuesadillaDPSGraph(this);
                    DPSGraphWindow.Show();
                }

                InCombat = true;
                if (!_combat.ContainsKey(e.Entry.CharacterName))
                {
                    _combat.Add(e.Entry.CharacterName, new List<CombatLine>());
                }
                //Log.Information(e.Entry.LineContent);

                Match match = _CombatRegexMissesYouCompletely.Match(e.Entry.LineContent);
                if (match.Success)
                {
                    _combat[e.Entry.CharacterName].Add(new CombatLine()
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

                    _combat[e.Entry.CharacterName].Add(new CombatLine()
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
                    foreach (AlertConfig alert in Configuration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
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
                            var c = CharacterList.Find(o => o.CharacterName.Equals(e.CharacterName));

                            bool sendAlert = true;

                            if (c != null)
                            {
                                path = new PathInfo()
                                {
                                    FromSystemId = c.LocationId,
                                    ToSystemId = c.LocationId,
                                    FromSystemName = c.Location,
                                    ToSystemName = c.Location,
                                    ToSystemRegionName = _universe.SolarSystems[c.LocationId].RegionName,
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
                                EnqueueAlert(
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
                                    }
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
                if (!_ISK.ContainsKey(e.Entry.CharacterName))
                {
                    _ISK.Add(e.Entry.CharacterName, new List<IskLine>());
                }

                Match match = _ISKRegexISKData.Match(e.Entry.LineContent);
                if (match.Success)
                {
                    double iskAmount = 0;

                    var amount = match.Groups["iskAmount"].ToString();
                    iskAmount = Double.Parse(amount.Trim());

                    _ISK[e.Entry.CharacterName].Add(new IskLine()
                    {
                        TimeAdded = e.Entry.TimeAdded,
                        IskAmount = iskAmount,
                    });
                }
            }
        }
        private void logFile_ProcessDecloak(object sender, ProcessDecloakEventArgs e)
        {
            //Log.Information("COMBAT:" + e.CombatEvent);
            if (e.Entry != null)
            {

                PathInfo path = null;
                var c = CharacterList.Find(o => o.CharacterName.Equals(e.CharacterName));
                string alertText = "";

                foreach (AlertConfig alert in Configuration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
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

                    EnqueueAlert(
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
                    }
                    );
                }
            }
        }

        private void buttonIntelSize_Click(object sender, EventArgs e)
        {
            if (intelLog.Height == _intelSizeMin)
            {
                // max it big
                intelLog.Height = _intelSizeMax;
                Height = intelLog.Height + _windowExtraHeight;
                buttonIntelSize.Text = "▼";
            }
            else
            {
                // make it small
                intelLog.Height = _intelSizeMin;
                Height = intelLog.Height + _windowExtraHeight;
                buttonIntelSize.Text = "▲";
            }
            buttonIntelSize.Top = intelLog.Top + 2;
            buttonIntelSize.Left = intelLog.Width - buttonIntelSize.Width - 2;
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

        private void UpdateEveProcesses()
        {
            this._processMonitor.GetUpdatedProcesses(out List<ProcessInfo> addedProcesses, out List<ProcessInfo> updatedProcesses, out List<ProcessInfo> removedProcesses);
            string previousToon = CurrentActiveToon;

            string foregroundWindowTitle = null;
            var ptr = this._processMonitor.GetForegroundWindowHandle();

            var PrevRect = new Rect()
            {
                Top = currentEveWindowRect.Top,
                Bottom = currentEveWindowRect.Bottom,
                Left = currentEveWindowRect.Left,
                Right = currentEveWindowRect.Right,
            };

            var result = this._processMonitor.GetWindowDimensions(ptr, ref currentEveWindowRect, ref currentEveClientPoint);

            if (ptr != null)
            {
                foregroundWindowTitle = this._processMonitor.GetTitleOfKnownProcess(ptr);
                if (foregroundWindowTitle != null)
                {
                    lblCurrentClient.Visible = true;
                    CurrentActiveToon = foregroundWindowTitle.Replace("EVE - ", "");
                    lblCurrentClient.Text = CurrentActiveToon;

                    SetupStatusBar();

                    // sort out DPS Graph
                    if (previousToon == null || previousToon == "" || previousToon != CurrentActiveToon)
                    {
                        if (Configuration.DPSShowWindow && DPSGraphWindow != null)
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
                    if (Configuration.DPSShowWindow && DPSGraphWindow != null)
                    {
                        DPSGraphWindow.Hide();
                    }
                }
            }

            if (foregroundWindowTitle == null)
            {
                lblCurrentClient.Visible = false;
                CurrentActiveToon = "";
                lblCurrentClient.Text = CurrentActiveToon;
                SetupStatusBar();
            }

            // just double check thru charachter list active clients to make sure there is still an active client.
            foreach (var c in CharacterList)
            {
                c.IsClientActive = _processMonitor.IsCharStillActive(c.CharacterName);
            }
        }
        public string GetIskLastHour(string toon)
        {
            var ret = "";
            double isk = 0;

            //var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
            var expireTime = DateTime.Now.AddSeconds(-3600);

            foreach (var i in _ISK[toon].Where(o => o.TimeAdded >= expireTime))
            {
                isk += i.IskAmount;
            }

            if (isk > 0)
            {
                ret = Math.Truncate(isk).ToString("0,000 ISK");
            }

            return ret;
        }

        private void ZKill_ProcessNewData(object sender, ZKillEventArgs e)
        {
            string systemName = "---";
            string regionName = "---";

            var victimShipName = _universe.ItemTypes.ContainsKey((int)e.Kill.VictimShipTypeId) ? _universe.ItemTypes[(int)e.Kill.VictimShipTypeId].Name : "Unknown:" + e.Kill.VictimShipTypeId.ToString();
            e.Kill.VictimShipName = victimShipName;

            if (_universe.SolarSystems.ContainsKey((int)e.Kill.SolarSystemId))
            {
                systemName = _universe.SolarSystems[(int)e.Kill.SolarSystemId].Name;
                regionName = _universe.SolarSystems[(int)e.Kill.SolarSystemId].RegionName;
                e.Kill.SolarSystemName = systemName;
                e.Kill.RegionName = regionName;

                if (_universe.SolarSystems[(int)e.Kill.SolarSystemId].Kills == null)
                {
                    _universe.SolarSystems[(int)e.Kill.SolarSystemId].Kills = new List<ZKillKill>();
                }
                _universe.SolarSystems[(int)e.Kill.SolarSystemId].Kills.Add(e.Kill);
                lock (ZKillKills)
                {
                    ZKillKills.Add(e.Kill);
                }
            }

            Log.Information($"ZKILL Event handler {e.Kill.ID} {regionName}:{systemName} {victimShipName} {_universe.GetCharacterName((int)e.Kill.VictimCharacterId)}");
            e.Kill.Context = generateZKillAlertText(e.Kill, null, Configuration.ZKillContextText, "");

            // check if zkill system matches any range alerts
            // and that report zkill flag is set
            if (!Configuration.ShowZKillAlerts) return;

            var toSystemId = (int)e.Kill.SolarSystemId;

            int homeSystemId = _universe.HomeSolarSystemID;
            var homeSystemPathInfo = _universe.FindAndCachePath(homeSystemId, toSystemId);
            if (homeSystemPathInfo != null)
            {
                Log.Debug("Jumps from HOME:" + _universe.SolarSystems[homeSystemId].Name + " to " + _universe.SolarSystems[toSystemId].Name + " is " + homeSystemPathInfo.TotalJumps);
            }
            foreach (var c in CharacterList.Where(o => o.LocationId > 0))
            {
                var CurrectLocationPathInfo = _universe.FindAndCachePath(c.LocationId, toSystemId);
                if (CurrectLocationPathInfo != null)
                {
                    Log.Debug("Jumps from " + c.CharacterName + ":" + _universe.SolarSystems[c.LocationId].Name + " to " + _universe.SolarSystems[toSystemId].Name + " is " + CurrectLocationPathInfo.TotalJumps);
                }
            }

            // Range alerts
            foreach (AlertConfig alert in Configuration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
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
                            alertText = generateZKillAlertText(e.Kill, path, Configuration.ZKillAlertText,"");
                            Log.Information("ALERT - " + alertText);

                            EnqueueAlert(
                                new AlertQueueItem()
                                {
                                    AlertText = alertText,
                                    MenuText = generateZKillAlertText(e.Kill, path, Configuration.ZKillMenuText, ""),
                                    Kill = e.Kill,
                                    Path = path,
                                    Context = homeSystemPathInfo.FromSystemName,
                                    SoundFile = Configuration.ZKillSound,
                                    FriendlyName = "zkill",
                                }
                                );
                        }
                        break;

                    case AlertConfigRangeType.System:
                        // check against specific system
                        path = _universe.FindAndCachePath(_universe.Names[alert.TriggerText], toSystemId);
                        sendAlert = checkJumpsRange(alert, path);
                        if (sendAlert)
                        {
                            alertText = generateZKillAlertText(e.Kill, path, Configuration.ZKillAlertText, "");
                            Log.Information("ALERT - " + alertText);
                            EnqueueAlert(
                                new AlertQueueItem()
                                {
                                    AlertText = alertText,
                                    MenuText = generateZKillAlertText(e.Kill, path, Configuration.ZKillMenuText, ""),
                                    Kill = e.Kill,
                                    Path = path,
                                    Context = path.FromSystemName,
                                    SoundFile = Configuration.ZKillSound,
                                    FriendlyName = "zkill",
                                }
                                );
                        }
                        break;
                    case AlertConfigRangeType.OneCharacter:
                    case AlertConfigRangeType.OneOnlineCharacter:
                        // check against one specific character
                        foreach (var x in CharacterList.Where(o => o.CharacterName == alert.TriggerText))
                        {
                            if (x.LocationId != 0)
                            {
                                path = _universe.FindAndCachePath(x.LocationId, toSystemId);
                                sendAlert = checkJumpsRange(alert, path);
                                if (sendAlert)
                                {
                                    alertText = generateZKillAlertText(e.Kill, path, Configuration.ZKillAlertText, x.CharacterName);
                                    Log.Information("ALERT - " + alertText);
                                    if (
                                        alert.AlertRangeType == AlertConfigRangeType.OneCharacter ||
                                        alert.AlertRangeType == AlertConfigRangeType.OneOnlineCharacter && x.IsClientActive
                                        )
                                    {
                                        EnqueueAlert(
                                        new AlertQueueItem()
                                        {
                                            AlertText = alertText,
                                            MenuText = generateZKillAlertText(e.Kill, path, Configuration.ZKillMenuText, x.CharacterName), 
                                            Kill = e.Kill,
                                            Path = path,
                                            Context = x.CharacterName,
                                            SoundFile = Configuration.ZKillSound,
                                            FriendlyName = alert.FriendlyName,
                                        }
                                        );
                                    }
                                }
                            }
                        }
                        break;
                    case AlertConfigRangeType.AllCharacters:
                    case AlertConfigRangeType.AllOnlineCharacters:
                        // check against all characters
                        foreach (var x in CharacterList)
                        {
                            if (x.LocationId != 0)
                            {
                                path = _universe.FindAndCachePath(x.LocationId, toSystemId);
                                sendAlert = checkJumpsRange(alert, path);
                                if (sendAlert)
                                {
                                    alertText = generateZKillAlertText(e.Kill, path, Configuration.ZKillAlertText, x.CharacterName);
                                    Log.Information("ALERT - " + alertText);
                                    if (
                                        alert.AlertRangeType == AlertConfigRangeType.AllCharacters ||
                                        alert.AlertRangeType == AlertConfigRangeType.AllOnlineCharacters && x.IsClientActive
                                        )
                                    {
                                        EnqueueAlert(
                                        new AlertQueueItem()
                                        {
                                            AlertText = alertText,
                                            MenuText = generateZKillAlertText(e.Kill, path, Configuration.ZKillMenuText, x.CharacterName),
                                            Kill = e.Kill,
                                            Path = path,
                                            Context = x.CharacterName,
                                            SoundFile = Configuration.ZKillSound,
                                            FriendlyName = "zkill",
                                        }
                                        );
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        public void SetupStatusBar()
        {
            StringBuilder status = new StringBuilder();

            if (CurrentActiveToon != null && CurrentActiveToon != "")
            {
                var c = CharacterList.Where(o => o.CharacterName == CurrentActiveToon).FirstOrDefault();
                if (c != null)
                {
                    if (c.ClosestThera != null)
                    {
                        status.Append("  Thera <- ");
                        status.Append(c.ClosestTheraPath.ToSystemName);
                        status.Append(" (");
                        status.Append(c.ClosestTheraPath.TotalJumps);
                        status.Append(") - Sig:");
                        status.Append((c.ClosestThera.FromSystemId == c.ClosestTheraPath.ToSystemId) ? c.ClosestThera.FromSignature : c.ClosestThera.ToSignature );
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
            } else
            {
                status.Append("No active Character");
            }

            lblStatus.Text = status.ToString().Trim();
        }

        private void CheckEveScoutConnection(Character c)
        {

            if (c == null || c.LocationId == 0) return;

            var ClosestThera = EveScoutProcessor.FindClosest(_universe, c.LocationId, "Thera");
            var ClosestTurnur = EveScoutProcessor.FindClosest(_universe, c.LocationId, "Turnur");
            PathInfo closePath;

            if (ClosestThera != null)
            {
                closePath = _universe.FindAndCachePath(c.LocationId, (ClosestThera.FromSystemId == EveScoutManager.SYSTEMID_THERA) ? (int)ClosestThera.ToSystemId : (int)ClosestThera.FromSystemId);
                if (closePath != null)
                {
                    Log.Information($"EveScount: Closest Thera {ClosestThera.ToSystemId}:{ClosestThera.FromSystemId} {closePath.TotalJumps} {closePath.ToSystemName}");
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
                closePath = _universe.FindAndCachePath(c.LocationId, (ClosestTurnur.FromSystemId == EveScoutManager.SYSTEMID_TURNUR) ? (int)ClosestTurnur.ToSystemId : (int)ClosestTurnur.FromSystemId);
                if (closePath != null)
                {
                    Log.Information($"EveScount: Closest Turnur {ClosestTurnur.ToSystemId}:{ClosestTurnur.FromSystemId} {closePath.TotalJumps} {closePath.ToSystemName}");
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
            foreach(var c in CharacterList.Where(o => o.IsClientActive)) { 
                CheckEveScoutConnection(c);
            }
        }

        private void trayIconMenuShowHistoricAlert(object sender, EventArgs e)
        {
            AlertQueueItem alert = (AlertQueueItem)((MenuItem)sender).Tag;
            var NewAlertWindow = new QuesadillaAlertPopup(alert, Configuration, LoadImageFromFile(Configuration.AlertPopupBackground), this);
            NewAlertWindow.Show(this);
        }
    }
}
