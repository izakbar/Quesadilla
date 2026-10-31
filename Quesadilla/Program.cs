using EVEStandard;
using EVEStandard.Enumerations;
using Microsoft.AspNetCore.Hosting;
using Quesadilla.Classes;
using Quesadilla.Classes.Alerts;
using Quesadilla.Classes.Anoikis;
using Quesadilla.Classes.Characters;
using Quesadilla.Classes.ESIData;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.GraphData;
using Quesadilla.Classes.ImageGeneration;
using Quesadilla.Classes.InterOp;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.Processes;
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
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.WebPages;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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
        public static ProcessMonitor ProcessMonitorHandler = new ProcessMonitor();
		public static CharacterManager CharacterManager = new CharacterManager();
        public static GraphDataManager GraphDataManager = new GraphDataManager();
        public static AnoikisManager AnoikisProcessor;

        public static Version version;
        public static String UserAgent;
        public static Image CachedBackground;

		private static Mutex _mutex;
		private static string MUTEX_NAME = "Quesadilla Single Instance Mutex";
		public const string RESTORE_INSTANCE_MESSAGE_NAME = "Quesadilla Restore Instance";

		[STAThread]
        static void Main()
        {
			Program._mutex = Program.GetMutex();
			if (Program._mutex == null)
			{
				uint message = User32NativeMethods.RegisterWindowMessage(Program.RESTORE_INSTANCE_MESSAGE_NAME);
				if (message != 0)
				{
					User32NativeMethods.PostMessage(User32NativeMethods.HWND_BROADCAST, message, IntPtr.Zero, IntPtr.Zero);
				}
				return;
			}

			QuesadillaPaths.Initialise();
            ConfigureSerilog();

            MainConfiguration = Config.Load(QuesadillaPaths.ConfigurationFile);

            try { 
				System.Deployment.Application.ApplicationDeployment ad = System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
                if (ad != null) {
                    version = ad.CurrentVersion;
                }
			}
            catch
            {
                version = Assembly.GetEntryAssembly().GetName().Version;
			}
			UserAgent = $"Mozilla/5.0 (Quesadilla version {version.ToString()} )";

			esiClient = new EVEStandardAPI(
                    "Quesadilla",                  // User agent
                    DataSource.Tranquility,         // Server [Tranquility/Singularity]
					CompatibilityDate.v2026_06_09,
					TimeSpan.FromSeconds(30));       // Timeout


            ESIHandler = new ESIWorker(esiClient, Log.Logger);
            Universe = new UniverseManager(esiClient, Log.Logger);

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

			DateTime end = DateTime.Now.AddSeconds(2);

			while (DateTime.Now < end)
			{
				Application.DoEvents();
				System.Threading.Thread.Sleep(50);
			}

			splash.SetStatus("Loading Universe...");
			Application.DoEvents();
			Universe.ReadSolarSystems("Quesadilla.Systems.Json");

			splash.SetStatus("Loading Item Types...");
			Application.DoEvents();
			Universe.ReadItemTypes("Quesadilla.ItemTypes.Json");

			splash.SetStatus("Loading Character Cache...");
			Application.DoEvents();
			Universe.ReadESICharacters(QuesadillaPaths.CharacterCacheFile);

			splash.SetStatus("Creating Logfile Watchers...");
			Application.DoEvents();
			LogWatcherManager = new LogWatcherManager(MainConfiguration.HowManyTicksBeforeLogfileCheck * MainConfiguration.TickMiliseconds);
			Program.SetupLogfileWatchers();

			splash.SetStatus("Creating Main Window...");
			Application.DoEvents();
			q = new Quesadilla();
			q.SplashScreen = splash;
            q.ReloadConfiguration();
			ESIHandler.ESIEventHandler += new EventHandler<ESIEventArgs>(q.ESI_ProcessNewData);
			ESIHandler.EnqueueRequestIncursions();

			splash.SetStatus("Setting up EveScout...");
			Application.DoEvents();
			EveScoutProcessor = new EveScoutManager(Log.Logger);
			EveScoutProcessor.EveScoutEventHandler += new EventHandler<EveScoutEventArgs>(q.EveScout_ProcessNewData);

			splash.SetStatus("Setting up Anoikis...");
			Application.DoEvents();
			AnoikisProcessor = new AnoikisManager();

			splash.SetStatus("Creating Zkill Handler");
			Application.DoEvents();
			ZKillHandler = new ZKill();
			ZKillHandler.ZKillFeedR2Z2.ZKillEventHandler += new EventHandler<ZKillEventArgs>(q.ZKill_ProcessNewData);

			splash.SetStatus("Creating Alert Queue Manager...");
			Application.DoEvents();
			AlertQueueManager = new AlertQueueManager();


			splash.SetStatus("Starting Logwatcher.");
			Application.DoEvents();
			LogWatcherManager.Start();

			splash.SetStatus("Starting Tickmangers");
			Application.DoEvents();
			q.TickManagerUpdates.Start();
			q.TickManagerUI.Start();

			splash.SetStatus("Done.");
			Application.DoEvents();
			System.Threading.Thread.Sleep(500);

			Application.Run(q);
			MainConfiguration.Save(QuesadillaPaths.ConfigurationFile);
            GDICache.Clear();
        }

		private static Mutex GetMutex()
		{
			try
			{
				Mutex.OpenExisting(Program.MUTEX_NAME);
				return null;
			}
			catch (UnauthorizedAccessException)
			{
				return null;
			}
			catch (Exception)
			{
				Mutex m = new Mutex(true, Program.MUTEX_NAME, out var result);
				return result ? m : null;
			}
		}

		public static void SaveAndReloadConfiguration(Config c)
        {
            // Store items into config
            Program.MainConfiguration = c;
            c.Save(QuesadillaPaths.ConfigurationFile);
            q.ReloadConfiguration();
        }

        public static void SetupLogfileWatchers()
        {

            // load from config
            foreach(var l in MainConfiguration.Logfiles.Where(o => o.Enabled == true))
            {
                LogWatcherManager.AddWatcher(l.Logfile, l.LogType, l.Name);
            }

			// always add gamelogs and local !
			LogWatcherManager.AddWatcher(null, Classes.Logfile.LogFileType.Local, "Local chat");
			LogWatcherManager.AddWatcher(null, Classes.Logfile.LogFileType.Game, "Gamelogs");
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

            if (File.Exists(Path.Combine(QuesadillaPaths.SoundDirectory, fileName)))
            {
                using (FileStream fileStream = File.Open(Path.Combine(QuesadillaPaths.SoundDirectory, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

            if (File.Exists(Path.Combine(QuesadillaPaths.ImageDirectory, fileName)))
            {
                Image imageFile = Image.FromFile(Path.Combine(QuesadillaPaths.ImageDirectory, fileName));
                return imageFile;
            }
            return (Image)null;
        }
        private static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.File(QuesadillaPaths.LogFile, rollingInterval: RollingInterval.Day)
                .WriteToSimpleAndRichTextBox(new MessageTemplateTextFormatter("{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
            .CreateLogger();
        }
	}
}
