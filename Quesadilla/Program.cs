using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
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

        public const string configuration_filename = "Quesadilla.config.json";
        public const string CHARACTER_CACHE_FILE = "Quesadilla.CharacterCache.Json";

        [STAThread]
        static void Main()
        {
            ConfigureSerilog();

            MainConfiguration = Config.Load(configuration_filename);

            Universe.ReadSolarSystems("Quesadilla.Systems.Json");
            Universe.ReadItemTypes("Quesadilla.ItemTypes.Json");
            Universe.ReadESICharacters(CHARACTER_CACHE_FILE);
            Universe.SetupESI();

            var webhost = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                //                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:65432") // <-----
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
