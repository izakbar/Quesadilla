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
                Image RoundedImage = ClipToCircle(pAlertImage.Image, new PointF(pAlertImage.Image.Width / 2, pAlertImage.Image.Height / 2), pAlertImage.Image.Width / 2, 
                    Color.Transparent ,_main.Configuration.ZKillCircleColour, 10, alert.Kill.TotalValue, _main.Configuration.ZKillIskColour, _main.Configuration.ZKillShowBar, _main.Configuration.ZKillBarColour);
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

            if (_main.Configuration.AlertPopupMinimap && (alert.Path != null && alert.Path.PathSystems != null && alert.Path.PathSystems.Count > 0 && alert.Path.FromSystemId != 0) )
            {
                Image minimapImage = ClipMiniMap(new Bitmap(pMinimapImage.Width, pMinimapImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb) , alert.Path ,
                    _main.Configuration.AlertPopupMinimapShowSystemText, 
                    _main.Configuration.AlertPopupShowMinimapBorder, _main.Configuration.AlertPopupMinimapBorderColor,
                    _main.Configuration.AlertPopupMinimapLineColorNormal,
                    _main.Configuration.AlertPopupMinimapLineColorRoute,
                    _main.Configuration.AlertPopupMinimapSystemColorNormal,
                    _main.Configuration.AlertPopupMinimapSystemColorRoute,
                    _main.Configuration.AlertPopupMinimapSystemColorFrom,
                    _main.Configuration.AlertPopupMinimapSystemColorTo,
                    _main.Configuration.AlertPopupMinimapSystemColorText
                    );
                Bitmap minimapBitmap = new Bitmap(minimapImage);
                minimapBitmap.MakeTransparent(minimapBitmap.GetPixel(0, 0));
                pMinimapImage.Image = minimapBitmap;
                pMinimapImage.Location = _main.Configuration.AlertPopupMinimapPosition;
                pMinimapImage.Size = (Size)_main.Configuration.AlertPopupMinimapSize;
                pMinimapImage.Visible = true;
            }
            else
            {
                pMinimapImage.Visible = false;
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

                using (Font myfont = new Font("Arial", TEXT_SIZE - 20,FontStyle.Bold))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Far,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.None,
                    };
                    if ( showBar ) g.FillRectangle(new SolidBrush(barColor), 0, srcImage.Height - TEXT_SIZE , srcImage.Width, TEXT_SIZE );
                    g.DrawString(isk, myfont, new SolidBrush(iskColour), new RectangleF(0, (srcImage.Height - TEXT_SIZE), srcImage.Width, TEXT_SIZE), format);
                }


                return dstImage;
            }
        }

        // makes nice round ellipse/circle images from rectangle images
        public Image ClipMiniMap(Image srcImage, PathInfo path, 
            bool showSystemText,
            bool Border, Color BorderColor,
            Color LineColorNormal,
            Color LineColorRoute,
            Color SystemColorNormal,
            Color SystemColorRoute,
            Color SystemColorFrom,
            Color SystemColorTo,
            Color SystemColorText
            )
        {
            Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, srcImage.PixelFormat);
            int TRANSPARENT_BORDER_WIDTH = 1;
            int BORDER_WIDTH = 1;
            int TEXT_SIZE = 8;

            double LY_SCALING = 9460000000000; 
            double GUI_SCALING = 30;

            int CIRCLE_RADIUS = 3;

            int combinedBorder = TRANSPARENT_BORDER_WIDTH + BORDER_WIDTH;

            Point center = new Point((srcImage.Width/2)-1, (srcImage.Height / 2)-1);

            using (Graphics g = Graphics.FromImage(dstImage))
            {
                // main background
                using (Brush br = new SolidBrush(Color.FromKnownColor(KnownColor.PaleGoldenrod)))
                {
                    g.FillRectangle(br, 0, 0, dstImage.Width, dstImage.Height);
                }

                // Border ?
                if (Border)
                {
                    using (Brush br = new SolidBrush(BorderColor))
                    {
                        g.FillRectangle(br, TRANSPARENT_BORDER_WIDTH, TRANSPARENT_BORDER_WIDTH, dstImage.Width - (TRANSPARENT_BORDER_WIDTH * 2), dstImage.Height - (TRANSPARENT_BORDER_WIDTH * 2));
                    }
                    using (Brush br = new SolidBrush(Color.FromKnownColor(KnownColor.PaleGoldenrod)))
                    {
                        g.FillRectangle(br, combinedBorder, combinedBorder, dstImage.Width - (combinedBorder * 2), dstImage.Height - (combinedBorder * 2));
                    }
                } 
                else
                {
                    using (Brush br = new SolidBrush(Color.FromKnownColor(KnownColor.PaleGoldenrod)))
                    {
                        g.FillRectangle(br, TRANSPARENT_BORDER_WIDTH, TRANSPARENT_BORDER_WIDTH, dstImage.Width - (TRANSPARENT_BORDER_WIDTH * 2), dstImage.Height - (TRANSPARENT_BORDER_WIDTH * 2));
                    }
                }

                var FromSystem = _main._universe.SolarSystems[path.FromSystemId];


                // add route systems
                List<MinimapSystem> minimapSystems = new List<MinimapSystem>();

                foreach(var pathSystem in path.PathSystems)
                {
                    var _solarSystem = _main._universe.SolarSystems[pathSystem];

                    minimapSystems.Add(new MinimapSystem()
                    {
                        solarSystem = _solarSystem,
                        fromSystem = pathSystem == path.FromSystemId ? true : false,
                        toSystem = pathSystem == path.ToSystemId ? true : false,
                        route = true,
                        textDisplay = (pathSystem == path.FromSystemId || pathSystem == path.ToSystemId) ? true : false,
                        X = (int)Math.Round((FromSystem.X - _solarSystem.X) / LY_SCALING),
                        Y = (int)Math.Round((FromSystem.Y - _solarSystem.Y) / LY_SCALING),
                    });
                }

                // work out scale 
                int maxX = 0;
                int maxY = 0;
                int minX = 0;
                int minY = 0;

                foreach (var currentSystem in minimapSystems)
                {
                    maxX = Math.Max(maxX, currentSystem.X);
                    maxY = Math.Max(maxY, currentSystem.Y);
                    minX = Math.Min(minX, currentSystem.X);
                    minY = Math.Min(minY, currentSystem.Y);
                }
                int maxSize = Math.Max((int)(maxX - minX), (int)(maxY - minY));
                int pictureSize = Math.Min(srcImage.Width, srcImage.Height) - BORDER_WIDTH - TRANSPARENT_BORDER_WIDTH - (CIRCLE_RADIUS*2);
                GUI_SCALING = (int)Math.Round(  (double)(maxSize / (pictureSize * .8)));

                // Apply scale
                maxX = 0;maxY = 0;minX = 0;minY = 0;
                foreach (var currentSystem in minimapSystems)
                {
                    currentSystem.X = (int)Math.Round(currentSystem.X / GUI_SCALING);
                    currentSystem.Y = (int)Math.Round(currentSystem.Y / GUI_SCALING);

                    maxX = Math.Max(maxX, currentSystem.X);
                    maxY = Math.Max(maxY, currentSystem.Y);
                    minX = Math.Min(minX, currentSystem.X);
                    minY = Math.Min(minY, currentSystem.Y);

                }

                int ox = center.X;
                int oy = center.Y;

                if (minimapSystems.Count > 1)
                {
                    ox = ((srcImage.Width - (maxX - minX)) / 2) - BORDER_WIDTH - CIRCLE_RADIUS - TRANSPARENT_BORDER_WIDTH - 1;
                    oy = ((srcImage.Height - (maxY - minY)) / 2) - BORDER_WIDTH - CIRCLE_RADIUS - TRANSPARENT_BORDER_WIDTH - 1;
                    if (minX < 0) ox = ox + Math.Abs(minX);
                    if (minY < 0) oy = oy + Math.Abs(minY);
                }

                // adjust origin
                Point originPoint = new Point(ox, oy);

                // draw route lines
                var previousSystem = minimapSystems[0];
                Point previousPoint = new Point(originPoint.X + minimapSystems[0].X, originPoint.Y + minimapSystems[0].Y);
                foreach (var currentSystem in minimapSystems)
                {
                    Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
                    if (!currentSystem.solarSystem.Id.Equals(path.FromSystemId))
                    {
                        Pen linePen = new Pen(currentSystem.route ? LineColorRoute : LineColorNormal, 1);

                        g.DrawLine(linePen, previousPoint, currentPoint);
                    }

                    previousSystem = currentSystem;
                    previousPoint = currentPoint;
                }

                // draw system blobs
                foreach (var currentSystem in minimapSystems)
                {
                    Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
                    Color systemBrushColour = SystemColorNormal;
                    if (currentSystem.route) systemBrushColour = SystemColorRoute;
                    if (currentSystem.fromSystem) systemBrushColour = SystemColorFrom;
                    if (currentSystem.toSystem) systemBrushColour = SystemColorTo;
                    Brush systemBrush = new SolidBrush(systemBrushColour);

                    g.FillEllipse(systemBrush, currentPoint.X - CIRCLE_RADIUS, currentPoint.Y - CIRCLE_RADIUS, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
                }

                // draw text
                if (showSystemText)
                {
                    foreach (var currentSystem in minimapSystems)
                    {
                        if (!currentSystem.textDisplay) continue;

                        Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);

                        using (Font myfont = new Font("Verdana", TEXT_SIZE, FontStyle.Regular))
                        {
                            g.TextRenderingHint = TextRenderingHint.AntiAlias;

                            g.DrawString(currentSystem.solarSystem.Name, myfont, new SolidBrush(SystemColorText), currentPoint.X + CIRCLE_RADIUS + 2, currentPoint.Y + CIRCLE_RADIUS + 2, new StringFormat
                            {
                                Alignment = StringAlignment.Far,
                                LineAlignment = StringAlignment.Near,
                                Trimming = StringTrimming.None,
                            });
                        }

                    }
                }

                return dstImage;
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
                ShipName.Visible = true;
            }
        }

        private void pAlertImage_MouseLeave(object sender, EventArgs e)
        {
            ShipName.Visible = false;
        }

        private void pMinimap_Click(object sender, EventArgs e)
        {

        }

        private void QuesadillaAlertPopup_Paint(object sender, PaintEventArgs e)
        {
            AlertDrawText(e, AlertText);
            AlertDrawText(e, SystemName);
            AlertDrawText(e,RegionName);
            AlertDrawText(e,ContextInfo);
            //AlertDrawText(e, ShipName);
            AlertDrawText(e, ContextInfo);
            AlertDrawText(e, IntelLine);

        }
        private void AlertDrawText(PaintEventArgs e, System.Windows.Forms.Label l)
        {
            var flags = TextFormatFlags.Right;
            if ( l.TextAlign == ContentAlignment.TopLeft || l.TextAlign == ContentAlignment.BottomLeft || l.TextAlign == ContentAlignment.MiddleLeft ) flags = TextFormatFlags.Left;
            flags = flags | TextFormatFlags.WordBreak;

            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            TextRenderer.DrawText(e.Graphics,l.Text,l.Font,new Rectangle(l.Left, l.Top, l.Width, l.Height),l.ForeColor, flags);
        }

        private void ShipName_Click(object sender, EventArgs e)
        {

        }

        private void QuesadillaAlertPopup_Shown(object sender, EventArgs e)
        {
            _main.latestAlertBitmap = new Bitmap(this.Width, this.Height);
            this.DrawToBitmap(_main.latestAlertBitmap, new Rectangle(Point.Empty, _main.latestAlertBitmap.Size));
            _main.latestAlertTimestamp = DateTime.Now;
            _main.latestAlertBitmap.Save("QuesadillaLatestAlert.png");
        }
    }
}
