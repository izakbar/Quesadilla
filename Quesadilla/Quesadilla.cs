using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.SolarSystems;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quesadilla
{
    public partial class Quesadilla : Form
    {
        private Queue<LogEntry> _newIntelQueue = new Queue<LogEntry>();
        private SolarSystemManager _solarSystems = new SolarSystemManager();
        private Config Configuration = new Config();
        private List<LogWatcher> watchers = new List<LogWatcher>();

        private string configuration_filename = "Quesadilla.config.json";

        // TODO - config things - really need to go into config structure later
        public List<Character> CharacterList { get; set; }

        public Quesadilla()
        {
            InitializeComponent();

            Configuration = Config.Load(configuration_filename);

            // stop main tick timer!
            mainTimer.Enabled = false;

            CharacterList = new List<Character>();
            _solarSystems.ReadSolarSystems("Quesadilla.Systems.Json");

            _solarSystems.HomeSolarSystemID = _solarSystems.Names[Configuration.HomeSystem];

            watchers.Add(new LogWatcher("izakbar", Classes.Logfile.LogFileType.Chat));

            watchers.Add(new LogWatcher("querious", Classes.Logfile.LogFileType.Chat));
            watchers.Add(new LogWatcher("delve", Classes.Logfile.LogFileType.Chat));
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

        private void Quesadilla_Load(object sender, EventArgs e)
        {
            if (Configuration.InitialPosition != null)
            {
                Location = Configuration.InitialPosition;
            }
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            foreach (LogWatcher watcher in watchers)
            {
                watcher.Tick();
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

            shouldAlert = checkJumpsRangeComparison(path.TotalJumps, alert.Jumps1Comparison, alert.Jumps1Number) && checkJumpsRangeComparison(path.TotalJumps, alert.Jumps2Comparison, alert.Jumps2Number);

            return shouldAlert;
        }
        private string generateAlertText(PathInfo path, AlertConfig alert, string characterName)
        {
            string returnString = alert.TextDisplay;
            returnString = returnString.Replace("{jumps}", path.TotalJumps.ToString());
            returnString = returnString.Replace("{character}", characterName);
            returnString = returnString.Replace("{tosystem}", _solarSystems.SolarSystems[path.ToSystem].Name);
            returnString = returnString.Replace("{fromsystem}", _solarSystems.SolarSystems[path.FromSystem].Name);
            return returnString;
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
                if (!Configuration.ignoreStrings.Any<Regex>((Func<Regex, bool>)(ignoreString => ignoreString.IsMatch(entry.LineContent))) && entry.LogType == LogFileType.Chat)
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
                        Log.Information("Jumps from HOME:" + _solarSystems.SolarSystems[homeSystemId].Name + " to " + _solarSystems.SolarSystems[toSystemId].Name + " is " + homeSystemPathInfo.TotalJumps);
                        foreach (var c in CharacterList.Where(o => o.LocationId > 0))
                        {
                            var CurrectLocationPathInfo = _solarSystems.FindAndCachePath(c.LocationId, toSystemId);
                            Log.Information("Jumps from " + c.CharacterName + ":" + _solarSystems.SolarSystems[c.LocationId].Name + " to " + _solarSystems.SolarSystems[toSystemId].Name + " is " + CurrectLocationPathInfo.TotalJumps);
                        }

                        /*
                                                                foreach (AlertTrigger alertTrigger in this._alertTriggers.Where<AlertTrigger>((Func<AlertTrigger, bool>)(tempTrigger =>
                                                                {
                                                                    if (!tempTrigger.Enabled)
                                                                        return false;
                                                                    return tempTrigger.RangeTo == RangeAlertType.System || tempTrigger.RangeTo == RangeAlertType.Character;
                                                                })))
                                                                {
                                                                    if (alertTrigger.RangeTo == RangeAlertType.System)
                                                                    {
                                                                        int systemId = alertTrigger.SystemId;
                                                                        if (!this._solarSystems.PathCache.ContainsKey(this._solarSystems.GenerateUniquePathId(systemId, toSystemId)))
                                                                            this._solarSystems.FindAndCachePath(systemId, toSystemId);
                                                                    }
                                                                }
                        */

                        // Range alerts
                        foreach (AlertConfig alert in Configuration.AlertConfiguration.Where<AlertConfig>((Func<AlertConfig, bool>)(tempTrigger =>
                        {
                            if (!tempTrigger.Enabled)
                                return false;
                            return tempTrigger.AlertType == AlertConfigType.Range;
                        })))
                        {
                            Log.Debug("trigger");
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
                                        alertText = generateAlertText(path, alert, "");
                                        Log.Error("ALERT - " + alertText);
                                    }
                                    break;

                                case AlertConfigRangeType.System:
                                    // check against specific system
                                    path = _solarSystems.FindAndCachePath(_solarSystems.Names[alert.TriggerText], toSystemId);
                                    sendAlert = checkJumpsRange(alert, path);
                                    if (sendAlert)
                                    {
                                        alertText = generateAlertText(path, alert, "");
                                        Log.Error("ALERT - " + alertText);
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
                                            alertText = generateAlertText(path, alert, x.CharacterName);
                                            Log.Error("ALERT - " + alertText);
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
                                            alertText = generateAlertText(path, alert, x.CharacterName);
                                            Log.Error("ALERT - " + alertText);
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
            Configuration.Save(configuration_filename);
        }
    }
}
