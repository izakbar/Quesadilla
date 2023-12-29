using Quesadilla.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quesadilla
{
    public partial class Quesadilla : Form
    {
        

        private List<LogWatcher> watchers = new List<LogWatcher>();
        public Quesadilla()
        {
            InitializeComponent();

            watchers.Add(new LogWatcher("querious", Models.Logfile.LogFileType.Chat));
            watchers.Add(new LogWatcher("delve", Models.Logfile.LogFileType.Chat));

            foreach (LogWatcher watcher in watchers)
            {
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
    }
}
