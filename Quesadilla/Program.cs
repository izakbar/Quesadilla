using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Resources;
using System.Web.WebPages;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.Processes;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.Resources;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Classes.ZKill;
using Serilog;
using Serilog.Formatting.Display;
using Serilog.Sinks.WinForms.Base;

namespace Quesadilla
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        public static Quesadilla q = null;

        public static ZKill ZKillHandler = new ZKill();
        public static Config MainConfiguration = new Config();

        public static UniverseManager Universe = new UniverseManager();

        public static List<QuesadillaSound> Sounds = new List<QuesadillaSound>();
        public static List<QuesadillaBackground> Backgrounds = new List<QuesadillaBackground>();

        public static Queue<AlertQueueItem> NewAlerts = new Queue<AlertQueueItem>();
        public static Queue<AlertQueueItem> ActiveAlerts = new Queue<AlertQueueItem>();

        public static  List<LogWatcher> logfileWatchers = new List<LogWatcher>();
        public static List<Character> CharacterList { get; set; }

        public static ProcessMonitor ProcessMonitorHandler = new ProcessMonitor();

        public static Dictionary<string, List<CombatLine>> Combat = new Dictionary<string, List<CombatLine>>();
        public static Dictionary<string, List<IskLine>> ISK = new Dictionary<string, List<IskLine>>();
        public static Dictionary<string, List<MiningLine>> Mining = new Dictionary<string, List<MiningLine>>();

        public const string configuration_filename = "Quesadilla.config.json";
        public const string CHARACTER_CACHE_FILE = "Quesadilla.CharacterCache.Json";

        public static Version version = Assembly.GetEntryAssembly().GetName().Version;

        [STAThread]
        static void Main()
        {
            ConfigureSerilog();

            MainConfiguration = Config.Load(configuration_filename);

            Universe.ReadSolarSystems("Quesadilla.Systems.Json");
            Universe.ReadItemTypes("Quesadilla.ItemTypes.Json");
            Universe.ReadESICharacters(CHARACTER_CACHE_FILE);
            Universe.SetupESI();

            CharacterList = new List<Character>();

            string listeningPort = MainConfiguration.IntegrationPort.ToString();

            var webhost = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                //                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls($"http://localhost:{listeningPort}") // <-----
                ;
            webhost.Build().RunAsync();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(q = new Quesadilla());

            MainConfiguration.Save(configuration_filename);
        }

        public static void SaveAndReloadConfiguration(Config c)
        {
            // Store items into config
            Program.MainConfiguration = c;
            c.Save(configuration_filename);
            q.ReloadConfiguration();
        }

        public static void SetupLogfileWatchers()
        {

            // load from config
            foreach(var l in MainConfiguration.Logfiles.Where(o => o.Enabled == true))
            {
                logfileWatchers.Add(new LogWatcher(l.Logfile, l.LogType, l.FriendlyName));
            }

            // always add gamelogs and local !
            logfileWatchers.Add(new LogWatcher(null, Classes.Logfile.LogFileType.Local, "Local chat"));
            logfileWatchers.Add(new LogWatcher(null, Classes.Logfile.LogFileType.Game, "Gamelogs"));
        }

        public static void StartLogfileWatchers()
        {
            // start watchers
            foreach (LogWatcher watcher in logfileWatchers)
            {
                watcher.StartWatch();
            }
        }

        public static void AddNewCharacter(string characterName)
        {
            if (characterName.Trim().Length == 0)
                return;

            CharacterList.Add(new Character()
            {
                CharacterName = characterName.Trim(),
                IsClientActive = false,
            });
            Log.Information("New character found: " + characterName);
        }
        public static void InitializeResources()
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

        public static int ExpireAlerts()
        {
            int Expired = 0;

            lock (ActiveAlerts)
            {
                bool removedAlert = true;
                while (ActiveAlerts.Count > 0 && removedAlert)
                {
                    var alert = ActiveAlerts.First();
                    DateTime timeAdded = alert.Entry != null ? alert.Entry.TimeAdded : alert.Kill.TimeAdded;
                    var diffInSeconds = (DateTime.Now - timeAdded).TotalSeconds;
                    if (diffInSeconds > Program.MainConfiguration.ExpireAlertSeconds)
                    {

                        alert = Program.ActiveAlerts.Dequeue();
                        Expired++;

                        // expired so dequeue
                        if (!alert.isFaction && !alert.isDecloak && alert.Kill == null && alert.Path != null)
                        {
                            // remove alert to solarsystem info
                            var solarSystem = Universe.SolarSystems[alert.Path.ToSystemId];
                            if (solarSystem.Alerts != null)
                            {
                                var removed = solarSystem.Alerts.Remove(alert);
                            }
                        }
                        removedAlert = true;
                    }
                    else
                    {
                        removedAlert = false;
                    }
                }
            }
            return Expired;
        }

        public static void EnqueueNewAlert(AlertQueueItem alert)
        {
            // only add to new alerts if not already in new alerts. This should prevent an alert for each character on same logfile entry
            var f = NewAlerts.Where(o =>
            (o.Path != null && alert.Path != null && o.Path.ToSystemId == alert.Path.ToSystemId)
            || (o.AlertText == alert.AlertText)
            ).ToList();
            if (f.Count == 0 || NewAlerts.Count == 0)
            {
                alert.RouteText = Program.Universe.PathToString(alert.Path);
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
                    f.First().ShowAlert = alert.ShowAlert;
                }
            }
        }

        public static void EnqueueActiveAlert(AlertQueueItem alert)
        {
            ActiveAlerts.Enqueue(alert);
            if ( alert.Path != null && alert.Path.ToSystemId != null )
            {
                Program.Universe.AddAlert(alert.Path.ToSystemId, alert);
            }
        }

        public static string GetIskLastHour(string toon)
        {
            var ret = "";
            double isk = 0;

            //var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
            var expireTime = DateTime.Now.AddSeconds(-3600);

            foreach (var t in CharacterList)
            {
                if (!toon.IsEmpty() && t.CharacterName != toon || !ISK.ContainsKey(t.CharacterName)) continue;
                foreach (var i in ISK[t.CharacterName].Where(o => o.TimeAdded >= expireTime))
                {
                    isk += i.IskAmount;
                }
            }

            if (isk > 0)
            {
                ret = Math.Truncate(isk).ToString("0,000 ISK");
            }

            return ret;
        }
        public static void ExpireMining()
        {
            // expire ISK ?
            foreach (var k in Mining.Keys)
            {
                if (Mining[k].Count > 0)
                {
                    bool removedEntry = true;
                    while (removedEntry)
                    {
                        removedEntry = false;
                        TimeSpan ts = DateTime.Now - Mining[k].First().TimeAdded;
                        if (ts.TotalSeconds > MainConfiguration.ExpireMiningSeconds)
                        {
                            Mining[k].Remove(Mining[k].First());
                            removedEntry = true;
                        }
                    }
                }
            }
        }
        public static void ExpireISK()
        {
            // expire ISK ?
            foreach (var k in ISK.Keys)
            {
                if (ISK[k].Count > 0)
                {
                    bool removedEntry = true;
                    while (removedEntry)
                    {
                        removedEntry = false;
                        TimeSpan ts = DateTime.Now - ISK[k].First().TimeAdded;
                        if (ts.TotalSeconds > MainConfiguration.ExpireISKSeconds)
                        {
                            ISK[k].Remove(ISK[k].First());
                            removedEntry = true;
                        }
                    }
                }
            }
        }
        public static void ExpireCombat()
        {
            // expire combats ?
            foreach (var k in Combat.Keys)
            {

                bool removedEntry = true;
                while (removedEntry)
                {
                    removedEntry = false;
                    if (Combat[k].Count > 0)
                    {
                        TimeSpan ts = DateTime.Now - Combat[k].First().TimeAdded;
                        if (ts.TotalSeconds > MainConfiguration.ExpireCombatSeconds)
                        {
                            Combat[k].Remove(Combat[k].First());
                            removedEntry = true;
                        }
                    }
                }
            }
        }
        public static SoundPlayer LoadSoundFromFile(string fileName)
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
        public static Image LoadImageFromFile(string fileName)
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

        public static void LogfileWatchersProcessing()
        {
            // progress logfiles
            foreach (LogWatcher watcher in logfileWatchers)
            {
                watcher.Tick();
            }

        }
        private static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.File("Quesadilla_.log", rollingInterval: RollingInterval.Day)
                .WriteToSimpleAndRichTextBox(new MessageTemplateTextFormatter("{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message} {Exception}\n"))
                .CreateLogger();
        }
    }
}
