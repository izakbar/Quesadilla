using System;
using System.Windows.Forms;
using Serilog;

namespace Quesadilla
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigureSerilog();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Quesadilla());
        }

        private static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.File("Quesadilla_.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}
