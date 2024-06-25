using EVEStandard.Models;
using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.SolarSystems;
using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Reflection.Emit;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Quesadilla
{
    public partial class QuesadillaAlertPopup : Form
    {
        private AlertQueueItem _alert;
        private int _seconds;

        private int _opacity;
        private int _opacitystep;
        private PopupLocationType _location;

        private bool ShowShipname = false;

        private Quesadilla _main;

        public bool IsZkill = false;
        public bool IsAlert = true;

        private const string ALERT_TEXT = "Alert";
        private const string FACTION_TEXT = "Faction Spawn";
        private const string DECLOAK_TEXT = "Pilot decloaked";

        private const int ZOOM_SIZE_INCREASE = 1;

        private bool _rightMouseDown = false;
        private Point diffPoint;

        public QuesadillaAlertPopup(AlertQueueItem alert, Config configuration, Image background, Quesadilla mainForm)
        {
            _main = mainForm;
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

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
                else if (alert.isDecloak)
                {
                    SystemName.Text = DECLOAK_TEXT;
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
                if ( alert.isDecloak)
                {
                    RegionName.Text = alert.Entry.CharacterName;
                }
            }

            ContextInfo.ForeColor = configuration.AlertPopupContextInfoColour;
            if (alert.Path != null && ! (alert.isFaction|| alert.isDecloak))
            {
                ContextInfo.Text = _alert.Path.TotalJumps.ToString() + " jump" + (_alert.Path.TotalJumps == 1 ? "" : "s") + "-> " + _alert.Context;
            }
            else
            {
                if (alert.isFaction)
                {
                    ContextInfo.Text = alert.Context;
                }
                else if (alert.isDecloak)
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
                Image RoundedImage = ImageGenerator.ClipToCircle(pAlertImage.Image, new PointF(pAlertImage.Image.Width / 2, pAlertImage.Image.Height / 2), pAlertImage.Image.Width / 2, 
                    Color.Transparent , configuration.ZKillCircleColour, 10, alert.Kill.TotalValue, configuration.ZKillIskColour, configuration.ZKillShowBar, configuration.ZKillBarColour);
                Bitmap killBitmap = new Bitmap(RoundedImage);
                killBitmap.MakeTransparent(killBitmap.GetPixel(0, 0));
                pAlertImage.Image = killBitmap;

                ShipName.Text = alert.Kill.VictimShipName;
            }
            else
            {
                IntelLine.Text = _alert.Entry.LineContent;
                ShipName.Text = "";
            }

            if (configuration.AlertPopupMinimap && (alert.Path != null && alert.Path.PathSystems != null && alert.Path.PathSystems.Count > 0 && alert.Path.FromSystemId != 0) )
            {
                pMinimapImage.Size = (Size)configuration.AlertPopupMinimapSize;
                Image minimapImage = ImageGenerator.ClipMiniMap(new Bitmap(pMinimapImage.Width, pMinimapImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb) , alert.Path , 0,100,
                    configuration.AlertPopupMinimapShowSystemText,
                    configuration.AlertPopupShowMinimapBorder, configuration.AlertPopupMinimapBorderColour,
                    configuration.AlertPopupMinimapLineColourNormal,
                    configuration.AlertPopupMinimapLineColourRoute,
                    configuration.AlertPopupMinimapSystemColourNormal,
                    configuration.AlertPopupMinimapSystemColourRoute,
                    configuration.AlertPopupMinimapSystemColourFrom,
                    configuration.AlertPopupMinimapSystemColourTo,
                    configuration.AlertPopupMinimapSystemColourText
                    );
                Bitmap minimapBitmap = new Bitmap(minimapImage);
                minimapBitmap.MakeTransparent(minimapBitmap.GetPixel(0, 0));
                pMinimapImage.Image = minimapBitmap;
                pMinimapImage.Location = configuration.AlertPopupMinimapPosition;
                pMinimapImage.Visible = true;
            }
            else
            {
                pMinimapImage.Visible = false;
            }

            BackgroundImage = background;

            // position on screen
            if (configuration.AlertTransparent)
            {
                // use current eve window (for transparent)
                switch (Program.MainConfiguration.DPSLocation)
                {
                    case PopupLocationType.TopLeft:
                        Location = new Point()
                        {
                            X = _main.currentEveWindowRect.Left + configuration.DPSGraphWindowSizeAdjust.X,
                            Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.TopCenter:
                        Location = new Point()
                        {
                            X = ((_main.currentEveWindowRect.Right - _main.currentEveWindowRect.Left) / 2) - (Width / 2) + _main.currentEveWindowRect.Left,
                            Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.TopRight:
                        Location = new Point()
                        {
                            X = _main.currentEveWindowRect.Right - Width - configuration.DPSGraphWindowSizeAdjust.X,
                            Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.BottomLeft:
                        Location = new Point()
                        {
                            X = _main.currentEveWindowRect.Left + configuration.DPSGraphWindowSizeAdjust.X,
                            Y = _main.currentEveWindowRect.Bottom - Height - configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.BottomCenter:
                        Location = new Point()
                        {
                            X = ((_main.currentEveWindowRect.Right - _main.currentEveWindowRect.Left) / 2) - (Width / 2) + _main.currentEveWindowRect.Left,
                            Y = _main.currentEveWindowRect.Bottom - Height - configuration.DPSGraphWindowSizeAdjust.Y,
                        };
                        break;
                    case PopupLocationType.BottomRight:
                        Location = new Point()
                        {
                            X = _main.currentEveWindowRect.Right - Width - configuration.DPSGraphWindowSizeAdjust.X,
                            Y = _main.currentEveWindowRect.Bottom - Height - configuration.DPSGraphWindowSizeAdjust.Y,
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
            MouseEventArgs e2 = (MouseEventArgs)e;
            Point clickLocation = new Point(e2.X, e2.Y);
            Rectangle r = new Rectangle(SystemName.Location.X, SystemName.Location.Y, SystemName.Width, SystemName.Height);
            if (r.Contains(clickLocation))
            {
                SystemName_Click(sender, e);
            }
            else
            {
                Opacity = 0;
                CloseWindow();
            }
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

        private void Zoom_MouseEnter(object sender, EventArgs e)
        {
            System.Windows.Forms.Label x = sender as System.Windows.Forms.Label;
            x.Font = new Font(x.Font.Name, x.Font.Size+ZOOM_SIZE_INCREASE, FontStyle.Bold);
            x.TextAlign = ContentAlignment.TopLeft;
        }

        private void Zoom_MouseLeave(object sender, EventArgs e)
        {
            System.Windows.Forms.Label x = sender as System.Windows.Forms.Label;
            x.Font = new Font(x.Font.Name, x.Font.Size-ZOOM_SIZE_INCREASE, FontStyle.Regular);
            x.TextAlign = ContentAlignment.TopRight;
        }

        private void IntelLine_Click(object sender, EventArgs e)
        {

        }

        private void QuesadillaAlertPopup_MouseDown(object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Right)
            {
                _rightMouseDown = true;
                diffPoint.X = System.Windows.Forms.Cursor.Position.X - this.Left;
                diffPoint.Y = System.Windows.Forms.Cursor.Position.Y - this.Top;
            }
        }

        private void QuesadillaAlertPopup_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _rightMouseDown = false;
            }
        }

        private void QuesadillaAlertPopup_MouseMove(object sender, MouseEventArgs e)
        {
            if (_rightMouseDown)
            {
                this.Left = System.Windows.Forms.Cursor.Position.X - diffPoint.X;
                this.Top = System.Windows.Forms.Cursor.Position.Y - diffPoint.Y;
            }
        }

        private void pAlertImage_MouseEnter(object sender, EventArgs e)
        {
            if (IsZkill)
            {
                //ShipName.Visible = true;
                ShowShipname = true;
                this.Invalidate();
            }
        }

        private void pAlertImage_MouseLeave(object sender, EventArgs e)
        {
            //ShipName.Visible = false;
            ShowShipname = false;
            this.Invalidate();  
        }

        private void pMinimap_Click(object sender, EventArgs e)
        {

        }

        private void QuesadillaAlertPopup_Paint(object sender, PaintEventArgs e)
        {
            ImageGenerator.PaintDrawText(e, AlertText);
            ImageGenerator.PaintDrawText(e, SystemName);
            ImageGenerator.PaintDrawText(e,RegionName);
            ImageGenerator.PaintDrawText(e,ContextInfo);
            if ( ShowShipname ) ImageGenerator.PaintDrawText(e, ShipName);
            ImageGenerator.PaintDrawText(e, ContextInfo);
            ImageGenerator.PaintDrawText(e, IntelLine);

        }

        private void ShipName_Click(object sender, EventArgs e)
        {

        }

        private void QuesadillaAlertPopup_Shown(object sender, EventArgs e)
        {
            _main.latestAlertBitmap = new Bitmap(this.Width, this.Height);
            this.DrawToBitmap(_main.latestAlertBitmap, new Rectangle(Point.Empty, _main.latestAlertBitmap.Size));
            _main.latestAlertTimestamp = DateTime.Now;
            // _main.latestAlertBitmap.Save("QuesadillaLatestAlert.png");
        }

        private void ContextInfo_Click(object sender, EventArgs e)
        {

        }
    }
}
