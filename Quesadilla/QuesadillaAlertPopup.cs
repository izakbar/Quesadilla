using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Queues;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        private Quesadilla _main;

        public bool IsZkill = false;
        public bool IsAlert = true;

        private const string ALERT_TEXT = "Alert";
        private const string FACTION_TEXT = "Faction Spawn";

        public QuesadillaAlertPopup(AlertQueueItem alert, Config configuration, Image background, Quesadilla mainForm)
        {
            _main = mainForm;
            InitializeComponent();
            _alert = alert;
            _seconds = configuration.AlertPopupSeconds;
            _opacity = configuration.AlertPopupOpacity;
            _opacitystep = configuration.AlertPopupOpacityStep;
            Opacity = 0;

            AlertText.ForeColor = configuration.AlertPopupAlertTextColour;
            AlertText.Text = _alert.AlertText;

            if (alert.Kill != null)
            {
                IsZkill = true;
                IsAlert = false;
            }

            SystemName.ForeColor = configuration.AlertPopupSystemNameColour;
            if (alert.Path != null)
            {
                SystemName.Text = _alert.Path.ToSystemName;
            }
            else
            {
                if (alert.isFaction)
                {
                    SystemName.Text = FACTION_TEXT;
                }
                else
                {
                    SystemName.Text = ALERT_TEXT;
                }
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
            if (alert.Path != null && ! alert.isFaction)
            {
                ContextInfo.Text = _alert.Path.TotalJumps.ToString() + " jump" + (_alert.Path.TotalJumps == 1 ? "" : "s") + "-> " + _alert.Context;
            }
            else
            {
                if (alert.isFaction)
                {
                    ContextInfo.Text = alert.Context;
                }
                else
                {
                    ContextInfo.Text = alert.FriendlyName;
                }
            }

            IntelLine.ForeColor = configuration.AlertPopupIntelLineColour;
            if (alert.isFaction)
            {
                IntelLine.Text = FACTION_TEXT;
            } else if ( IsZkill )
            {
                IntelLine.Text = alert.Kill.Context;

                pAlertImage.Load($"https://images.evetech.net/types/{alert.Kill.VictimShipTypeId}/render");
                Image RoundedImage = ClipToCircle(pAlertImage.Image, new PointF(pAlertImage.Image.Width / 2, pAlertImage.Image.Height / 2), pAlertImage.Image.Width / 2, 
                    Color.Transparent ,_main.Configuration.ZKillCircleColour, 10, alert.Kill.TotalValue, _main.Configuration.ZKillIskColour, _main.Configuration.ZKillShowBar, _main.Configuration.ZKillBarColour);
                Bitmap b = new Bitmap(RoundedImage);
                b.MakeTransparent(b.GetPixel(0, 0));
                pAlertImage.Image = b;
            }
            else
            {
                IntelLine.Text = _alert.Entry.LineContent;
            }

            BackgroundImage = background;


            // position on screen
            if (_main.Configuration.AlertTransparent)
            {
                // use current eve window (for transparent)
                switch (_main.Configuration.DPSLocation)
                {
                    case PopupLocationType.TopLeft:
                        Location = new Point()
                        {
                            X = _main.currentEveWindowRect.Left + _main.Configuration.DPSGraphWindowSizeAdjust.X,
                            Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + _main.Configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.TopCenter:
                        Location = new Point()
                        {
                            X = ((_main.currentEveWindowRect.Right - _main.currentEveWindowRect.Left) / 2) - (Width / 2) + _main.currentEveWindowRect.Left,
                            Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + _main.Configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.TopRight:
                        Location = new Point()
                        {
                            X = _main.currentEveWindowRect.Right - Width - _main.Configuration.DPSGraphWindowSizeAdjust.X,
                            Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + _main.Configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.BottomLeft:
                        Location = new Point()
                        {
                            X = _main.currentEveWindowRect.Left + _main.Configuration.DPSGraphWindowSizeAdjust.X,
                            Y = _main.currentEveWindowRect.Bottom - Height - _main.Configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.BottomCenter:
                        Location = new Point()
                        {
                            X = ((_main.currentEveWindowRect.Right - _main.currentEveWindowRect.Left) / 2) - (Width / 2) + _main.currentEveWindowRect.Left,
                            Y = _main.currentEveWindowRect.Bottom - Height - _main.Configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.BottomRight:
                        Location = new Point()
                        {
                            X = _main.currentEveWindowRect.Right - Width - _main.Configuration.DPSGraphWindowSizeAdjust.X,
                            Y = _main.currentEveWindowRect.Bottom - Height - _main.Configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.Center:
                    default:
                        Location = new Point()
                        {
                            X = ((_main.currentEveWindowRect.Right - _main.currentEveWindowRect.Left) / 2) - (Width / 2) + _main.currentEveWindowRect.Left,
                            Y = ((_main.currentEveWindowRect.Top - _main.currentEveWindowRect.Bottom) / 2) - (Height / 2) + _main.currentEveWindowRect.Top,
                        };
                        break;
                }
            }
            else
            {

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
                    case PopupLocationType.TopCenter:
                        Location = new Point()
                        {
                            X = (screen.WorkingArea.Right / 2) - (Width / 2),
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
                    case PopupLocationType.BottomCenter:
                        Location = new Point()
                        {
                            X = (screen.WorkingArea.Right / 2) - (Width / 2),
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
                            X = (screen.WorkingArea.Right / 2) - (Width / 2),
                            Y = (screen.WorkingArea.Bottom / 2) - (Height / 2),
                        };
                        break;
                }
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

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private void pAlertImage_Click(object sender, EventArgs e)
        {
            if ( IsZkill )
            {
                System.Diagnostics.Process.Start($"https://zkillboard.com/kill/{_alert.Kill.ID}");
            }
        }

        // makes nice round ellipse/circle images from rectangle images
        public Image ClipToCircle(Image srcImage, PointF center, float radius, Color backGround, Color circleColour, int circleWidth, string isk, Color iskColour, bool showBar, Color barColor)
        {
            int TEXT_SIZE = 135;
            Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, srcImage.PixelFormat);
            
            using (Graphics g = Graphics.FromImage(dstImage))
            {
                // Border
                RectangleF r2 = new RectangleF(center.X - radius, center.Y - radius, (radius * 2), (radius * 2));

                // Main picture
                RectangleF r = new RectangleF(center.X - radius + circleWidth, center.Y - radius + circleWidth, (radius * 2)- (circleWidth*2), (radius * 2)- (circleWidth*2));

                RectangleF rISK = new RectangleF(0, 0, srcImage.Width, srcImage.Height);

                // enables smoothing of the edge of the circle (less pixelated)
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (backGround != Color.Transparent)
                {
                    // fills background color
                    using (Brush br = new SolidBrush(backGround))
                    {
                        g.FillRectangle(br, 0, 0, dstImage.Width, dstImage.Height);
                    }
                }

                if (circleWidth > 0)
                {
                    GraphicsPath pathborder = new GraphicsPath();
                    pathborder.AddEllipse(r2);
                    g.SetClip(pathborder);
                    using (Brush br = new SolidBrush(circleColour))
                    {
                        g.FillRectangle(br, 0, 0, dstImage.Width, dstImage.Height);
                    }
                }

                // adds the new ellipse & draws the image again 
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(r);
                g.SetClip(path);
                g.DrawImage(srcImage, 0, 0);

                g.ResetClip();
/*
                using (Brush br = new SolidBrush(Color.Orange))
                {
                    g.FillRectangle(br, 0, srcImage.Height-100 , srcImage.Width, 100);
                }
*/

                using (Font myfont = new Font("Arial", TEXT_SIZE - 10,FontStyle.Bold))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Far,
                        LineAlignment = StringAlignment.Center
                    };
                    if ( showBar ) g.FillRectangle(new SolidBrush(barColor), 0, srcImage.Height - TEXT_SIZE, srcImage.Width, TEXT_SIZE);
                    g.DrawString(isk, myfont, new SolidBrush(iskColour), new RectangleF(0, srcImage.Height - TEXT_SIZE, srcImage.Width, TEXT_SIZE), format);
                }


                return dstImage;
            }
        }


    }
}
