using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Queues;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quesadilla
{
    public partial class QuesadillaAlertPopup : Form
    {
        private AlertQueueItem _alert;
        private int _seconds;

        private int _opacity;
        private int _opacitystep;
        private PopupLocationType _location;

        private const string ALERT_TEXT = "Alert";

        public QuesadillaAlertPopup(AlertQueueItem alert, Config configuration, Image background)
        {
            InitializeComponent();
            _alert = alert;
            _seconds = configuration.AlertPopupSeconds;
            _opacity = configuration.AlertPopupOpacity;
            _opacitystep = configuration.AlertPopupOpacityStep;

            Opacity = 0;

            AlertText.ForeColor = configuration.AlertPopupAlertTextColour;
            AlertText.Text = _alert.AlertText;

            SystemName.ForeColor = configuration.AlertPopupSystemNameColour;
            if (alert.Path != null)
            {
                SystemName.Text = _alert.Path.ToSystemName;
            }
            else
            {
                SystemName.Text = ALERT_TEXT;
            }

            RegionName.ForeColor = configuration.AlertPopupRegionNameColour;
            if (alert.Path != null)
            {
                RegionName.Text = _alert.Path.ToSystemRegionName;
            }
            else
            {
                RegionName.Text = "";
            }

            ContextInfo.ForeColor = configuration.AlertPopupContextInfoColour;
            if (alert.Path != null)
            {
                ContextInfo.Text = _alert.Path.TotalJumps.ToString() + " jump" + (_alert.Path.TotalJumps == 1 ? "" : "s") + "-> " + _alert.Context;
            }
            else
            {
                ContextInfo.Text = alert.FriendlyName;
            }

            IntelLine.ForeColor = configuration.AlertPopupIntelLineColour;
            IntelLine.Text = _alert.Entry.LineContent;

            BackgroundImage = background;

            // position on screen
            Screen screen = Screen.FromControl(this);
            switch (configuration.AlertPopupLocation)
            {
                case PopupLocationType.TopLeft:
                    Location = new Point()
                    {
                        X = screen.WorkingArea.Left,
                        Y = screen.WorkingArea.Top,
                    };
                    break;
                case PopupLocationType.TopRight:
                    Location = new Point()
                    {
                        X = screen.WorkingArea.Right - Width,
                        Y = screen.WorkingArea.Top,
                    };
                    break;
                case PopupLocationType.BottomLeft:
                    Location = new Point()
                    {
                        X = screen.WorkingArea.Left,
                        Y = screen.WorkingArea.Bottom - Height,
                    };
                    break;
                case PopupLocationType.BottomRight:
                    Location = new Point()
                    {
                        X = screen.WorkingArea.Right - Width,
                        Y = screen.WorkingArea.Bottom - Height,
                    };
                    break;
                case PopupLocationType.Center:
                default:
                    Location = new Point()
                    {
                        X = (screen.WorkingArea.Right/2) - (Width/2),
                        Y = (screen.WorkingArea.Bottom/2) - (Height/2),
                    };
                    break;
            }
        }

        private void QuesadillaAlertPopup_Load(object sender, EventArgs e)
        {
            Visible = true;
            Opacity = 0;
            timerAlertWindow.Interval = _seconds*1000;
            timerAlertWindow.Enabled = true;
            timerOpacityOpen.Enabled = true;
            ShowInTaskbar = false;
        }

        private void CloseWindow()
        {
            timerOpacityClose.Enabled = true;
        }

        private void timerAlertWindow_Tick(object sender, EventArgs e)
        {
            CloseWindow();
        }

        private void timerOpacityOpen_Tick(object sender, EventArgs e)
        {
            Opacity += (double)_opacitystep/100;
            if (Opacity >= (double)_opacity/100)
            {
                timerOpacityOpen.Enabled = false;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void QuesadillaAlertPopup_Click(object sender, EventArgs e)
        {
            Opacity = 0;
            CloseWindow();
        }

        private void timerOpacityClose_Tick(object sender, EventArgs e)
        {
            Opacity -= (double)_opacitystep / 100;
            if (Opacity <= 0)
            {
                Close();
            }
        }

        private void SystemName_Click(object sender, EventArgs e)
        {
            if ( SystemName.Text != ALERT_TEXT )
            {
                System.Diagnostics.Process.Start("https://evemaps.dotlan.net/map/" + RegionName.Text.Replace(" ","_") + "/" + SystemName.Text);
            }
        }
    }
}
