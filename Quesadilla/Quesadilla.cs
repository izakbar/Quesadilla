using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.Resources;
using Quesadilla.Classes.SolarSystems;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Quesadilla.Extensions.StringExtensions;

namespace Quesadilla
{
    public partial class Quesadilla : Form
    {

        private readonly Regex _CombatRegexMissesYouCompletely = new Regex("(?<combatEntity>.*)\\smisses\\syou\\scompletely", RegexOptions.Compiled);
        private readonly Regex _CombatRegexCombatData = new Regex("^<color=0x........><b>(?<combatAmount>.*)</b>\\s<color=0x........><font size=10>(?<combatContext>.*)</font>\\s<b><color=0xffffffff>(?<combatEntity>.*)</b><font size=10><color=0x77ffffff>\\s-\\s(?<combatText>.*)", RegexOptions.Compiled);

        private Queue<LogEntry> _newIntelQueue = new Queue<LogEntry>();

        private SolarSystemManager _solarSystems = new SolarSystemManager();
        private Config Configuration = new Config();
        private List<LogWatcher> watchers = new List<LogWatcher>();

        private Queue<AlertQueueItem> NewAlerts = new Queue<AlertQueueItem>();
        private Queue<AlertQueueItem> ActiveAlerts = new Queue<AlertQueueItem>();

        private List<QuesadillaSound> Sounds = new List<QuesadillaSound>();
        private List<QuesadillaBackground> Backgrounds = new List<QuesadillaBackground>();

        private string configuration_filename = "Quesadilla.config.json";
        private int _intelSizeMax = 200;
        private int _intelSizeMin = 48;

        private MenuItem menuConfiguredAlerts = new MenuItem("Configured Alerts");
        private MenuItem menuBackgrounds = new MenuItem("Popup Background");
        private MenuItem menuLatestAlerts = new MenuItem("Latest Alerts");
        private MenuItem menuLogWatchers = new MenuItem("Logfiles");
        private MenuItem menuConfiguration = new MenuItem("Configuration");

        public bool AlertShowing = false;
        private bool ChatServerStatus = true;
        private bool InCombat = false;
        private int TicksSinceLastCleardown = 0;

        private Dictionary<string, List<CombatLine>> _combat = new Dictionary<string, List<CombatLine>>();

        // TODO - config things - really need to go into config structure later
        public List<Character> CharacterList { get; set; }

        private NotifyIcon trayIcon;

        public Quesadilla()
        {
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

            RebuildAlertsMenu();
            RebuildBackgroundsMenu();
            RebuildLatestAlertsMenu();

            trayIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                    menuConfiguration,
                    menuLatestAlerts,
                    new MenuItem("-"),
                    new MenuItem("Exit", new EventHandler(trayIconMenuClose)),
            });
            menuConfiguration.MenuItems.Add(menuBackgrounds);
            menuConfiguration.MenuItems.Add(menuConfiguredAlerts);
            menuConfiguration.MenuItems.Add(menuLogWatchers);

            // stop main tick timer!
            mainTimer.Enabled = false;
            mainTimer.Interval = Configuration.TickMiliseconds;

            CharacterList = new List<Character>();
            _solarSystems.ReadSolarSystems("Quesadilla.Systems.Json");

            _solarSystems.HomeSolarSystemID = _solarSystems.Names[Configuration.HomeSystem];

            watchers.Add(new LogWatcher("catch", Classes.Logfile.LogFileType.ChatIntel, "Catch Intel"));
            watchers.Add(new LogWatcher("ftn", Classes.Logfile.LogFileType.ChatIntel, "Fountain Intel"));
            watchers.Add(new LogWatcher("period", Classes.Logfile.LogFileType.ChatIntel, "Period Basis Intel"));

            watchers.Add(new LogWatcher("querious", Classes.Logfile.LogFileType.ChatIntel, "Querious Intel"));
            watchers.Add(new LogWatcher("delve", Classes.Logfile.LogFileType.ChatIntel, "Delve Intel"));

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
                else
                {
                    watcher.ProcessNewData += new EventHandler<ProcessNewDataEventArgs>(this.logFile_ProcessNewData);
                    watcher.ProcessCombat += new EventHandler<ProcessCombatEventArgs>(this.logFile_ProcessCombat);
                }
            }

            // start watchers
            foreach (LogWatcher watcher in watchers)
            {
                watcher.StartWatch();
            }

            // start main tick timer
            mainTimer.Enabled = true;
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
                        if (name.StartsWith("background"))
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
            foreach (var alert in ActiveAlerts.Reverse().Take(Configuration.ShowLastUnexpiredAlerts).ToList())
            {
                string menuText = alert.Entry.TimeAdded.ToString("HH:mm") + " " + (alert.Path != null ? alert.Path.ToSystemName  : "") + " - " + alert.MenuText;
                MenuItem menuAlert = new MenuItem(menuText);
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
            foreach (var background in Backgrounds.OrderBy(o => o.BackgroundName).ToList() )
            {
                MenuItem menuBackground = new MenuItem(background.BackgroundName, trayIconMenuBackgroundClick);
                if ( Configuration.AlertPopupBackground == background.BackgroundName) menuBackground.Checked = true;
                menuBackgrounds.MenuItems.Add(menuBackground);
            }
        }

        private void Quesadilla_Load(object sender, EventArgs e)
        {
            if (Configuration.InitialPosition != null)
            {
                Location = Configuration.InitialPosition;
                if (Location.X > Screen.FromControl(this).Bounds.Width || Location.X < Screen.FromControl(this).Bounds.X ||
                    Location.Y > Screen.FromControl(this).Bounds.Height || Location.Y < Screen.FromControl(this).Bounds.Y 
                    )
                {
                    Location = new Point(0, 0);
                }

                TopMost = Configuration.StayOnTop;
                Width = Configuration.WindowSize.X >= 100 ? Configuration.WindowSize.X : 100 ;
                Height = Configuration.WindowSize.Y >= 100 ? Configuration.WindowSize.Y : 100;
            }

            buttonIntelSize_Click(null, null);
        }
        private SoundPlayer LoadSoundFromFile(string fileName)
        {

            Stream soundStream = null;

            if ( Sounds.Find(o => o.SoundName == fileName) != null )
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
        private Image LoadImageFromFile(string fileName)
        {
            Bitmap imageStream = null;

            if (Backgrounds.Find(o => o.BackgroundName== fileName) != null)
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
            foreach (LogWatcher watcher in watchers)
            {
                watcher.Tick();
            }

            // dequeue Alerts
            if ( NewAlerts.Count > 0 )
            {
                var NewAlert = NewAlerts.Dequeue();
                Log.Error(NewAlert.AlertText);

                ActiveAlerts.Enqueue(NewAlert);
                // do any extra processing 

                var NewAlertWindow = new QuesadillaAlertPopup(NewAlert, Configuration, LoadImageFromFile(Configuration.AlertPopupBackground));
                NewAlertWindow.Show(this);

                SoundPlayer sound = LoadSoundFromFile(NewAlert.SoundFile);
                if ( sound != null ) sound.Play();

                RebuildLatestAlertsMenu();
            }


            // do cleardown/rebuild of alerts menu - if number of ticks passed
            if (++TicksSinceLastCleardown > Configuration.HowManyTicksBeforeCleardown)
            {
                // Show number of recent alerts
                if (ActiveAlerts.Count > 0)
                {
                    // check if we should expire first entry
                    var a = ActiveAlerts.First();
                    var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
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
            }

            CombatFlag.Visible = InCombat;
            if (InCombat)
            {
                combatOutstanding.Text = _combat.Count.ToString();
            } else
            {
                combatOutstanding.Text = "";
            }
        }
        private void AddNewCharacter(string characterName)
        {
            if (characterName.Trim().Length == 0)
                return;

            this.CharacterList.Add(new Character()
            {
                CharacterName = characterName.Trim(),
            });
            Log.Information("New character found: " + characterName);
        }
        private bool checkJumpsRange(AlertConfig alert, PathInfo path)
        {
            bool checkJumpsRangeComparison(int jumps, AlertConfigJumpsComparison comparison, int? comparisonNumber) { 
                if ( comparison == AlertConfigJumpsComparison.None) return true;
                if ( comparisonNumber is null ) return false;

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

            shouldAlert = 
                checkJumpsRangeComparison(path.TotalJumps, alert.Jumps1Comparison, alert.Jumps1Number) && 
                checkJumpsRangeComparison(path.TotalJumps, alert.Jumps2Comparison, alert.Jumps2Number) &&
                path.TotalJumps < Configuration.IgnoreAsTooManyJumps ;

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
                    route += _solarSystems.SolarSystems[systemId].Name;
                }
            }
            return route;
        }
        private string generateAlertText(LogEntry entry, PathInfo path, AlertConfig alert, string characterName, int type)
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
                returnString = returnString.Replace("{tosystem}", _solarSystems.SolarSystems[path.ToSystemId].Name);
                returnString = returnString.Replace("{region}", _solarSystems.SolarSystems[path.ToSystemId].RegionName);
                returnString = returnString.Replace("{fromsystem}", _solarSystems.SolarSystems[path.FromSystemId].Name);
            }
            returnString = returnString.Replace("{character}", characterName);
            returnString = returnString.Replace("{text}", entry.LineContent);
            returnString = returnString.Replace("{reporter}", entry.PlayerName);
            return returnString;
        }
        private void EnqueueAlert(AlertQueueItem alert)
        {
            // only add to new alerts if not already in new alerts. This should prever an alert for each character on same logfile entry
            var f = NewAlerts.Where(o => o.Path.ToSystemId == alert.Path.ToSystemId).ToList();
            if (f.Count == 0 || NewAlerts.Count == 0)
            {
                alert.RouteText = PathToString(alert.Path);
                NewAlerts.Enqueue(alert);
            }
            if ( f.Count != 0)
            {
                if (alert.Path.TotalJumps < f.First().Path.TotalJumps )
                {
                    f.First().Path = alert.Path;
                    f.First().AlertText = alert.AlertText;
                    f.First().Entry= alert.Entry;
                    f.First().Context = alert.Context;
                    f.First().RouteText = alert.RouteText;
                    f.First().SoundFile= alert.SoundFile;
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
                        _solarSystems.SolarSystems.Values.Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => tempSystem.MatchNameRegex(entry.LineContent)))
                        .Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => !Configuration.ignoreSystems.Contains(this._solarSystems.Names[tempSystem.Name])))
                        )
                        intSet.Add(_solarSystems.Names[solarSystem.Name]);
                }

                if (intSet.Count > 0)
                {
                    Log.Information(entry.LineContent);
                    entry.MatchedIds = intSet;
                    foreach (int toSystemId in intSet)
                    {
                        int homeSystemId = _solarSystems.HomeSolarSystemID;

                        // Cache paths - if not already stored
                        var homeSystemPathInfo = _solarSystems.FindAndCachePath(homeSystemId, toSystemId);
                        Log.Debug("Jumps from HOME:" + _solarSystems.SolarSystems[homeSystemId].Name + " to " + _solarSystems.SolarSystems[toSystemId].Name + " is " + homeSystemPathInfo.TotalJumps);
                        foreach (var c in CharacterList.Where(o => o.LocationId > 0))
                        {
                            var CurrectLocationPathInfo = _solarSystems.FindAndCachePath(c.LocationId, toSystemId);
                            Log.Debug("Jumps from " + c.CharacterName + ":" + _solarSystems.SolarSystems[c.LocationId].Name + " to " + _solarSystems.SolarSystems[toSystemId].Name + " is " + CurrectLocationPathInfo.TotalJumps);
                        }

                        // Range alerts
                        foreach (AlertConfig alert in Configuration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
                        {
                            return tempTrigger.Enabled;
/*                            if (!tempTrigger.Enabled)
                                return false;
                            return tempTrigger.AlertType == AlertConfigType.Range;
*/
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
                                        alertText = generateAlertText(entry, path, alert, "",0);
                                        Log.Debug("ALERT - " + alertText);
                                        EnqueueAlert(
                                            new AlertQueueItem()
                                            {
                                                AlertText = alertText,
                                                MenuText = generateAlertText(entry, path, alert, "", 1),
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
                                    path = _solarSystems.FindAndCachePath(_solarSystems.Names[alert.TriggerText], toSystemId);
                                    sendAlert = checkJumpsRange(alert, path);
                                    if (sendAlert)
                                    {
                                        alertText = generateAlertText(entry, path, alert, "",0);
                                        Log.Debug("ALERT - " + alertText);
                                        EnqueueAlert(
                                            new AlertQueueItem()
                                            {
                                                AlertText = alertText,
                                                MenuText = generateAlertText(entry, path, alert, "", 1),
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
                                    // check against one specific character
                                    foreach (var x in CharacterList.Where(o => o.CharacterName == alert.TriggerText))
                                    {
                                        path = _solarSystems.FindAndCachePath(x.LocationId, toSystemId);
                                        sendAlert = checkJumpsRange(alert, path);
                                        if (sendAlert)
                                        {
                                            alertText = generateAlertText(entry, path, alert, x.CharacterName,0);
                                            Log.Debug("ALERT - " + alertText);
                                            EnqueueAlert(
                                                new AlertQueueItem()
                                                {
                                                    AlertText = alertText,
                                                    MenuText = generateAlertText(entry, path, alert, x.CharacterName, 1),
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
                                    break;
                                case AlertConfigRangeType.AllCharacters:
                                    // check against all characters
                                    foreach (var x in CharacterList)
                                    {
                                        path = _solarSystems.FindAndCachePath(x.LocationId, toSystemId);
                                        sendAlert = checkJumpsRange(alert, path);
                                        if (sendAlert)
                                        {
                                            alertText = generateAlertText(entry, path, alert, x.CharacterName,0);
                                            Log.Debug("ALERT - " + alertText);
                                            EnqueueAlert(
                                                new AlertQueueItem()
                                                {
                                                    AlertText = alertText,
                                                    MenuText = generateAlertText(entry, path, alert, x.CharacterName, 1),
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
                                    break;
                                case AlertConfigRangeType.None:
                                    // must be custom text
                                    if (entry.LineContent.Contains(alert.TriggerText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        foreach (var x in CharacterList)
                                        {
                                            path = _solarSystems.FindAndCachePath(x.LocationId, toSystemId);

                                            alertText = generateAlertText(entry, path, alert, alert.TriggerText,0);
                                            Log.Debug("ALERT - " + alertText);
                                            EnqueueAlert(
                                                new AlertQueueItem()
                                                {
                                                    AlertText = alertText,
                                                    MenuText = generateAlertText(entry, path, alert, alert.TriggerText, 1),
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
                        _solarSystems.AddAlert(systemId2);
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
                            
                            string alertText = generateAlertText(entry, null, alert, alert.TriggerText,0);
                            Log.Debug("ALERT - " + alertText);
                            EnqueueAlert(
                                new AlertQueueItem()
                                {
                                    AlertText = alertText,
                                    MenuText = generateAlertText(entry, null, alert, alert.TriggerText, 1),
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
                using (IEnumerator<KeyValuePair<int, SolarSystem>> enumerator = this._solarSystems.SolarSystems.Select(tempSolarSystem => new
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
                using (IEnumerator<KeyValuePair<int, SolarSystem>> enumerator = this._solarSystems.SolarSystems.Select(tempSolarSystem => new
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
                    CharacterList.Add(new Character()
                    {
                        CharacterName = characterName,
                        LocationId = foundSystemId,
                        Location = foundSystemName,
                    });
                }
                // TODO                if (this._conf.CameraFollowCharacter && this.CentreOnCharacter.Text == key) this.ZoomTo(num1);
            }

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
            Configuration.WindowSize = new Point(Width, Height);
            Configuration.StayOnTop = TopMost;
            Configuration.Save(configuration_filename);
        }


        private void trayIconMenuClose(object sender, EventArgs e)
        {
            Application.Exit();
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
            foreach(MenuItem z in menuBackgrounds.MenuItems)
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
                InCombat = true;
                if (!_combat.ContainsKey(e.Entry.CharacterName))
                {
                    _combat.Add(e.Entry.CharacterName, new List<CombatLine>()) ;
                }
                //Log.Information(e.Entry.LineContent);

                Match match = _CombatRegexMissesYouCompletely.Match(e.Entry.LineContent);
                if (match.Success)
                {
                    _combat[e.Entry.CharacterName].Add(new CombatLine() {
                        TimeAdded = e.Entry.TimeAdded,
                        Entity = match.Groups["combatEntity"].ToString(),
                    });
                }  else
                {
                    match = _CombatRegexCombatData.Match(e.Entry.LineContent);
                    if (match.Success)
                    {
                        int dmgAmount = 0;
                        int neutAmount = 0;

                        var amount = match.Groups["combatAmount"].ToString();
                        if (amount.Contains("GJ") )
                        {
                            neutAmount = Int32.Parse(amount.Replace(" GJ", "").Trim());
                        } else
                        {
                            dmgAmount = Int32.Parse(amount.Trim());
                        }

                        _combat[e.Entry.CharacterName].Add(new CombatLine() {
                            TimeAdded = e.Entry.TimeAdded,
                            Entity = match.Groups["combatEntity"].ToString(),
                            Text = match.Groups["combatText"].ToString(),
                            NeutAmount = neutAmount,
                            DamageAmount = dmgAmount,
                        });
                    }
                }

            } 
            else
            {
                InCombat = false;
            }
        }

        private void buttonIntelSize_Click(object sender, EventArgs e)
        {
            if ( intelLog.Height == _intelSizeMin )
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
            buttonIntelSize.Top = intelLog.Top + 2;
            buttonIntelSize.Left = intelLog.Width - buttonIntelSize.Width - 2;
            intelLog.ScrollToCaret();
        }
    }
}
