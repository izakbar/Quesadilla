using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
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
        public static Queue<ZKillKill> OBSZKills = new Queue<ZKillKill>();
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


        public static void EnqueueZKill(ZKillKill kill)
        {
            // only add to new zkill list if not already in list. This should prevent multiple entries for same thing
            var f = Program.OBSZKills.Where(o => o.ID == kill.ID).ToList();
            if (f.Count == 0 || q.NewAlerts.Count == 0)
            {
                OBSZKills.Enqueue(kill);
            }
        }

        public static void ExpireOBSZKillQueue()
        {
            // expire zkills (OBS OPage)?
            lock (Program.OBSZKills)
            {
                bool removedOBSZkill = true;
                while (Program.OBSZKills.Count > 0 && removedOBSZkill)
                {
                    var k = Program.OBSZKills.First();
                    TimeSpan ts = DateTime.Now - k.TimeAdded;
                    if (ts.TotalSeconds > q.MainConfiguration.ExpireZKillSeconds)
                    {
                        k.Expired = true;
                        Program.OBSZKills.Dequeue();
                        removedOBSZkill = true;
                    }
                    else
                    {
                        removedOBSZkill = false;
                    }
                }
            }

        }
    }
}
