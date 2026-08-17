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
using Microsoft.CodeAnalysis.Text;
using EVEStandard.Models;
using System.Collections.Immutable;
using EVEStandard.API;
using System.Drawing.Imaging;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using Quesadilla.Classes.Configuration;
using Windows.UI.Composition.Effects;
using System.Windows.Forms.DataVisualization.Charting;
using Windows.Devices.Display;

namespace Quesadilla.Classes
{
    public class ImageGenerator
    {
        private const int TRANSPARENT_BORDER_WIDTH = 1;
        private const int BORDER_WIDTH = 1;
        private const int TEXT_SIZE = 20;

        private const double MERCOXIT_SECURITY = -0.75;


		private const int CIRCLE_INITIAL_GAP = 3;
		private const int CIRCLE_INCREASE_GAP = 7;

		private const double LY_SCALING = 9460000000000;

        private const int CIRCLE_RADIUS = 3;
        private const int SYSTEM_CIRCLE_SIZE = 6;
        private const int STORM_CIRCLE_SIZE = 40;

		private const float MINIMUM_TEXT_SIZE = 9;
        private const int MINIMUM_BLOB_SIZE = 3;
        private const float MINIMUM_CIRCLE_SIZE = 2.3f;

        public static void DrawSystemName(Graphics g, int X, int Y, string text, Color c, float textSize = MINIMUM_TEXT_SIZE, bool enforceMinimumSize = true )
        {
            // ensure we do not go too small
            if ( enforceMinimumSize && (textSize < MINIMUM_TEXT_SIZE || float.IsInfinity(textSize) ) ) textSize = MINIMUM_TEXT_SIZE;
            if (float.IsInfinity(textSize)) return;
            using (Font myfont = new Font("Verdana", textSize, FontStyle.Regular))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                var stringSize = g.MeasureString(text, myfont);

                g.DrawString(text, myfont, new SolidBrush(c),
                    X - (stringSize.Width / 2),
                    Y + CIRCLE_RADIUS + 2
                    );
            }
        }


        public static void DrawStorm(string stormType, string location, Graphics g, Point currentPoint, float circleSize, float blobSize, bool outline)
		{
			float[] dashSet = { 4, 8, 4, 8 };
			Pen circlePen = new Pen(Color.White, 2);
			Brush circleBrush = new SolidBrush(Color.White);

			switch (stormType)
			{
				case "Electric":
					circleBrush = new HatchBrush(HatchStyle.Percent60, Color.DeepSkyBlue);
					break;
				case "Plasma":
					circleBrush = new HatchBrush(HatchStyle.Percent60, Color.DarkRed);
					break;
				case "Gamma":
					circleBrush = new HatchBrush(HatchStyle.Percent60, Color.ForestGreen);
					break;
				case "Exotic":
					circleBrush = new HatchBrush(HatchStyle.Percent60, Color.BlueViolet);
					break;
			}

			switch (location)
            {
			    case "CENTRE" : circlePen = new Pen(Color.Red, 4); break;
			    case "STRONG" : circlePen = new Pen(Color.Orange, 4); break;
			    case "WEAK" : circlePen = new Pen(Color.Yellow, 4); break;
                default:
                    {
                        circlePen = new Pen(Color.Black, 4); 
						circleBrush = new SolidBrush(Color.Black);
                        break;
					}
			}

            //			circlePen.DashPattern = dashSet;

            try
            {
                if (outline)
                {
                    g.DrawArc(circlePen,
                        //currentPoint.X - (blobSize + (circleSize) + (float)animationCycleAlts), currentPoint.Y - (blobSize + (circleSize) + (float)animationCycleAlts),
                        currentPoint.X - (blobSize + circleSize), currentPoint.Y - (blobSize + circleSize),
                        //(blobSize + circleSize + (float)animationCycleAlts) * 2, (blobSize + circleSize + (float)animationCycleAlts) * 2,
                        (blobSize + circleSize) * 2, (blobSize + circleSize) * 2,
                        0, 360);
                }
                else
                {
                    g.FillEllipse(circleBrush,
                    currentPoint.X - (blobSize + circleSize), currentPoint.Y - (blobSize + circleSize),
                    (blobSize + circleSize) * 2, (blobSize + circleSize) * 2);
                }
                      }
            catch { }
            {

            }
		}

        private static void MapCircles(Graphics g, List<MinimapSystem> systems, Point originPoint, float size, Pen outlinePen, Brush fillBrush) 
        {
			Brush eraserBrush = new SolidBrush(Color.Black);
			// outlines
			foreach (var currentSystem in systems)
			{
				Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
				g.DrawEllipse(outlinePen, currentPoint.X - size, currentPoint.Y - size, size * 2, size * 2);
			}

			// clear any overspill
			foreach (var currentSystem in systems)
			{
				Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
				g.FillEllipse(eraserBrush, currentPoint.X - size, currentPoint.Y - size, size * 2, size * 2);
			}

            // fill inside
			foreach (var currentSystem in systems)
			{
				Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
				g.FillEllipse(fillBrush, currentPoint.X - size, currentPoint.Y - size, size * 2, size * 2);
			}
		}


		public static Image ClipMiniMap(Image srcImage, PathInfo path, int closeSystems, int mapScale, 
            MapType whichMap, ref string extraInformation, ref List<MinimapSystem> returnMinimapSystems,
            bool showSystemText,bool showAllSystemsText, bool showConnectingSystemsText,
            bool Border, Color BorderColor,
            Color LineColourNormal,
            Color LineColourOutOfRegion,
            Color LineColourRoute,
            Color SystemColourNormal,
            Color SystemColourRoute,
            Color SystemColourFrom,
            Color SystemColourTo,
			Color SystemColourText, Color SystemColourAllText, Color SystemColourConnectingText,
			Color ZKillColour,
            Color IntelColour,
            Color AltColour,
            Color UniHoleColour,
            Color IncursionColour,Color IncursionOutlineColour,
			Color SecLimitColour,Color SecLimitOutlineColour,
			Color IceColour,
            Color MercoxitColour,
            int? animationCycleIntel,
            int? animationCycleKills,
            int? animationCycleAlts,
            int? animationCycleUniHoles,
            bool defaultTextSize = false,
            bool showOutOfRegion = false,
            bool showSecLimit = false,
            int incursionAlphaAmount = 50,
			int incursionCloudSize = 60,
			int secLimitAlphaAmount = 50,
			int secLimitCloudSize = 20,
			int connectingLineGap = 10,
            float secLimit = 0.0f
            )
        {
			double GUI_SCALING = 30;

            Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, PixelFormat.Format32bppArgb);

            int combinedBorder = TRANSPARENT_BORDER_WIDTH + BORDER_WIDTH;

            StringBuilder sbExtraInformation = new StringBuilder();

            Point center = new Point((srcImage.Width / 2) - 1, (srcImage.Height / 2) - 1);

			Brush eraserBrush = new SolidBrush(Color.Black);

			// quick note on x/y coord calcs
			// dotlan calcs need to be inverted to make sure map displays correctly - not an error

			using (Graphics g = Graphics.FromImage(dstImage))
            {

                Brush backgroundBrush = new SolidBrush(Color.FromArgb(0, Color.PaleGoldenrod));
				// main background
				//g.FillRectangle(backgroundBrush, 0, 0, dstImage.Width, dstImage.Height);

				// Border ?
				if (Border)
                {
                    using (Brush br = new SolidBrush(BorderColor))
                    {
                        g.FillRectangle(br, TRANSPARENT_BORDER_WIDTH, TRANSPARENT_BORDER_WIDTH, dstImage.Width - (TRANSPARENT_BORDER_WIDTH * 2), dstImage.Height - (TRANSPARENT_BORDER_WIDTH * 2));
                    }
                    g.FillRectangle(backgroundBrush, combinedBorder, combinedBorder, dstImage.Width - (combinedBorder * 2), dstImage.Height - (combinedBorder * 2));
                }
                else
                {
                    g.FillRectangle(backgroundBrush, TRANSPARENT_BORDER_WIDTH, TRANSPARENT_BORDER_WIDTH, dstImage.Width - (TRANSPARENT_BORDER_WIDTH * 2), dstImage.Height - (TRANSPARENT_BORDER_WIDTH * 2));
                }

                if (path is null || path.FromSystemId == 0 || path.FromSystemId == -1) return dstImage;
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
                            textDisplayConnecting = false,
							X = ( 
                            whichMap == MapType.SDE ? (int)Math.Round((FromSystem.X - nextSolarSystem.X) / LY_SCALING) :
                            ( whichMap == MapType.SDE2D ? (int)Math.Round((FromSystem.X2D - nextSolarSystem.X2D) / LY_SCALING) :
							(int)Math.Round((nextSolarSystem.DotlanX - FromSystem.DotlanX)))
							),
							Y = (
							whichMap == MapType.SDE ? (int)Math.Round((FromSystem.Y - nextSolarSystem.Y) / LY_SCALING) :
							(whichMap == MapType.SDE2D ? (int)Math.Round((FromSystem.Y2D - nextSolarSystem.Y2D) / LY_SCALING) :
							(int)Math.Round((nextSolarSystem.DotlanY - FromSystem.DotlanY)))
							),
						});

                        // add connection
                        if (nextSolarSystem != FromSystem)
                        {
                            minimapConnectingSystems.Add(new MinimapConnection()
                            {
                                FromSystem = previousSystem,
                                ToSystem = nextSolarSystem,
                                route = true,
								P1 = (
							        whichMap == MapType.SDE ? new Point((int)Math.Round((FromSystem.X - previousSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - previousSystem.Y) / LY_SCALING)) :
							        (whichMap == MapType.SDE2D ? new Point((int)Math.Round((FromSystem.X2D - previousSystem.X2D) / LY_SCALING), (int)Math.Round((FromSystem.Y2D - previousSystem.Y2D) / LY_SCALING)) :
							        new Point((int)Math.Round((previousSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((previousSystem.DotlanY - FromSystem.DotlanY))))
							    ),
								P2 = (
									whichMap == MapType.SDE ? new Point((int)Math.Round((FromSystem.X - nextSolarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - nextSolarSystem.Y) / LY_SCALING)) :
									(whichMap == MapType.SDE2D ? new Point((int)Math.Round((FromSystem.X2D - previousSystem.X2D) / LY_SCALING), (int)Math.Round((FromSystem.Y2D - previousSystem.Y2D) / LY_SCALING)) :
									new Point((int)Math.Round((nextSolarSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((nextSolarSystem.DotlanY - FromSystem.DotlanY))))
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

                                if (showOutOfRegion == false && linkedSolarSystem.RegionName != FromSystem.RegionName) continue; // do not add to systems

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
										textDisplayConnecting = (linkedSolarSystem.ConnectedTo.Where(o => o.ToSystemId == FromSystem.Id).Count() > 0),
										X = (
							                whichMap == MapType.SDE ? (int)Math.Round((FromSystem.X - linkedSolarSystem.X) / LY_SCALING) :
							                (whichMap == MapType.SDE2D ? (int)Math.Round((FromSystem.X2D - linkedSolarSystem.X2D) / LY_SCALING) :
							                (int)Math.Round((linkedSolarSystem.DotlanX - FromSystem.DotlanX)))
							            ),
										Y = (
							                whichMap == MapType.SDE ? (int)Math.Round((FromSystem.Y - linkedSolarSystem.Y) / LY_SCALING) :
							                (whichMap == MapType.SDE2D ? (int)Math.Round((FromSystem.Y2D - linkedSolarSystem.Y2D) / LY_SCALING) :
											(int)Math.Round((linkedSolarSystem.DotlanY - FromSystem.DotlanY)))
										),
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

										P1 = (
									        whichMap == MapType.SDE ? new Point((int)Math.Round((FromSystem.X - mapSystem.solarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - mapSystem.solarSystem.Y) / LY_SCALING)) :
									        (whichMap == MapType.SDE2D ? new Point((int)Math.Round((FromSystem.X2D - mapSystem.solarSystem.X2D) / LY_SCALING), (int)Math.Round((FromSystem.Y2D - mapSystem.solarSystem.Y2D) / LY_SCALING)) :
											new Point((int)Math.Round((mapSystem.solarSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((mapSystem.solarSystem.DotlanY - FromSystem.DotlanY))))
										),

										P2 = (
									        whichMap == MapType.SDE ? new Point((int)Math.Round((FromSystem.X - linkedSolarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - linkedSolarSystem.Y) / LY_SCALING)) :
									        (whichMap == MapType.SDE2D ? new Point((int)Math.Round((FromSystem.X2D - linkedSolarSystem.X2D) / LY_SCALING), (int)Math.Round((FromSystem.Y2D - linkedSolarSystem.Y2D) / LY_SCALING)) :
											new Point((int)Math.Round((linkedSolarSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((linkedSolarSystem.DotlanY - FromSystem.DotlanY))))
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

                int sizeAdjust = (int)SYSTEM_CIRCLE_SIZE + (Math.Max(Math.Max(STORM_CIRCLE_SIZE, secLimitCloudSize), incursionCloudSize));

				foreach (var currentSystem in minimapSystems)
                {
                    maxX = Math.Max(maxX, currentSystem.X );
                    maxY = Math.Max(maxY, currentSystem.Y );
                    minX = Math.Min(minX, currentSystem.X );
                    minY = Math.Min(minY, currentSystem.Y );
                }

				maxX += sizeAdjust; maxY += sizeAdjust;
				minX -= sizeAdjust; minY -= sizeAdjust;
				if (minX < 0) { maxX += Math.Abs(minX); minX = 0; }
				if (minY < 0) { maxY += Math.Abs(minY); minY = 0; }

				//int maxSize = Math.Max((int)(maxX - minX), (int)(maxY - minY));
                //int pictureSize = Math.Min(srcImage.Width, srcImage.Height) - BORDER_WIDTH - TRANSPARENT_BORDER_WIDTH - (8);

				double GUI_SCALINGx = (double)(maxX - minX) / (srcImage.Width * (whichMap == MapType.SDE ? .9 : (whichMap == MapType.SDE2D ? .9 : .9)));
				double GUI_SCALINGy = (double)(maxY - minY)/ (srcImage.Height * (whichMap == MapType.SDE ? .9 : (whichMap == MapType.SDE2D ? .9 : .9)));
				GUI_SCALING = Math.Max(GUI_SCALINGx, GUI_SCALINGy);

				//GUI_SCALING = (int)Math.Round((double)(maxSize) / ((pictureSize * (whichMap == MapType.SDE ? .9 : (whichMap == MapType.SDE2D ? .9 : .9 )) )));
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

                float textSize = defaultTextSize == false ? (TEXT_SIZE / (float)GUI_SCALING) : MINIMUM_TEXT_SIZE;
                if (textSize < MINIMUM_TEXT_SIZE) textSize = MINIMUM_TEXT_SIZE;


			// draw incursion base if required
			if (minimapSystems.Where(o => o.solarSystem.Incursion).Count() > 0)
				{
					//g.CompositingMode = CompositingMode.SourceOver;
					//g.CompositingQuality = CompositingQuality.GammaCorrected;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;

					//Brush incursionBrush = new SolidBrush(IncursionColour);
					Brush incursionBrush = new HatchBrush(HatchStyle.Percent50, IncursionColour);

					Brush incursionOutlineBrush = new SolidBrush(IncursionOutlineColour);
                    Pen incursionOutlinePen = new Pen(incursionOutlineBrush, 3);
					float incursionBlobSize = ((float) incursionCloudSize / (float)GUI_SCALING);

					Image incursionImage = new Bitmap(srcImage.Width, srcImage.Height, PixelFormat.Format32bppArgb);

                    ColorMatrix incursionColourMatrix = new ColorMatrix();
					incursionColourMatrix.Matrix33 = (float)((float)incursionAlphaAmount / (float)100);  // alpha !
					ImageAttributes incursionImageAttr = new ImageAttributes();
                    incursionImageAttr.SetColorMatrix(incursionColourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

					using (Graphics gIncursion = Graphics.FromImage(incursionImage))
					{

						MapCircles(gIncursion, minimapSystems.Where(o => o.solarSystem.Incursion).ToList(), originPoint, incursionBlobSize, incursionOutlinePen, incursionBrush);
						g.DrawImage(
							incursionImage,
							new Rectangle(0, 0, incursionImage.Width, incursionImage.Height),
							0, 0, incursionImage.Width, incursionImage.Height,
							GraphicsUnit.Pixel,
							incursionImageAttr
							);
					}
				}

				//Seclimit ?
				if (showSecLimit && minimapSystems.Where(o => o.solarSystem.Security <= secLimit).Count() > 0)
				{
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					//Brush secLimitBrush = new SolidBrush(SecLimitColour);
					Brush secLimitBrush = new HatchBrush(HatchStyle.Percent30, SecLimitColour);

					Brush secLimitOutlineBrush = new SolidBrush(SecLimitOutlineColour);
					Pen secLimitOutlinePen = new Pen(secLimitOutlineBrush, 3);
					float secLimitBlobSize = ((float)secLimitCloudSize / (float)GUI_SCALING);
					Image secLimitImage = new Bitmap(srcImage.Width, srcImage.Height, PixelFormat.Format32bppArgb);

					ColorMatrix secLimitColourMatrix = new ColorMatrix();
					secLimitColourMatrix.Matrix33 = (float)((float)secLimitAlphaAmount / (float)100);  // alpha !
					ImageAttributes secLimitImageAttr = new ImageAttributes();
					secLimitImageAttr.SetColorMatrix(secLimitColourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

					using (Graphics gSecLimit = Graphics.FromImage(secLimitImage))
					{
						MapCircles(gSecLimit, minimapSystems.Where(o => o.solarSystem.Security <= secLimit).ToList(), originPoint, secLimitBlobSize, secLimitOutlinePen, secLimitBrush);

						g.DrawImage(
							secLimitImage,
							new Rectangle(0, 0, secLimitImage.Width, secLimitImage.Height),
							0, 0, secLimitImage.Width, secLimitImage.Height,
							GraphicsUnit.Pixel,
							secLimitImageAttr
							);
					}
				}


				// storm detection
				float circleSizeStorm = STORM_CIRCLE_SIZE / (float)GUI_SCALING;
				float blobSize = (SYSTEM_CIRCLE_SIZE / (float)GUI_SCALING);
				if (blobSize < MINIMUM_BLOB_SIZE || float.IsInfinity(blobSize))
				{
					blobSize = MINIMUM_BLOB_SIZE;
				}
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;

				ColorMatrix stormLimitColourMatrix = new ColorMatrix();
				ImageAttributes stormImageAttr = new ImageAttributes();


				Image stormLimitImage = new Bitmap(srcImage.Width, srcImage.Height, PixelFormat.Format32bppArgb);

				int stormAlphaAmount = 0;

                // Draw outlines first

                // go through systems used 3 times - draw weak first, then strong then centre
                for (int stormStrength = 0; stormStrength <= 2; stormStrength++)
                {
					stormLimitImage = new Bitmap(srcImage.Width, srcImage.Height, PixelFormat.Format32bppArgb);
					using (Graphics gStorm = Graphics.FromImage(stormLimitImage))
					{
						switch (stormStrength)
						{
							case 0: stormAlphaAmount = 90; break;
							case 1: stormAlphaAmount = 120; break;
							case 2: stormAlphaAmount = 155; break;
						}
						stormLimitColourMatrix.Matrix33 = (float)((float)stormAlphaAmount / (float)255);  // alpha !
                        stormImageAttr.SetColorMatrix(stormLimitColourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        foreach (var currentSystem in minimapSystems)
                        {
                            Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
                            foreach (var storm in Program.Universe.Storms)
                            {
								if (stormStrength == 0 && storm.WeakSystemIds != null && storm.WeakSystemIds.Contains(currentSystem.solarSystem.Id))
									DrawStorm(storm.StormType, "WEAK", gStorm, currentPoint, circleSizeStorm, blobSize, true);
								if (stormStrength == 1 && storm.StrongSystemIds != null && storm.StrongSystemIds.Contains(currentSystem.solarSystem.Id))
									DrawStorm(storm.StormType, "STRONG", gStorm, currentPoint, circleSizeStorm, blobSize, true);
								if (stormStrength == 2 && storm.SystemId == currentSystem.solarSystem.Id)
									DrawStorm(storm.StormType, "CENTRE", gStorm, currentPoint, circleSizeStorm, blobSize, true);
							}
						}
                        g.DrawImage(
                            stormLimitImage, // target
                            new Rectangle(0, 0, stormLimitImage.Width, stormLimitImage.Height), // source
                            0, 0, stormLimitImage.Width, stormLimitImage.Height,
                            GraphicsUnit.Pixel,
                            stormImageAttr
                            );
                    }
                }


				// go through systems used 3 times - draw weak first, then strong then centre
				for (int stormStrength = 0; stormStrength <= 2; stormStrength++)
				{
					// clear background 
					foreach (var currentSystem in minimapSystems)
					{
						Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
						foreach (var storm in Program.Universe.Storms)
						{
							if (stormStrength == 0 && storm.WeakSystemIds != null && storm.WeakSystemIds.Contains(currentSystem.solarSystem.Id))
								DrawStorm(storm.StormType, "EMPTY", g, currentPoint, circleSizeStorm, blobSize, false);
							if (stormStrength == 1 && storm.StrongSystemIds != null && storm.StrongSystemIds.Contains(currentSystem.solarSystem.Id))
								DrawStorm(storm.StormType, "EMPTY", g, currentPoint, circleSizeStorm, blobSize, false);
							if (stormStrength == 2 && storm.SystemId == currentSystem.solarSystem.Id)
								DrawStorm(storm.StormType, "EMPTY", g, currentPoint, circleSizeStorm, blobSize, false);
						}
					}

					stormLimitImage = new Bitmap(srcImage.Width, srcImage.Height, PixelFormat.Format32bppArgb);
					using (Graphics gStorm = Graphics.FromImage(stormLimitImage))
					{
						switch (stormStrength)
						{
							case 0: stormAlphaAmount = 90; break;
							case 1: stormAlphaAmount = 120; break;
							case 2: stormAlphaAmount = 155; break;
						}

						stormLimitColourMatrix.Matrix33 = (float)((float)stormAlphaAmount / (float)255);  // alpha !
						stormImageAttr.SetColorMatrix(stormLimitColourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

						foreach (var currentSystem in minimapSystems)
						{
							Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
							foreach (var storm in Program.Universe.Storms)
							{
								if (stormStrength == 0 && storm.WeakSystemIds != null && storm.WeakSystemIds.Contains(currentSystem.solarSystem.Id))
									DrawStorm(storm.StormType, "WEAK", gStorm, currentPoint, circleSizeStorm, blobSize, false);
								if (stormStrength == 1 && storm.StrongSystemIds != null && storm.StrongSystemIds.Contains(currentSystem.solarSystem.Id))
									DrawStorm(storm.StormType, "STRONG", gStorm, currentPoint, circleSizeStorm, blobSize, false);
								if (stormStrength == 2 && storm.SystemId == currentSystem.solarSystem.Id)
									DrawStorm(storm.StormType, "CENTRE", gStorm, currentPoint, circleSizeStorm, blobSize, false);
							}
						}
						g.DrawImage(
						   stormLimitImage, // target
						   new Rectangle(0, 0, stormLimitImage.Width, stormLimitImage.Height), // source
						   0, 0, stormLimitImage.Width, stormLimitImage.Height,
						   GraphicsUnit.Pixel,
						   stormImageAttr
						   );
					}
				}

			// draw connections
			float lineGap = (float)(connectingLineGap / GUI_SCALING); 
                if ( lineGap < 0) lineGap = 0.1f;
				foreach (var connection in minimapConnectingSystems)
                {
                    Point P1 = new Point(originPoint.X + connection.P1.X, originPoint.Y + connection.P1.Y);
                    Point P2 = new Point(originPoint.X + connection.P2.X, originPoint.Y + connection.P2.Y);
                    Pen linePen = new Pen(connection.route ? LineColourRoute : ((FromSystem.RegionName == connection.ToSystem.RegionName) ? LineColourNormal : LineColourOutOfRegion), 1);
                    float distance = (float) Math.Sqrt(Math.Pow((P1.X - P2.X), 2) + Math.Pow((P1.Y - P2.Y), 2)) - (lineGap*2);
                    if (distance < 0.1f) distance = 0.1f;
					linePen.DashPattern = new float[] { 0.1f, lineGap, distance, lineGap };
					g.DrawLine(linePen, P1, P2);
                }

                // draw system blobs
                foreach (var currentSystem in minimapSystems)
                {
                    int circlesUsed = -1;

                    Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);

                    Color systemBrushColour = SystemColourNormal;
                    if (currentSystem.route) systemBrushColour = SystemColourRoute;
                    if (currentSystem.fromSystem) systemBrushColour = SystemColourFrom;
                    if (currentSystem.toSystem) systemBrushColour = SystemColourTo;
                    Brush systemBrush = new SolidBrush(systemBrushColour);
                    g.FillEllipse(systemBrush, currentPoint.X - blobSize, currentPoint.Y - blobSize, blobSize * 2, blobSize * 2);

					// How about mercoxit ?
					if (currentSystem.solarSystem.Security < MERCOXIT_SECURITY)
					{
						DrawSystemName(g, currentPoint.X-10, currentPoint.Y-3, "✦", MercoxitColour, textSize);
					}

					// do we have ice !
					if (currentSystem.solarSystem.IceField == true)
					{
						DrawSystemName(g, currentPoint.X + 10, currentPoint.Y - 3, "❅", IceColour, textSize);
					}


                    // Check skyhooks
                    bool skyhooksVulnerable = false;
                    bool skyhooksVulnerableSoon = false;

					float addX = (float)(6 + (blobSize / 2) / GUI_SCALING);
					float addY = (float)(20 + (blobSize / 2) / GUI_SCALING);

                    float skyhookTextSize = (float)(10 / GUI_SCALING);

					uint animationCycleSkyhooks = 10;

					DateTime utcNow = DateTime.UtcNow;
                    foreach (var p in currentSystem.solarSystem.Planets.Where(o => o.SkyhookVulneralbilityStart != null && o.SkyhookVulneralbilityEnd >= utcNow) )
                    {
						if (p.SkyhookVulneralbilityStart > utcNow)
							skyhooksVulnerableSoon = true;
						if (p.SkyhookVulneralbilityStart <= utcNow)
							skyhooksVulnerable = true;
					}

					if (skyhooksVulnerableSoon)
					{
						/*
												var circlePen = new Pen(Color.Orange, 1);
												float[] dashSet = { 4, 8, 4, 8 };
												circlePen.DashPattern = dashSet;
												g.DrawArc(circlePen,
													currentPoint.X + addX - (circleSizeLater/2) - ((circleSizeLater - circleSizeNow)/2), currentPoint.Y - addY - circleSizeLater,
													circleSizeLater * 2, circleSizeLater * 2,
													20 * (float)animationCycleSkyhooks, 360);
						*/
						DrawSystemName(g, currentPoint.X + (int)addX, currentPoint.Y - (int)addY - (int)skyhookTextSize, "S", Color.Orange, skyhookTextSize);
					}

					if (skyhooksVulnerable)
                    {
						/*
												var circlePen = new Pen(Color.LightSeaGreen, 1);
												float[] dashSet = { 6, 8, 6, 8 };
												circlePen.DashPattern = dashSet;
												g.DrawArc(circlePen,
													currentPoint.X + addX - (circleSizeNow/2), currentPoint.Y - addY - circleSizeNow,
													circleSizeNow * 2, circleSizeNow * 2,
													20 * (float)animationCycleSkyhooks, 360);
						*/
						DrawSystemName(g, currentPoint.X + (int)addX, currentPoint.Y - (int)addY - (int)skyhookTextSize, "S", Color.Green, skyhookTextSize);
					}

					// Unihole ?
					if (currentSystem.solarSystem.UniHole == true)
					{
						var circlePen = new Pen(UniHoleColour, 2);
						float circleSize = 0;

						if (animationCycleUniHoles is null)
						{
							g.DrawEllipse(circlePen,
								currentPoint.X - (blobSize + circleSize),
								currentPoint.Y - (blobSize + circleSize),
								(blobSize + circleSize) * 2,
								(blobSize + circleSize) * 2);
						}
						else
						{
							float[] dashSet = { 10, 4, 4, 4};
							//circlePen.DashStyle = DashStyle.DashDot;
							circlePen.DashPattern = dashSet;
							g.DrawArc(circlePen,
								//currentPoint.X - (blobSize + (circleSize) + (float)animationCycleAlts), currentPoint.Y - (blobSize + (circleSize) + (float)animationCycleAlts),
								currentPoint.X - (blobSize + circleSize), currentPoint.Y - (blobSize + circleSize),
								//(blobSize + circleSize + (float)animationCycleAlts) * 2, (blobSize + circleSize + (float)animationCycleAlts) * 2,
								(blobSize + circleSize) * 2, (blobSize + circleSize) * 2,
								20 * (float)animationCycleUniHoles, 360);
						}
					}

					// DrawSystemName(g, currentPoint.X, currentPoint.Y, $"{currentSystem.solarSystem.NPCKillsDeltaLastHour}", Color.AliceBlue, textSize); 

					if (currentSystem.solarSystem.CountKills() > 0)
                    {
                        var circlePen = new Pen(ZKillColour,2);
                        circlesUsed++;
                        float circleSize = CIRCLE_INITIAL_GAP + (CIRCLE_INCREASE_GAP * circlesUsed) / (float)GUI_SCALING;
                        if (circleSize < MINIMUM_CIRCLE_SIZE || float.IsInfinity(circleSize))
                        {
                            circleSize = MINIMUM_CIRCLE_SIZE;
                        }

                        if (animationCycleKills is null)
                        {
                            g.DrawEllipse(circlePen,
                                currentPoint.X - (blobSize + circleSize),
                                currentPoint.Y - (blobSize + circleSize),
                                (blobSize + circleSize) * 2,
                                (blobSize + circleSize) * 2);
                        }
                        else
                        {
                            float[] dashSet = { 1, 1, 2, 2, 4, 4 };
                            //circlePen.DashStyle = DashStyle.DashDot;
                            circlePen.DashPattern = dashSet;
                            g.DrawArc(circlePen,
                                currentPoint.X - (blobSize + circleSize), currentPoint.Y - (blobSize + circleSize),
                                (blobSize + circleSize) * 2, (blobSize + circleSize) * 2,
                                30 * (float)animationCycleIntel, 360);
                        }

                        try
                        {
                            foreach (var k in currentSystem.solarSystem.GetKills())
                            {
                                sbExtraInformation.AppendLine($"ZKILL {k.Timestamp.ToString("HH:mm:ss")} {k.SolarSystemName} {k.VictimShipName} {k.VictimCharacterName}");
                            }
                        }
                        catch (Exception ex)
                        {
                        }


					}

					if (currentSystem.solarSystem.CountAlerts(false, false, false) > 0)
					{
                        var circlePen = new Pen(IntelColour,2);
                        circlesUsed++;
                        float circleSize = CIRCLE_INITIAL_GAP + (CIRCLE_INCREASE_GAP * circlesUsed) / (float)GUI_SCALING;
                        if ( circleSize < MINIMUM_CIRCLE_SIZE || float.IsInfinity(circleSize))
                        {
                            circleSize = MINIMUM_CIRCLE_SIZE;
                        }
                        if (animationCycleIntel is null)
                        {
                            g.DrawEllipse(circlePen,
                                currentPoint.X - (blobSize + circleSize),
                                currentPoint.Y - (blobSize + circleSize),
                                (blobSize + circleSize) * 2,
                                (blobSize + circleSize) * 2);
                        }
                        else
                        {
                            float[] dashSet = { 4, 4, 2, 2, 1, 1 };
                            //circlePen.DashStyle = DashStyle.DashDot;
                            circlePen.DashPattern = dashSet;
                            g.DrawArc(circlePen,
                                currentPoint.X - (blobSize + circleSize), currentPoint.Y - (blobSize + circleSize),
                                (blobSize + circleSize) * 2, (blobSize + circleSize) * 2,
                                20 * (float)animationCycleIntel, 360);
                        }

                        foreach (var a in currentSystem.solarSystem.GetAlerts(false,false,false))
                        {
                            sbExtraInformation.AppendLine($"{a.Entry.LogTime} {a.Entry.LineContent}");
                        }
                    }

                    // active alt in sytstem ?
                    if (Program.CharacterList.Where(o => o.IsClientActive && o.IsCurrentlActiveClient == false && o.LocationId == currentSystem.solarSystem.Id).Count() > 0)
                    {
                        var circlePen = new Pen(AltColour,2);
                        circlesUsed++;
                        float circleSize = CIRCLE_INITIAL_GAP + (CIRCLE_INCREASE_GAP * circlesUsed) / (float)GUI_SCALING;
                        if (circleSize < MINIMUM_CIRCLE_SIZE || float.IsInfinity(circleSize))
                        {
                            circleSize = MINIMUM_CIRCLE_SIZE;
                        }

                        if (animationCycleAlts is null)
                        {
                            g.DrawEllipse(circlePen, 
                                currentPoint.X - (blobSize + circleSize), 
                                currentPoint.Y - (blobSize + circleSize),
                                (blobSize + circleSize) * 2, 
                                (blobSize + circleSize) * 2);
                        }
                        else
                        {
                            float[] dashSet = { 4,4,2,2,1,1 };
                            circlePen.DashPattern = dashSet;
                            g.DrawArc(circlePen,
                                currentPoint.X - (blobSize + circleSize) , currentPoint.Y - (blobSize + circleSize),
                                (blobSize + circleSize) * 2, (blobSize + circleSize) * 2, 
                                10 * (float)animationCycleAlts, 360);
                        }
                    }
                }

                // show all systems names ?
                if (showAllSystemsText)
                {
                    foreach (var currentSystem in minimapSystems.Where(o => ! o.textDisplayConnecting && ! o.textDisplay))
                    {
						Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y); 
                        DrawSystemName(g, currentPoint.X, currentPoint.Y, currentSystem.solarSystem.Name, SystemColourAllText, textSize / 3 * 2, false);
					}
				}

				// show all connecting systems names
				if (showConnectingSystemsText)
				{
					foreach (var currentSystem in minimapSystems.Where(o => o.textDisplayConnecting && !o.textDisplay))
					{
						Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
						DrawSystemName(g, currentPoint.X, currentPoint.Y, currentSystem.solarSystem.Name, SystemColourConnectingText, textSize / 4 * 3, false);
					}
				}

				// show systems name
				if (showSystemText)
				{
					foreach (var currentSystem in minimapSystems.Where(o => o.textDisplay))
					{
						Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
						DrawSystemName(g, currentPoint.X, currentPoint.Y, currentSystem.solarSystem.Name, SystemColourText, textSize);
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

        private static void AddLogoImage(Graphics g, Image srcLogo, RectangleF Outside, RectangleF Inside, Color FillColour, Color LineColour, bool drawBox)
        {
            if (drawBox)
            {
                Brush brFill = new SolidBrush(FillColour);
                Pen penLine = new Pen(LineColour);
                g.FillRectangle(brFill, Outside);
                g.DrawRectangle(penLine, Outside.Left, Outside.Top, Outside.Width, Outside.Height);
            }

			g.DrawImage(srcLogo, Inside);
		}

		// makes nice round ellipse/circle images from rectangle images
		public static Image ClipToCircle(Image srcImage, PointF center, float radius, Color backGround, Color circleColour, int circleWidth, string isk, Color iskColour, bool showBar, Color barColor, Image srcAgressorLogo, Image srcVictimLogo, int iskSize, int logoSize)
        {
            Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, srcImage.PixelFormat);

            using (Graphics g = Graphics.FromImage(dstImage))
            {
                // Border
                RectangleF r2 = new RectangleF(center.X - radius, center.Y - radius, (radius * 2), (radius * 2));

                // Main picture
                RectangleF r = new RectangleF(center.X - radius + circleWidth, center.Y - radius + circleWidth, (radius * 2) - (circleWidth * 2), (radius * 2) - (circleWidth * 2));

				RectangleF rISK = new RectangleF(0, 0, srcImage.Width, srcImage.Height);

				// top left 
				//new RectangleF(1, 1, logoSize, logoSize); 
				//new RectangleF(3, 3, logoSize - 2, logoSize - 2);

				// top right
				//new RectangleF(srcImage.Width - logoSize, 1, logoSize, logoSize);
				//new RectangleF(srcImage.Width - logoSize + 2, 3, logoSize - 2, logoSize - 2);

				RectangleF rVictimLogoOutside = new RectangleF(dstImage.Width - logoSize, 1, logoSize, logoSize);
				RectangleF rVictimLogoInside = new RectangleF(dstImage.Width - logoSize + 2, 3, logoSize - 2, logoSize - 2);

				RectangleF rAgressorLogoOutside = new RectangleF(1, dstImage.Height - iskSize - logoSize + 30, logoSize, logoSize);
				RectangleF rAgressorLogoInside = new RectangleF(3, dstImage.Height - iskSize - logoSize + 32, logoSize - 2, logoSize - 2);

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
                        // Using WIDTH for both makes image square/equal - thus allowing greater height to make isk clearer
                        g.FillRectangle(br, 0, 0, dstImage.Width, dstImage.Width);
                    }
                }

                // adds the new ellipse & draws the image again 
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(r);
                g.SetClip(path);
                g.DrawImage(srcImage, 0, 0);
				g.ResetClip();

                using (Font myfont = new Font("Arial", iskSize - 20, FontStyle.Bold))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Far,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.None,
                    };
                    if (showBar) g.FillRectangle(new SolidBrush(barColor), 0, dstImage.Height - iskSize, dstImage.Width, iskSize);
                    g.DrawString(isk, myfont, new SolidBrush(iskColour), new RectangleF(0, (dstImage.Height - iskSize), dstImage.Width, iskSize), format);
                }

				if (srcAgressorLogo != null)
				{
					AddLogoImage(g, srcAgressorLogo, rAgressorLogoOutside, rAgressorLogoInside, Color.Black, Color.White, true);
				}
				if (srcVictimLogo != null)
				{
					AddLogoImage(g, srcVictimLogo, rVictimLogoOutside, rVictimLogoInside, Color.Black, Color.White, true);
				}

				return dstImage;
            }
        }

		public static void PaintDrawText(Graphics g, System.Windows.Forms.Label l)
		{
            //if (l.Visible == false) return; 
			var flags = TextFormatFlags.Right;
			if (l.TextAlign == ContentAlignment.TopLeft || l.TextAlign == ContentAlignment.BottomLeft || l.TextAlign == ContentAlignment.MiddleLeft) flags = TextFormatFlags.Left;
			flags = flags | TextFormatFlags.WordBreak;

			g.TextRenderingHint = TextRenderingHint.AntiAlias;

			TextRenderer.DrawText(g, l.Text, l.Font, new Rectangle(l.Left, l.Top, l.Width, l.Height), l.ForeColor, flags);
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
