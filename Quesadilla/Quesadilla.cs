using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quesadilla.Classes;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.SolarSystems;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private List<LogWatcher> watchers = new List<LogWatcher>();

        // TODO - config things - really need to go into config structure later
        public List<Character> CharacterList { get; set; }

        private List<Regex> _ignoreStrings = new List<Regex>();
        private List<int> _ignoreSystems = new List<int>();

        public static IConfiguration Configuration { get; set; }
        private static void SetupConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("Quesadilla.config.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            // check for some configuration - if not found then setup defaults.
            if (string.IsNullOrEmpty(Configuration["HomeSystem"]))
            {
                Configuration["HomeSystem"] = "K7D-II";
            }

        }

        public Quesadilla()
        {
            InitializeComponent();
            SetupConfiguration();

            // stop main tick timer!
            mainTimer.Enabled = false;

            CharacterList = new List<Character>();
            _solarSystems.ReadSolarSystems("Quesadilla.Systems.Json");
            _solarSystems.HomeSolarSystemID = _solarSystems.Names[Configuration["HomeSystem"]];

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
            Log.Information("sys > New character found: " + characterName);
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
                        Log.Information(e.LogPrefix + " > Chat log opened: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.NewChatLogEvent:
                        //if (!this._conf.DisplayNewFileAlerts) break;
                        Log.Information(e.LogPrefix + " > New chat log detected: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.OpenGameLogEvent:
                        //if (!this._conf.DisplayOpenFileAlerts) break;
                        Log.Information(e.LogPrefix + " > Game log opened: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.NewGameLogEvent:
                        //if (!this._conf.DisplayNewFileAlerts) break;
                        Log.Information(e.LogPrefix + " > New game log detected: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                }
            }
            else
            {
                if (!entry.ParseSuccess)
                    return;
                HashSet<int> intSet = new HashSet<int>();
                if (!this._ignoreStrings.Any<Regex>((Func<Regex, bool>)(ignoreString => ignoreString.IsMatch(entry.LineContent))) && entry.LogType == LogFileType.Chat)
                {
                    // try and find systems mentioned in intel
                    foreach (SolarSystem solarSystem in
                        _solarSystems.SolarSystems.Values.Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => tempSystem.MatchNameRegex(entry.LineContent)))
                        .Where<SolarSystem>((Func<SolarSystem, bool>)(tempSystem => !this._ignoreSystems.Contains(this._solarSystems.Names[tempSystem.Name])))
                        )
                        intSet.Add(this._solarSystems.Names[solarSystem.Name]);
                }
                foreach (int toSystemId in intSet)
                {
                    int homeSystemId = _solarSystems.HomeSolarSystemID;

                    // Cache paths - if not already stored
                    var homeSystemPathInfo = _solarSystems.FindAndCachePath(homeSystemId, toSystemId);
                    Log.Debug("Jumps from " + _solarSystems.SolarSystems[homeSystemId].Name + " to " + _solarSystems.SolarSystems[toSystemId].Name + " is " + homeSystemPathInfo.TotalJumps );
                    foreach (var c in CharacterList.Where(o => o.LocationId > 0)  )
                    {
                        var CurrectLocationPathInfo = _solarSystems.FindAndCachePath(c.LocationId, toSystemId);
                        Log.Debug("Jumps from " + _solarSystems.SolarSystems[c.LocationId].Name + " to " + _solarSystems.SolarSystems[toSystemId].Name + " is " + CurrectLocationPathInfo.TotalJumps);
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
                }
                if (intSet.Count > 0)
                {
                    int systemId1 = -1;
                    foreach (int systemId2 in intSet)
                    {
//                        this._solarSystems.AddAlert(systemId2);
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
                } else
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
            Log.Information("sys > System Change: " + characterName + " (" + foundSystemName + ")");
        }

        private void Quesadilla_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
