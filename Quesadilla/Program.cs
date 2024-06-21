using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
using Quesadilla.Classes.SolarSystems;
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
        public static UniverseManager Universe = new UniverseManager();

        public const string CHARACTER_CACHE_FILE = "Quesadilla.CharacterCache.Json";

        [STAThread]
        static void Main()
        {
            ConfigureSerilog();

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
