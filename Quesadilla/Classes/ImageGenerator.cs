using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics.Metrics;
using System.ComponentModel;
using System.Data;
using Microsoft.CodeAnalysis;

namespace Quesadilla.Classes
{
    public class ImageGenerator
    {
        private const int TRANSPARENT_BORDER_WIDTH = 1;
        private const int BORDER_WIDTH = 1;
        private const int TEXT_SIZE = 8;
        private const int CIRCLE_KILL_GAP = 4;
        private const int CIRCLE_ALERT_GAP = 6;
        private const int CIRCLE_ALT_GAP = 2;

        private const double LY_SCALING = 9460000000000;

        private const int CIRCLE_RADIUS = 3;
        private const int INCURSION_CLOUD_SIZE = 60;
        private const int SYSTEM_CIRCLE_SIZE = 6;

        public static void DrawSystemName(Graphics g, int X, int Y, string text, Color c)
        {
            using (Font myfont = new Font("Verdana", TEXT_SIZE, FontStyle.Regular))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                var stringSize = g.MeasureString(text, myfont);

                g.DrawString(text, myfont, new SolidBrush(c),
                    X - (stringSize.Width / 2),
                    Y + CIRCLE_RADIUS + 2
                    );
            }
        }
        public static Image ClipMiniMap(Image srcImage, PathInfo path, int closeSystems, int mapScale, bool rawXY, ref string extraInformation, ref List<MinimapSystem> returnMinimapSystems,
            bool showSystemText,
            bool Border, Color BorderColor,
            Color LineColourNormal,
            Color LineColourRoute,
            Color SystemColourNormal,
            Color SystemColourRoute,
            Color SystemColourFrom,
            Color SystemColourTo,
            Color SystemColourText,
            Color ZKillColour,
            Color IntelColour,
            Color AltColour,
            Color IncursionColour,
            int animationCycle
            )
        {

            double GUI_SCALING = 30;

            Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, srcImage.PixelFormat);

            int combinedBorder = TRANSPARENT_BORDER_WIDTH + BORDER_WIDTH;

            StringBuilder sbExtraInformation = new StringBuilder();

            Point center = new Point((srcImage.Width / 2) - 1, (srcImage.Height / 2) - 1);

            // quick note on x/y coord calcs
            // dotlan calcs need to be inverted to make sure map displays correctly - not an error

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

                if (path is null || path.FromSystemId == 0) return dstImage;
                var FromSystem = Program.Universe.SolarSystems[path.FromSystemId];

                // add route systems
                List<MinimapSystem> minimapSystems = new List<MinimapSystem>();
                List<MinimapConnection> minimapConnectingSystems = new List<MinimapConnection>();

                SolarSystem previousSystem = FromSystem;
                foreach (var pathSystem in path.PathSystems)
                {
                    var nextSolarSystem = Program.Universe.SolarSystems[pathSystem];

                    if (minimapSystems.Where(o => o.solarSystem.Name == nextSolarSystem.Name).Count() == 0)
                    {
                        minimapSystems.Add(new MinimapSystem()
                        {
                            solarSystem = nextSolarSystem,
                            fromSystem = pathSystem == path.FromSystemId ? true : false,
                            toSystem = pathSystem == path.ToSystemId ? true : false,
                            route = true,
                            textDisplay = (pathSystem == path.FromSystemId || pathSystem == path.ToSystemId) ? true : false,
                            X = (rawXY ? (int)Math.Round((FromSystem.X - nextSolarSystem.X) / LY_SCALING) : (int)Math.Round((nextSolarSystem.DotlanX - FromSystem.DotlanX))),
                            Y = (rawXY ? (int)Math.Round((FromSystem.Y - nextSolarSystem.Y) / LY_SCALING) : (int)Math.Round((nextSolarSystem.DotlanY - FromSystem.DotlanY))),
                        });

                        // add connection
                        if (nextSolarSystem != FromSystem)
                        {
                            minimapConnectingSystems.Add(new MinimapConnection()
                            {
                                FromSystem = previousSystem,
                                ToSystem = nextSolarSystem,
                                route = true,
                                P1 = (rawXY
                                ? new Point((int)Math.Round((FromSystem.X - previousSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - previousSystem.Y) / LY_SCALING))
                                : new Point((int)Math.Round((previousSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((previousSystem.DotlanY - FromSystem.DotlanY)))
                                ),
                                P2 = (rawXY
                                ? new Point((int)Math.Round((FromSystem.X - nextSolarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - nextSolarSystem.Y) / LY_SCALING))
                                : new Point((int)Math.Round((nextSolarSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((nextSolarSystem.DotlanY - FromSystem.DotlanY)))
                                ),
                            });
                        }
                        previousSystem = nextSolarSystem;

                    }
                }

                // should we use connecting systems too ?
                if (closeSystems > 0)
                {
                    List<MinimapSystem> minimapSystemsAdditional = new List<MinimapSystem>();
                    for (int Counter = 0; Counter < closeSystems; Counter++)
                    {
                        foreach (var mapSystem in minimapSystems)
                        {
                            foreach (var connection in mapSystem.solarSystem.ConnectedTo)
                            {
                                var linkedSolarSystem = Program.Universe.SolarSystems[connection.ToSystemId];
                                var linkedMinimapSystem = minimapSystems.Where(o => o.solarSystem.Id == linkedSolarSystem.Id).FirstOrDefault();
                                if (minimapSystems.Where(o => o.solarSystem.Name == linkedSolarSystem.Name).Count() == 0 &&
                                    minimapSystemsAdditional.Where(o => o.solarSystem.Name == linkedSolarSystem.Name).Count() == 0)
                                {
                                    minimapSystemsAdditional.Add(new MinimapSystem()
                                    {
                                        solarSystem = linkedSolarSystem,
                                        fromSystem = false,
                                        toSystem = false,
                                        route = false,
                                        textDisplay = false,
                                        X = (rawXY ? (int)Math.Round((FromSystem.X - linkedSolarSystem.X) / LY_SCALING) : (int)Math.Round((linkedSolarSystem.DotlanX - FromSystem.DotlanX))),
                                        Y = (rawXY ? (int)Math.Round((FromSystem.Y - linkedSolarSystem.Y) / LY_SCALING) : (int)Math.Round((linkedSolarSystem.DotlanY - FromSystem.DotlanY))),
                                    });
                                }

                                // check if connection already saved
                                if (
                                    (minimapConnectingSystems.Where(o =>
                                      (o.FromSystem.Id == mapSystem.solarSystem.Id && o.ToSystem.Id == connection.ToSystemId) ||
                                      (o.ToSystem.Id == mapSystem.solarSystem.Id && o.FromSystem.Id == connection.ToSystemId)).Count() == 0)
                                    )
                                {
                                    minimapConnectingSystems.Add(new MinimapConnection()
                                    {
                                        FromSystem = mapSystem.solarSystem,
                                        ToSystem = linkedSolarSystem,
                                        route = (mapSystem.route == true & (linkedMinimapSystem != null && linkedMinimapSystem.route == true)),
                                        P1 = (rawXY
                                        ? new Point((int)Math.Round((FromSystem.X - mapSystem.solarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - mapSystem.solarSystem.Y) / LY_SCALING))
                                        : new Point((int)Math.Round((mapSystem.solarSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((mapSystem.solarSystem.DotlanY - FromSystem.DotlanY)))
                                        ),
                                        P2 = (rawXY
                                        ? new Point((int)Math.Round((FromSystem.X - linkedSolarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - linkedSolarSystem.Y) / LY_SCALING))
                                        : new Point((int)Math.Round((linkedSolarSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((linkedSolarSystem.DotlanY - FromSystem.DotlanY)))
                                        ),
                                    });
                                }
                            }
                        }
                        minimapSystems.AddRange(minimapSystemsAdditional);
                        minimapSystemsAdditional.Clear();
                    }
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
                int pictureSize = Math.Min(srcImage.Width, srcImage.Height) - BORDER_WIDTH - TRANSPARENT_BORDER_WIDTH - (CIRCLE_ALERT_GAP * 4);
                GUI_SCALING = (int)Math.Round((double)(maxSize) / ((pictureSize * (rawXY ? .8 : .8))));
                GUI_SCALING = GUI_SCALING * (((double)mapScale / 100));
                // Apply scale
                maxX = 0; maxY = 0; minX = 0; minY = 0;
                if (GUI_SCALING > 0)
                {
                    foreach (var currentSystem in minimapSystems)
                    {
                        currentSystem.X = (int)Math.Round(currentSystem.X / GUI_SCALING);
                        currentSystem.Y = (int)Math.Round(currentSystem.Y / GUI_SCALING);

                        maxX = Math.Max(maxX, currentSystem.X);
                        maxY = Math.Max(maxY, currentSystem.Y);
                        minX = Math.Min(minX, currentSystem.X);
                        minY = Math.Min(minY, currentSystem.Y);
                    }

                    foreach (var connection in minimapConnectingSystems)
                    {
                        connection.P1 = new Point((int)Math.Round(connection.P1.X / GUI_SCALING), (int)Math.Round(connection.P1.Y / GUI_SCALING));
                        connection.P2 = new Point((int)Math.Round(connection.P2.X / GUI_SCALING), (int)Math.Round(connection.P2.Y / GUI_SCALING));
                    }
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

                // draw incursion base if required
                foreach (var currentSystem in minimapSystems.Where(o => o.solarSystem.Incursion))
                {
                    Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
                    Color TransparentIncursionColour = Color.FromArgb(255, IncursionColour);
                    Brush incursionBrush = new SolidBrush(TransparentIncursionColour);
                    float incursionBlobSize = (INCURSION_CLOUD_SIZE / (float)GUI_SCALING);
                    g.FillEllipse(incursionBrush, currentPoint.X - incursionBlobSize, currentPoint.Y - incursionBlobSize, incursionBlobSize * 2, incursionBlobSize * 2);
                }

                    // draw connections
                foreach (var connection in minimapConnectingSystems)
                {
                    Point P1 = new Point(originPoint.X + connection.P1.X, originPoint.Y + connection.P1.Y);
                    Point P2 = new Point(originPoint.X + connection.P2.X, originPoint.Y + connection.P2.Y);
                    Pen linePen = new Pen(connection.route ? LineColourRoute : LineColourNormal, 1);
                    g.DrawLine(linePen, P1, P2);
                }

                // draw system blobs
                foreach (var currentSystem in minimapSystems)
                {
                    Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
                    float blobSize = (SYSTEM_CIRCLE_SIZE / (float)GUI_SCALING);

                    Color systemBrushColour = SystemColourNormal;
                    if (currentSystem.route) systemBrushColour = SystemColourRoute;
                    if (currentSystem.fromSystem) systemBrushColour = SystemColourFrom;
                    if (currentSystem.toSystem) systemBrushColour = SystemColourTo;
                    Brush systemBrush = new SolidBrush(systemBrushColour);
                    // g.FillEllipse(systemBrush, currentPoint.X - CIRCLE_RADIUS, currentPoint.Y - CIRCLE_RADIUS, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
                    g.FillEllipse(systemBrush, currentPoint.X - blobSize, currentPoint.Y - blobSize, blobSize * 2, blobSize * 2);

                    if (currentSystem.solarSystem.Kills != null && currentSystem.solarSystem.Kills.Count() > 0)
                    {
                        var zkillPen = new Pen(ZKillColour);
                        int zkillPulse = animationCycle;
                        g.DrawEllipse(zkillPen, currentPoint.X - (CIRCLE_RADIUS + CIRCLE_KILL_GAP + zkillPulse), currentPoint.Y - (CIRCLE_RADIUS + CIRCLE_KILL_GAP + zkillPulse),
                            (CIRCLE_RADIUS + CIRCLE_KILL_GAP + zkillPulse) * 2, (CIRCLE_RADIUS + CIRCLE_KILL_GAP+ zkillPulse) * 2);
                        foreach (var k in currentSystem.solarSystem.Kills)
                        {
                            sbExtraInformation.AppendLine($"ZKILL {k.Timestamp.ToString("HH:mm:ss")} {k.SolarSystemName} {k.VictimShipName} {k.VictimCharacterName}");
                        }
                    }
                    if (currentSystem.solarSystem.Alerts != null && currentSystem.solarSystem.Alerts.Where(o => !o.isFaction && !o.isDecloak && o.Kill is null).Count() > 0)
                    {
                        var intelPen = new Pen(IntelColour);
                        int intelPulse = animationCycle;
                        g.DrawEllipse(intelPen, currentPoint.X - (CIRCLE_RADIUS + (CIRCLE_ALERT_GAP) + intelPulse), currentPoint.Y - (CIRCLE_RADIUS + (CIRCLE_ALERT_GAP) + intelPulse), 
                            (CIRCLE_RADIUS + CIRCLE_ALERT_GAP + intelPulse) * 2, (CIRCLE_RADIUS + CIRCLE_ALERT_GAP + intelPulse) * 2);
                        foreach (var a in currentSystem.solarSystem.Alerts.Where(o => !o.isFaction && !o.isDecloak && o.Kill is null))
                        {
                            sbExtraInformation.AppendLine($"{a.Entry.LogTime} {a.Entry.LineContent}");
                        }
                    }

                    // active alt in sytstem ?
                    if (Program.CharacterList.Where(o => o.IsClientActive && o.IsCurrentlActiveClient == false && o.LocationId == currentSystem.solarSystem.Id).Count() > 0)
                    {
                        var altPen = new Pen(AltColour);
                        int altPulse = animationCycle;
/*
                        g.DrawEllipse(altPen, currentPoint.X - (CIRCLE_RADIUS + (CIRCLE_ALT_GAP) + altPulse), currentPoint.Y - (CIRCLE_RADIUS + (CIRCLE_ALT_GAP) + altPulse),
                            (CIRCLE_RADIUS + CIRCLE_ALT_GAP + altPulse) * 2, (CIRCLE_RADIUS + CIRCLE_ALT_GAP + altPulse) * 2);
*/
                        g.DrawEllipse(altPen, currentPoint.X - (blobSize + (CIRCLE_ALT_GAP) + altPulse), currentPoint.Y - (blobSize + (CIRCLE_ALT_GAP) + altPulse),
                            (blobSize + CIRCLE_ALT_GAP + altPulse) * 2, (blobSize+ CIRCLE_ALT_GAP + altPulse) * 2);
                    }
                }

                // draw text
                if (showSystemText)
                {
                    foreach (var currentSystem in minimapSystems)
                    {
                        if (!currentSystem.textDisplay) continue;

                        Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);

                        DrawSystemName(g, currentPoint.X, currentPoint.Y, currentSystem.solarSystem.Name, SystemColourText);
                    }
                }

                extraInformation = sbExtraInformation.ToString();

                if (returnMinimapSystems != null)
                {
                    lock (returnMinimapSystems)
                    {
                        returnMinimapSystems.Clear();

                        foreach (var currentSystem in minimapSystems)
                        {
                            currentSystem.X += originPoint.X;
                            currentSystem.Y += originPoint.Y;
                        }

                        returnMinimapSystems = minimapSystems;
                    }
                }

                return dstImage;
            }
        }

        // makes nice round ellipse/circle images from rectangle images
        public static Image ClipToCircle(Image srcImage, PointF center, float radius, Color backGround, Color circleColour, int circleWidth, string isk, Color iskColour, bool showBar, Color barColor)
        {
            int TEXT_SIZE = 135;
            Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, srcImage.PixelFormat);

            using (Graphics g = Graphics.FromImage(dstImage))
            {
                // Border
                RectangleF r2 = new RectangleF(center.X - radius, center.Y - radius, (radius * 2), (radius * 2));

                // Main picture
                RectangleF r = new RectangleF(center.X - radius + circleWidth, center.Y - radius + circleWidth, (radius * 2) - (circleWidth * 2), (radius * 2) - (circleWidth * 2));

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

                using (Font myfont = new Font("Arial", TEXT_SIZE - 20, FontStyle.Bold))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Far,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.None,
                    };
                    if (showBar) g.FillRectangle(new SolidBrush(barColor), 0, srcImage.Height - TEXT_SIZE, srcImage.Width, TEXT_SIZE);
                    g.DrawString(isk, myfont, new SolidBrush(iskColour), new RectangleF(0, (srcImage.Height - TEXT_SIZE), srcImage.Width, TEXT_SIZE), format);
                }


                return dstImage;
            }
        }

        public static void PaintDrawText(PaintEventArgs e, System.Windows.Forms.Label l)
        {
            var flags = TextFormatFlags.Right;
            if (l.TextAlign == ContentAlignment.TopLeft || l.TextAlign == ContentAlignment.BottomLeft || l.TextAlign == ContentAlignment.MiddleLeft) flags = TextFormatFlags.Left;
            flags = flags | TextFormatFlags.WordBreak;

            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            TextRenderer.DrawText(e.Graphics, l.Text, l.Font, new Rectangle(l.Left, l.Top, l.Width, l.Height), l.ForeColor, flags);
        }

        public static Size PaintDrawText(PaintEventArgs e, string s, int x, int y, Color c, Font f)
        {
            var flags = TextFormatFlags.Left;
            // if (l.TextAlign == ContentAlignment.TopLeft || l.TextAlign == ContentAlignment.BottomLeft || l.TextAlign == ContentAlignment.MiddleLeft) flags = TextFormatFlags.Left;
            flags = flags | TextFormatFlags.WordBreak;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            Size size = TextRenderer.MeasureText(s, f);
            TextRenderer.DrawText(e.Graphics, s, f, new Point(x, y), c, flags);
            return size;
        }

    }
}
