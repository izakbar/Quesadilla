using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quesadilla
{
    public partial class QuesadillaSplashScreen : Form
    {

        private int _opacitystep = 5;
        private int _opacity = 100;

        public QuesadillaSplashScreen()
        {
            InitializeComponent();
        }

        private void izakbar_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://evewho.com/character/241078583");
        }

        private void corp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://sh0ktherapy.space/");
        }

        private void alliance_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.bravecollective.com/");
        }

        private void timerOpen_Tick(object sender, EventArgs e)
        {
            if (Opacity <= (double)_opacity / 100)
            {
                Opacity += (double)_opacitystep / 100;
                //timerOpen.Enabled = false;
                //Thread.Sleep(50);
            }
            Show();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void QuesadillaSplashScreen_Load(object sender, EventArgs e)
        {
            Screen screen = Screen.FromControl(this);
            Left = (screen.WorkingArea.Width / 2) - (Width / 2);
            Top = (screen.WorkingArea.Height / 2) - (Height / 2);
            ShowInTaskbar = false;

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            VersionNumber.Text = version.ToString();
        }
    }
}
