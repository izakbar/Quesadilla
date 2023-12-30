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
        
        public int HomeSolarSystemID = 30004759; // 1DQ
        private List<Regex> _ignoreStrings = new List<Regex>();
        private List<int> _ignoreSystems = new List<int>();

        public Quesadilla()
        {
            InitializeComponent();

            CharacterList = new List<Character>();
            _solarSystems.ReadSolarSystems("Quesadilla.Systems.Json");


            watchers.Add(new LogWatcher("querious", Classes.Logfile.LogFileType.Chat));
            watchers.Add(new LogWatcher("delve", Classes.Logfile.LogFileType.Chat));
            watchers.Add(new LogWatcher(null, Classes.Logfile.LogFileType.Local));

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
                watcher.StartWatch();
            }

        }

        private void Quesadilla_Load(object sender, EventArgs e)
        {

        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            foreach(LogWatcher watcher in watchers)
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
                if (this.CharacterList.Where(o => o.CharacterName == entry.CharacterName).ToList().Count == 0 )
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
                /*
                                    foreach (int num in intSet)
                                    {
                                        int homeSystemId = this._solarSystems.HomeSystemId;
                                        int toSystemId = num;
                                        if (!this._solarSystems.PathCache.ContainsKey(this._solarSystems.GenerateUniquePathId(homeSystemId, toSystemId)))
                                            this._solarSystems.FindAndCachePath(homeSystemId, toSystemId);
                                        foreach (int key in this.BuildCharacterLocationIndex().Keys)
                                        {
                                            if (!this._solarSystems.PathCache.ContainsKey(this._solarSystems.GenerateUniquePathId(key, toSystemId)))
                                                this._solarSystems.FindAndCachePath(key, toSystemId);
                                        }
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
                                    }
                                    if (intSet.Count > 0)
                                    {
                                        int systemId1 = -1;
                                        foreach (int systemId2 in intSet)
                                        {
                                            this._solarSystems.AddAlert(systemId2);
                                            systemId1 = systemId2;
                                        }
                                        if (this.CheckZoomTo(entry.LogPrefix))
                                            this.ZoomTo(systemId1);
                                    }
                                }
                                entry.MatchedIds = intSet;
                                this._newIntelQueue.Enqueue(entry);
                */
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
            int num1 = -1;
            int nativeId;
/*
            if (int.TryParse(e.SystemName, out nativeId))
            {
                using (IEnumerator<KeyValuePair<int, SolarSystem>> enumerator = this._solarSystems.SolarSystems.Select(tempSolarSystem => new
                {
                    tempSolarSystem = tempSolarSystem,
                    tempSystem = tempSolarSystem.Value
                }).Where(_param1 => _param1.tempSystem.NativeId == nativeId).Select(_param1 => _param1.tempSolarSystem).GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        num1 = enumerator.Current.Key;
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
                        num1 = enumerator.Current.Key;
                }
            }
*/
            string key = e.CharName.Trim();
/*
            if (key.Length > 0 && !this._charLocations.ContainsKey(key) && num1 != -1)
            {
                this._charLocations.Add(key, num1);
                if (this._conf.CameraFollowCharacter && this.CentreOnCharacter.Text == key)
                    this.ZoomTo(num1);
                if (!this._conf.CharacterList.Contains(key))
                    this.AddNewCharacter(e.CharName);
            }
            else if (num1 != -1 && key.Length > 0)
            {
                this._charLocations[key] = num1;
                if (this.CameraFollowCharacter.Checked && this.CentreOnCharacter.Text == key)
                    this.ZoomTo(num1);
            }
            string characterName = key.Length == 0 ? "Unknown" : key;
            if (num1 == -1)
                return;
            Log.Information("sys > System Change: " + characterName + " (" + this._solarSystems.SolarSystems[num1].Name + ")", true);
            if (characterName.Length <= 0 || this._conf.MapRangeFrom == MainForm.kHomeIndexMapRange)
                return;
            int num2 = this._conf.CharacterId(characterName);
            if (num2 == -1 || this._conf.MapRangeFrom - 1 != num2)
                return;
            this._solarSystems.Set_MapRangeFrom_SystemID(num1);
*/

        }

    }
}
