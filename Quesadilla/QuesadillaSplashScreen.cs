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

		private bool _fadingOut = false;
		private int _opacityStep = 5;
		private int _targetOpacity = 100;

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
            System.Diagnostics.Process.Start("https://auth.insidiousevil.org/");
        }

		private void timerOpen_Tick(object sender, EventArgs e)
		{
			double targetOpacity = _targetOpacity / 100.0;

			if (!_fadingOut)
			{
				// Fade in
				if (Opacity < targetOpacity)
				{
					Opacity += _opacityStep / 100.0;

					if (Opacity > targetOpacity)
						Opacity = targetOpacity;
				}
			}
			else
			{
				// Fade out
				Opacity -= _opacityStep / 100.0;

				if (Opacity <= 0)
				{
					Opacity = 0;
					timerOpen.Stop();
					Close();
				}
			}
		}

		private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

		public void FadeOut()
		{
			_fadingOut = true;

			if (!timerOpen.Enabled)
				timerOpen.Start();
		}

		private void QuesadillaSplashScreen_Load(object sender, EventArgs e)
		{
			Screen screen = Screen.FromControl(this);

			Left = screen.WorkingArea.Left +
				   (screen.WorkingArea.Width - Width) / 2;

			Top = screen.WorkingArea.Top +
				  (screen.WorkingArea.Height - Height) / 2;

			ShowInTaskbar = false;

			Version version = Assembly.GetEntryAssembly().GetName().Version;

			try
			{
				System.Deployment.Application.ApplicationDeployment ad = System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
				if (ad != null)
				{
					version = ad.CurrentVersion;
				}
			}
			catch
			{
				version = Assembly.GetEntryAssembly().GetName().Version;
			}

			VersionNumber.Text = version?.ToString() ?? "Unknown";

			Opacity = 0;

			timerOpen.Start();
		}

		public void SetStatus(string text)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<string>(SetStatus), text);
				return;
			}

			lblStatus.Text = text;
			lblStatus.Refresh();
//			Refresh();
		}
	}
}
