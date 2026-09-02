using EVEStandard;
using EVEStandard.Enumerations;
using EVEStandard.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Alerts;
using Quesadilla.Classes.ESIData;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.ImageGeneration;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.Processes;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.Resources;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Classes.ZKill;
using Serilog;
using Serilog.Formatting.Display;
using Serilog.Sinks.WinForms.Base;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Web.WebPages;
using System.Windows.Forms;

namespace Quesadilla
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        public static Quesadilla q = null;

        private static EVEStandardAPI esiClient;

        public static ZKill ZKillHandler;
        public static Config MainConfiguration = new Config();
        public static ESIWorker ESIHandler;
		public static EveScoutManager EveScoutProcessor;
		public static UniverseManager Universe;

        public static List<QuesadillaSound> Sounds = new List<QuesadillaSound>();
        public static List<QuesadillaBackground> Backgrounds = new List<QuesadillaBackground>();

        public static AlertQueueManager AlertQueueManager;

		public static LogWatcherManager LogWatcherManager;
		public static List<Character> CharacterList { get; set; }

        public static ProcessMonitor ProcessMonitorHandler = new ProcessMonitor();

        public static ConcurrentDictionary<string, List<CombatLine>> Combat = new ConcurrentDictionary<string, List<CombatLine>>();
        public static ConcurrentDictionary<string, List<IskLine>> ISK = new ConcurrentDictionary<string, List<IskLine>>();
        public static ConcurrentDictionary<string, List<MiningLine>> Mining = new ConcurrentDictionary<string, List<MiningLine>>();

        public const string configuration_filename = "Quesadilla.config.json";
        public const string CHARACTER_CACHE_FILE = "Quesadilla.CharacterCache.Json";

        public static Version version = Assembly.GetEntryAssembly().GetName().Version;
        public static String UserAgent = $"Mozilla/5.0 (Quesadilla version {version.ToString()} )";

		[STAThread]
        static void Main()
        {
            ConfigureSerilog();

            MainConfiguration = Config.Load(configuration_filename);

			esiClient = new EVEStandardAPI(
                    "Quesadilla",                  // User agent
                    DataSource.Tranquility,         // Server [Tranquility/Singularity]
					CompatibilityDate.v2026_06_09,
					TimeSpan.FromSeconds(30));       // Timeout


            ESIHandler = new ESIWorker(esiClient, Log.Logger);
            Universe = new UniverseManager(esiClient, Log.Logger);
			EveScoutProcessor = new EveScoutManager(Log.Logger);

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

			var splash = new QuesadillaSplashScreen();
			splash.Show();

			splash.Show();

			DateTime end = DateTime.Now.AddSeconds(2);

			while (DateTime.Now < end)
			{
				Application.DoEvents();
				System.Threading.Thread.Sleep(50);
			}

			splash.SetStatus("Loading Universe...");
			Universe.ReadSolarSystems("Quesadilla.Systems.Json");
			Application.DoEvents();

			splash.SetStatus("Loading Item Types...");
			Universe.ReadItemTypes("Quesadilla.ItemTypes.Json");
			Application.DoEvents();

			splash.SetStatus("Loading Character Cache...");
			Universe.ReadESICharacters(CHARACTER_CACHE_FILE);
			Application.DoEvents();

			splash.SetStatus("Creating Zkill Handler");
			ZKillHandler = new ZKill();
			Application.DoEvents();


			splash.SetStatus("Creating Logfile Watchers...");
			Application.DoEvents();
			LogWatcherManager = new LogWatcherManager(MainConfiguration.HowManyTicksBeforeLogfileCheck * MainConfiguration.TickMiliseconds);
			Program.SetupLogfileWatchers();

			splash.SetStatus("Creating Alert Queue Manager...");
			Application.DoEvents();
			AlertQueueManager = new AlertQueueManager();

			splash.SetStatus("Creating Main Window...");
			Application.DoEvents();
			q = new Quesadilla();
			q.SplashScreen = splash;

			splash.SetStatus("Done.");
			Application.DoEvents();

			Application.Run(q);
			MainConfiguration.Save(configuration_filename);
            GDICache.Clear();
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
                LogWatcherManager.AddWatcher(l.Logfile, l.LogType, l.FriendlyName);
            }

			// always add gamelogs and local !
			LogWatcherManager.AddWatcher(null, Classes.Logfile.LogFileType.Local, "Local chat");
			LogWatcherManager.AddWatcher(null, Classes.Logfile.LogFileType.Game, "Gamelogs");
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
        public static string GetMiningLastHour(string toon)
        {
            var ret = "";
            double units = 0;
            double units3Wasted = 0;

            //var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
            var expireTime = DateTime.Now.AddSeconds(-3600);

            foreach (var t in CharacterList)
            {
                if (!toon.IsEmpty() && t.CharacterName != toon || !Mining.ContainsKey(t.CharacterName)) continue;
                foreach (var m in Mining[t.CharacterName].Where(o => o.TimeAdded >= expireTime))
                {
                    units += m.MiningAmount;
                    units3Wasted += m.WastageAmount;
                }
            }

            if (units > 0)
            {
                ret = ((int)Math.Truncate(units)).ToString("N0")+" - " + ((int)Math.Truncate(units3Wasted)).ToString("N0")+" ";
            }

            return ret;
        }
        public static Dictionary<string, List<MiningDetails>> GetMiningLastHourDetail(string toon, bool itemTotalsOnly)
        {
        Dictionary<string, List<MiningDetails>> miningDetails = new Dictionary<string, List<MiningDetails>>();

        //var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
        var expireTime = DateTime.Now.AddSeconds(-3600);

            foreach (var t in CharacterList)
            {
                if (!toon.IsEmpty() && t.CharacterName != toon || !Mining.ContainsKey(t.CharacterName)) continue;
                if (!miningDetails.ContainsKey(t.CharacterName))
                {
                    miningDetails[t.CharacterName] = new List<MiningDetails>();
                }

                foreach (var m in Mining[t.CharacterName].Where(o => o.TimeAdded >= expireTime))
                {
                    MiningDetails md = miningDetails[t.CharacterName].Find(o => o.MiningItem == m.MiningItem);
                    if (md == null)
                    {
                        md = new MiningDetails()
                        {
                            MiningItem = m.MiningItem,
                            MiningAmount = m.MiningAmount,
                            WastageAmount = m.WastageAmount,
                        };
                        miningDetails[t.CharacterName].Add(md);
                    } else
                    {
                        md.MiningAmount += m.MiningAmount;
                        md.WastageAmount += m.WastageAmount;
                    }
                }
            }

            if ( itemTotalsOnly)
            {
                return MiningItemSummary(miningDetails);
            }
            else
            {
                return miningDetails;
            }
        }

        public static Dictionary<string, List<MiningDetails>> MiningItemSummary(Dictionary<string, List<MiningDetails>> miningDetail)
        {
            Dictionary<string, List<MiningDetails>> miningSummary = new Dictionary<string, List<MiningDetails>>();

            if (miningDetail.Count == 0) return miningSummary;

            miningSummary["Summary"] = new List<MiningDetails>();
            foreach (var detail in miningDetail)
            {
                foreach (var detailItem in detail.Value)
                {
                    MiningDetails miningSummaryItem = miningSummary["Summary"].Find(o => o.MiningItem == detailItem.MiningItem);
                    if (miningSummaryItem == null)
                    {
                        miningSummaryItem= new MiningDetails()
                        {
                            MiningItem = detailItem.MiningItem,
                            MiningAmount = detailItem.MiningAmount,
                            WastageAmount = detailItem.WastageAmount,
                        };
                        miningSummary["Summary"].Add(miningSummaryItem);
                    }
                    else
                    {
                        miningSummaryItem.MiningAmount += detailItem.MiningAmount;
                        miningSummaryItem.WastageAmount += detailItem.WastageAmount;
                    }
                }
            }
            return miningSummary;
        }

        public static void ExpireMining()
        {
            // expire ISK ?
            foreach (var k in Mining.Keys)
            {
                bool removedEntry = true;
                while (removedEntry && Mining[k].Count > 0)
                {
                    removedEntry = false;
                    lock (Mining[k])
                    {
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
                bool removedEntry = true;
                while (removedEntry && ISK[k].Count > 0)
                {
                    removedEntry = false;
                    lock (ISK[k])
                    {
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
                while (removedEntry && Combat[k].Any())
                {
                    removedEntry = false;
                    lock (Combat[k])
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
