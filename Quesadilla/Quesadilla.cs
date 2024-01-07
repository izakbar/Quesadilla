using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Properties;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace Quesadilla
{
    public partial class Quesadilla : Form
    {
        private Queue<LogEntry> _newIntelQueue = new Queue<LogEntry>();

        private SolarSystemManager _solarSystems = new SolarSystemManager();
        private Config Configuration = new Config();
        private List<LogWatcher> watchers = new List<LogWatcher>();

        private Queue<AlertQueueItem> NewAlerts = new Queue<AlertQueueItem>();
        private Queue<AlertQueueItem> ActiveAlerts = new Queue<AlertQueueItem>();

        private string configuration_filename = "Quesadilla.config.json";

        private MenuItem menuAlerts = new MenuItem("Alerts");
        public bool AlertShowing = false;

        // TODO - config things - really need to go into config structure later
        public List<Character> CharacterList { get; set; }

        private NotifyIcon trayIcon;

        public Quesadilla()
        {
            InitializeComponent();

            Configuration = Config.Load(configuration_filename);

            // Initialize tray
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.q,
                ContextMenu = new ContextMenu(),
                Visible = true
            };

            RebuildAlertsMenu();

            trayIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                    menuAlerts,
                    new MenuItem("-"),
                    new MenuItem("Exit", new EventHandler(trayIconMenuClose)),
            });

            // stop main tick timer!
            mainTimer.Enabled = false;

            CharacterList = new List<Character>();
            _solarSystems.ReadSolarSystems("Quesadilla.Systems.Json");

            _solarSystems.HomeSolarSystemID = _solarSystems.Names[Configuration.HomeSystem];

            watchers.Add(new LogWatcher("catch", Classes.Logfile.LogFileType.ChatIntel));
            watchers.Add(new LogWatcher("ftn", Classes.Logfile.LogFileType.ChatIntel));
            watchers.Add(new LogWatcher("period", Classes.Logfile.LogFileType.ChatIntel));

            watchers.Add(new LogWatcher("querious", Classes.Logfile.LogFileType.ChatIntel));
            watchers.Add(new LogWatcher("delve", Classes.Logfile.LogFileType.ChatIntel));

            watchers.Add(new LogWatcher(null, Classes.Logfile.LogFileType.Local));

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

        private void RebuildAlertsMenu()
        {
            menuAlerts.MenuItems.Clear();
            foreach (var alertConfigItem in Configuration.AlertConfiguration)
            {
                MenuItem menuAlert = new MenuItem(alertConfigItem.FriendlyName);
                menuAlert.Checked = alertConfigItem.Enabled;
                menuAlerts.MenuItems.Add(menuAlert);
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

        }
        private SoundPlayer LoadSoundFromFile(string fileName)
        {

            Stream soundStream = null;

            switch (fileName)
            {
                case "1up1.wav":
                    soundStream = Properties.Resources._1up1;
                    break;
                case "Boo2.wav":
                    soundStream = Properties.Resources.Boo2;
                    break;
                case "Coin.wav":
                    soundStream = Properties.Resources.Coin;
                    break;
                case "KamekLaugh.wav":
                    soundStream = Properties.Resources.KamekLaugh;
                    break;
                case "Powerup.wav":
                    soundStream = Properties.Resources.Powerup;
                    break;
                case "RedCoin2.wav":
                    soundStream = Properties.Resources.RedCoin2;
                    break;
                case "RedCoin3.wav":
                    soundStream = Properties.Resources.RedCoin3;
                    break;
                case "StarCoin.wav":
                    soundStream = Properties.Resources.StarCoin;
                    break;
                case "SuitFly.wav":
                    soundStream = Properties.Resources.SuitFly;
                    break;
                case "SuitSpin.wav":
                    soundStream = Properties.Resources.SuitSpin;
                    break;
                case "Whistle.wav":
                    soundStream = Properties.Resources.Whistle;
                    break;
                case "tuba.wav":
                    soundStream = Properties.Resources.tuba;
                    break;
                case "xylophone.wav":
                    soundStream = Properties.Resources.xylophone;
                    break;
                case "oldtrombone.wav":
                    soundStream = Properties.Resources.oldtrombone;
                    break;
                case "harpsichord.wav":
                    soundStream = Properties.Resources.harpsichord;
                    break;
                case "button5.wav":
                    soundStream = Properties.Resources.button5;
                    break;
                case "modsinglenote.wav":
                    soundStream = Properties.Resources.modsinglenote;
                    break;
                case "flute.wav":
                    soundStream = Properties.Resources.flute;
                    break;
                case "violin.wav":
                    soundStream = Properties.Resources.violin;
                    break;
                case "skamos66.wav":
                    soundStream = Properties.Resources.skamos66;
                    break;
                case "trumpet.wav":
                    soundStream = Properties.Resources.trumpet;
                    break;


            }
            if (soundStream != null)
            {
                SoundPlayer soundPlayer = new SoundPlayer((Stream)soundStream);
                soundPlayer.Load();
                return soundPlayer;
            }

            if (soundStream == null && File.Exists(fileName))
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

                var NewAlertWindow = new QuesadillaAlertPopup(NewAlert, Configuration);
                NewAlertWindow.Show(this);

                SoundPlayer sound = LoadSoundFromFile(NewAlert.SoundFile);
                if ( sound != null ) sound.Play();
                
            }

            // Show number of recent alers
            if ( ActiveAlerts.Count > 0 )
            {
                // check if we should expire first entry
                var a = ActiveAlerts.First();
                var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
                if ( diffInSeconds > Configuration.ExpireAlertSeconds )
                {
                    // expired so dequeue
                    a = ActiveAlerts.Dequeue();
                }

                alertsOutstanding.Text = ActiveAlerts.Count.ToString();
            } else
            {
                alertsOutstanding.Text = "";
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
            foreach(int systemId in path.PathSystems)
            {
                if ( route.Length > 0 ) route += " -> ";
                route += _solarSystems.SolarSystems[systemId].Name;
            }

            return route;
        }
        private string generateAlertText(LogEntry entry, PathInfo path, AlertConfig alert, string characterName)
        {
            string returnString = alert.TextDisplay;
            returnString = returnString.Replace("{jumps}", path.TotalJumps.ToString());
            returnString = returnString.Replace("{character}", characterName);
            returnString = returnString.Replace("{tosystem}", _solarSystems.SolarSystems[path.ToSystemId].Name);
            returnString = returnString.Replace("{fromsystem}", _solarSystems.SolarSystems[path.FromSystemId].Name);
            returnString = returnString.Replace("{text}", entry.LineContent);
            returnString = returnString.Replace("{reporter}", entry.PlayerName);
            returnString = returnString.Replace("{route}", PathToString(path));
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
                if (alert.Path.TotalJumps < f.First().Path.TotalJumps)
                {
                    f.First().Path = alert.Path;
                    f.First().AlertText = alert.AlertText;
                    f.First().Entry= alert.Entry;
                    f.First().Context = alert.Context;
                    f.First().RouteText = alert.RouteText;
                    f.First().SoundFile= alert.SoundFile;
                    f.First().Context += "+++";
                }
            }
        }
        private void logFile_ProcessNewData(object sender, ProcessNewDataEventArgs e)
        {
            LogEntry entry = e.LogEntry;
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
                        .Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => ! Configuration.ignoreSystems.Contains(this._solarSystems.Names[tempSystem.Name])))
                        )
                        intSet.Add(_solarSystems.Names[solarSystem.Name]);
                }

                if (intSet.Count > 0)
                {
                    Log.Information(entry.LineContent);
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
                            if (!tempTrigger.Enabled)
                                return false;
                            return tempTrigger.AlertType == AlertConfigType.Range;
                        })))
                        {
                            bool sendAlert = false;
                            PathInfo path = new PathInfo();
                            string alertText = "";

                            switch ( alert.AlertRangeType )
                            {
                                case AlertConfigRangeType.Home:
                                     path = homeSystemPathInfo;
                                    sendAlert = checkJumpsRange(alert,path);
                                    if ( sendAlert )
                                    {
                                        alertText = generateAlertText(entry, path, alert, "");
                                        Log.Debug("ALERT - " + alertText);
                                        EnqueueAlert(
                                            new AlertQueueItem()
                                            {
                                                AlertText = alertText,
                                                Entry = entry,
                                                Path = path,
                                                Context = homeSystemPathInfo.FromSystemName,
                                                SoundFile = alert.SoundFile,
                                            }
                                            );
                                    }
                                    break;

                                case AlertConfigRangeType.System:
                                    // check against specific system
                                    path = _solarSystems.FindAndCachePath(_solarSystems.Names[alert.TriggerText], toSystemId);
                                    sendAlert = checkJumpsRange(alert, path);
                                    if (sendAlert)
                                    {
                                        alertText = generateAlertText(entry, path, alert, "");
                                        Log.Debug("ALERT - " + alertText);
                                        EnqueueAlert(
                                            new AlertQueueItem()
                                            {
                                                AlertText = alertText,
                                                Entry = entry,
                                                Path = path,
                                                Context = path.FromSystemName,
                                                SoundFile = alert.SoundFile,
                                            }
                                            );
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
                                            alertText = generateAlertText(entry, path, alert, x.CharacterName);
                                            Log.Debug("ALERT - " + alertText);
                                            EnqueueAlert(
                                                new AlertQueueItem()
                                                {
                                                    AlertText = alertText,
                                                    Entry = entry,
                                                    Path = path,
                                                    Context = x.CharacterName,
                                                    SoundFile = alert.SoundFile,
                                                }
                                                );
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
                                            alertText = generateAlertText(entry, path, alert, x.CharacterName);
                                            Log.Debug("ALERT - " + alertText);
                                            EnqueueAlert(
                                                new AlertQueueItem()
                                                {
                                                    AlertText = alertText,
                                                    Entry = entry,
                                                    Path = path,
                                                    Context = x.CharacterName,
                                                    SoundFile = alert.SoundFile,
                                                }
                                                );
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
                entry.MatchedIds = intSet;
                this._newIntelQueue.Enqueue(entry);
            }
        }

        private void logFile_ProcessCombat(object sender, ProcessCombatEventArgs e)
        {
            /*
                        if (e.CombatEvent == CombatEventType.Start)
                        {
                            foreach (ToolStripItem toolStripItem in this.anomalyMonitorMenuItem.DropDownItems.Cast<ToolStripMenuItem>().Where<ToolStripMenuItem>((Func<ToolStripMenuItem, bool>)(dropDownItem => dropDownItem.Text == e.CharacterName && dropDownItem.Checked)))
                                this.WriteIntel("sys", " > Anomaly Monitor | Combat started: " + toolStripItem.Text);
                        }
                        else
                        {
                            if (e.CombatEvent != CombatEventType.Stop)
                                return;
                            foreach (ToolStripItem toolStripItem in this.anomalyMonitorMenuItem.DropDownItems.Cast<ToolStripMenuItem>().Where<ToolStripMenuItem>((Func<ToolStripMenuItem, bool>)(dropDownItem => dropDownItem.Text == e.CharacterName && dropDownItem.Checked)))
                            {
                                this.WriteIntel("sys", " > Anomaly Monitor | Combat finished: " + toolStripItem.Text);
                                this.PlayAnomalyWatcherSound();
                            }
                        }
            */
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

        private void trayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
    }
}
