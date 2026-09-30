using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Data;
using Microsoft.CodeAnalysis;
using System.Drawing.Imaging;
using Quesadilla.Classes.Configuration;

namespace Quesadilla.Classes.ImageGeneration
{
	public class ImageGenerator
	{
		private const int BORDER_WIDTH = 1;
		private const int TEXT_SIZE = 20;

		private const double MERCOXIT_SECURITY = -0.75;

		private const int CIRCLE_INITIAL_GAP = 3;
		private const int CIRCLE_INCREASE_GAP = 7;

		private const double LY_SCALING = 9460000000000;

		private const int CIRCLE_RADIUS = 3;
		private const int SYSTEM_CIRCLE_SIZE = 6;

		private const float MINIMUM_TEXT_SIZE = 9;
		private const int MINIMUM_BLOB_SIZE = 3;
		private const float MINIMUM_CIRCLE_SIZE = 2.3f;

		public static void DrawSystemName(Graphics g, int X, int Y, string text, Color c, float textSize = MINIMUM_TEXT_SIZE, bool enforceMinimumSize = true)
		{
			// ensure we do not go too small
			if (enforceMinimumSize && (textSize < MINIMUM_TEXT_SIZE || float.IsInfinity(textSize))) textSize = MINIMUM_TEXT_SIZE;
			if (float.IsInfinity(textSize)) return;
			using (Font myfont = new Font("Verdana", textSize, FontStyle.Regular))
			{
				g.TextRenderingHint = TextRenderingHint.AntiAlias;

				var stringSize = g.MeasureString(text, myfont);

				g.DrawString(text, myfont, GDICache.GetBrush(c),
					X - (stringSize.Width / 2),
					Y + CIRCLE_RADIUS + 2
					);
			}
		}

		public static void DrawStorm(string stormType, string location, Graphics g, Point currentPoint, float circleSize, float blobSize, bool outline, int glowLayers)
		{
			Color stormFillColour;
			switch (stormType)
			{
				case "Electric":
					stormFillColour = Color.DodgerBlue;
					break;

				case "Plasma":
					stormFillColour = Color.DeepPink;
					break;

				case "Gamma":
					stormFillColour = Color.MediumOrchid;
					break;

				case "Exotic":
					stormFillColour = Color.LimeGreen;
					break;

				default:
					stormFillColour = Color.White;
					break;
			}

			Color severityColour;
			int baseAlpha;

			switch (location)
			{
				case "CENTRE":
					severityColour = Color.Crimson;
					baseAlpha = 255;
					break;

				case "STRONG":
					severityColour = Color.DarkOrange;
					baseAlpha = 160;
					break;

				case "WEAK":
					severityColour = Color.Gold;
					baseAlpha = 80;
					break;

				default:
					severityColour = Color.Black;
					baseAlpha = 255; // 100%
					break;
			}

			Pen circlePen = GDICache.GetPen(severityColour, 4);

			float radius = blobSize + circleSize;
			float diameter = radius * 2;

			RectangleF rect = new RectangleF(
				currentPoint.X - radius,
				currentPoint.Y - radius,
				diameter,
				diameter);

			if (outline)
			{
				//
				// Glow based on severity
				//
				for (int i = glowLayers; i >= 1; i--)
				{
					float glowRadius = radius + (i * 3);

					int alpha = 120 / i;

					using (var glowBrush = new SolidBrush(Color.FromArgb(alpha, severityColour)))
					{
						g.FillEllipse(glowBrush,currentPoint.X - glowRadius,currentPoint.Y - glowRadius,glowRadius * 2,glowRadius * 2);
					}
				}

				g.DrawEllipse(circlePen, rect);
			}
			else
			{
				using (var circleBrush =
					new SolidBrush(Color.FromArgb(baseAlpha, (severityColour == Color.Black ? severityColour : stormFillColour))))
				{
					g.FillEllipse(circleBrush, rect);
				}
			}
		}

		private static void MapCircles(
			Graphics g,
			IReadOnlyList<MinimapSystem> systems,
			Point originPoint,
			float size,
			Pen outlinePen,
			Brush fillBrush,
			Color glowColor,
			int? animationCycle)
		{
			Brush eraserBrush = GDICache.GetBrush(Color.Black);

			float diameter = size * 2;

			// Pulse value 0 -> 1
			float pulse = 0f;

			if (animationCycle.HasValue)
			{
				pulse = (animationCycle.Value % 30) / 30f;
			}

			// glow
			foreach (var system in systems)
			{
				float centerX = originPoint.X + system.X;
				float centerY = originPoint.Y + system.Y;

				for (int layer = 8; layer >= 1; layer--)
				{
					float layerRadius =
						size +
						(layer * 2.0f) +
						(pulse * 4.0f);

					int alpha = (int)(
						(80.0f / layer) *
						(0.75f + (float)Math.Sin(pulse * Math.PI) * 0.25f));

					using (var glowBrush = new SolidBrush(
						Color.FromArgb(
							Math.Min(255, Math.Max(1, alpha)),
							glowColor)))
					{

						float glowDiameter = layerRadius * 2;

						g.FillEllipse(
							glowBrush,
							centerX - layerRadius,
							centerY - layerRadius,
							glowDiameter,
							glowDiameter);
					}
				}
			}

			// outlines
			foreach (var system in systems)
			{
				g.DrawEllipse(
					outlinePen,
					originPoint.X + system.X - size,
					originPoint.Y + system.Y - size,
					diameter,
					diameter);
			}

			// tidy up
			foreach (var system in systems)
			{
				g.FillEllipse(
					eraserBrush,
					originPoint.X + system.X - size,
					originPoint.Y + system.Y - size,
					diameter,
					diameter);
			}

			// fills
			foreach (var system in systems)
			{
				g.FillEllipse(
					fillBrush,
					originPoint.X + system.X - size,
					originPoint.Y + system.Y - size,
					diameter,
					diameter);
			}
		}


		private static void MapWormhole(
			Graphics g,
			IReadOnlyList<MinimapSystem> systems,
			Point originPoint,
			float size,
			Pen outlinePen,
			Brush fillBrush,
			Color glowColor,
			int? animationCycle)
		{
			Brush eraserBrush = GDICache.GetBrush(Color.Black);

			// Animation phase
			float phase = animationCycle.HasValue
				? (animationCycle.Value % 120) / 120f * (float)(Math.PI * 2.0)
				: 0f;

			// Number of points making up the wormhole outline.
			// More points = smoother.
			const int pointCount = 64;

			// Number of ripples around the circumference.
			const float wiggleCount = 6f;

			// Amount that the edge moves in/out.
			float wiggleAmount = Math.Max(1.5f, size * 0.18f);

			// glow
			foreach (var system in systems)
			{
				float centerX = originPoint.X + system.X;
				float centerY = originPoint.Y + system.Y;

				for (int layer = 8; layer >= 1; layer--)
				{
					float layerRadius = size + (layer * 2.0f);
					int alpha = (int)( (80.0f / layer) * (0.75f + (float)Math.Sin(phase * 0.5f) * 0.25f));

					alpha = Math.Min(255, Math.Max(1, alpha));

					PointF[] glowPoints = CreateWormholePoints(centerX,centerY,layerRadius,wiggleAmount,wiggleCount,phase,pointCount);

					using (var glowBrush = new SolidBrush(Color.FromArgb(alpha, glowColor)))
						g.FillPolygon(glowBrush, glowPoints);
				}
			}

			// outlines
			foreach (var system in systems)
			{
				float centerX = originPoint.X + system.X;
				float centerY = originPoint.Y + system.Y;

				PointF[] points = CreateWormholePoints(centerX,centerY,size,wiggleAmount,wiggleCount,phase,pointCount);
				g.DrawClosedCurve(outlinePen,points,0.35f,System.Drawing.Drawing2D.FillMode.Winding);
			}

			// tidy up insides
			foreach (var system in systems)
			{
				float centerX = originPoint.X + system.X;
				float centerY = originPoint.Y + system.Y;

				PointF[] points = CreateWormholePoints(centerX,centerY,size,wiggleAmount,wiggleCount,phase,pointCount);
				g.FillClosedCurve(eraserBrush,points,System.Drawing.Drawing2D.FillMode.Winding,0.35f);
			}

			// fill
			foreach (var system in systems)
			{
				float centerX = originPoint.X + system.X;
				float centerY = originPoint.Y + system.Y;

				PointF[] points = CreateWormholePoints(centerX,centerY,size,wiggleAmount,wiggleCount,phase,pointCount);
				g.FillClosedCurve(fillBrush,points,System.Drawing.Drawing2D.FillMode.Winding,0.35f);
			}
		}

		private static PointF[] CreateWormholePoints(
			float centerX,
			float centerY,
			float radius,
			float wiggleAmount,
			float wiggleCount,
			float phase,
			int pointCount)
		{
			// full disclosure - I 100% used copilot to help with the maths here

			PointF[] points = new PointF[pointCount];

			for (int i = 0; i < pointCount; i++)
			{
				float angle = ((float)i / pointCount) * (float)(Math.PI * 2.0);

				//
				// Primary animated ripple.
				//
				float wiggle = (float)Math.Sin(angle * wiggleCount + phase);

				//
				// Secondary ripple prevents it looking like a
				// mathematically perfect flower/star.
				//
				wiggle += (float)Math.Sin(angle * (wiggleCount + 3.0f) - phase * 1.7f) * 0.35f;

				float r = radius + wiggle * wiggleAmount;

				points[i] = new PointF(centerX + (float)Math.Cos(angle) * r, centerY + (float)Math.Sin(angle) * r);
			}

			return points;
		}

		public static void DrawPulseOrb(Graphics g, Point center, Color colour, float radius, int? animationCycle, int glowLayers = 4, float pulseSize = 3.0f, int maxAlpha = 100)
		{
			float pulse = 0;
			if (animationCycle.HasValue)
			{
				pulse = (animationCycle.Value % 20) / 20.0f;
			}

			float expansion = pulse * pulseSize;
			int alpha = (int)(maxAlpha * (1.0f - pulse));

			// glow
			for (int i = glowLayers; i >= 1; i--)
			{
				using (Brush glowBrush =
					new SolidBrush(Color.FromArgb(Math.Max(1, alpha / (i * 2)), colour)))
				{
					float glowRadius = radius + expansion + (i * 2);
					float glowDiameter = glowRadius * 2;
					g.FillEllipse(glowBrush, center.X - glowRadius, center.Y - glowRadius, glowDiameter, glowDiameter);
				}
			}

			// core orb
			float diameter = radius * 2;
			g.FillEllipse(GDICache.GetBrush(colour), center.X - radius, center.Y - radius, diameter, diameter);
		}

		private static void DrawPulseIndicator(Graphics g, Point center, Color colour, float baseRadius, int? animationCycle, float pulseSize = 10.0f, int maxAlpha = 100, int glowLayers = 3, float ringWidth = 2.0f)
		{
			RectangleF rect = new RectangleF(
				center.X - baseRadius,
				center.Y - baseRadius,
				baseRadius * 2,
				baseRadius * 2);

			// Static ring if animation disabled
			if (animationCycle == null)
			{
				Pen ringPen = GDICache.GetPen(colour, ringWidth, DashStyle.Solid);
				g.DrawEllipse(ringPen, rect);
				return;
			}

			float pulse = ((float)animationCycle.Value % 20) / 20.0f;

			float expansion = pulse * pulseSize;

			int alpha = (int)(maxAlpha * (1.0f - pulse));

			// Glow
			for (int i = glowLayers; i >= 1; i--)
			{
				using (Pen glowPen = new Pen(Color.FromArgb(Math.Max(1, alpha / (i * 2)), colour), ringWidth + (i * 2)))
				{
					glowPen.LineJoin = LineJoin.Round;
					g.DrawEllipse(glowPen, rect.X - expansion, rect.Y - expansion, rect.Width + (expansion * 2), rect.Height + (expansion * 2));
				}
			}

			// Permanent ring
			Pen mainPen = GDICache.GetPen(colour, ringWidth, DashStyle.Solid);
			g.DrawEllipse(mainPen, rect);
		}

		private static void DrawPulseIndicatorLine(Graphics g,Point center,Color colour,float baseRadius,int? animationCycle,float pulseSize = 2.0f,int maxAlpha = 120,float ringWidth = 2.0f)
		{
			RectangleF innerRect = new RectangleF(
				center.X - baseRadius,
				center.Y - baseRadius,
				baseRadius * 2,
				baseRadius * 2);

			if (animationCycle == null)
			{
				Pen ringPen = GDICache.GetPen(colour, ringWidth, DashStyle.Solid);
				g.DrawEllipse(ringPen, innerRect);
				return;
			}

			float radius = baseRadius + (animationCycle.Value * pulseSize);

			int alpha = Math.Max(0,maxAlpha - (animationCycle.Value * 6));

			//
			// Slowly rotate the tri-circle spokes to give a sense of motion
			//
			float angleOffset =(animationCycle.Value * 15f) % 120f;

			/*
						//
						// Outer pulse glow
						//
						for (int glow = 4; glow >= 1; glow--)
						{
							int glowAlpha = Math.Max(1,alpha / (glow * 2));

							using (Pen glowPen = new Pen(Color.FromArgb(glowAlpha, colour),ringWidth + glow))
							{
								float grow = glow * 2;

								g.DrawEllipse(
									glowPen,
									center.X - radius - grow,
									center.Y - radius - grow,
									(radius + grow) * 2,
									(radius + grow) * 2);
							}
						}

						//
						// Main pulse ring
						//
						using (Pen pulsePen = new Pen(Color.FromArgb(Math.Max(1, alpha),colour),ringWidth))
						{
							g.DrawEllipse(
								pulsePen,
								center.X - radius,
								center.Y - radius,
								radius * 2,
								radius * 2);
						}
			*/


			//
			// Motion blur trail
			//
			const int trailSteps = 8;
			const float trailAngleStep = 6f;

			for (int trail = trailSteps; trail >= 1; trail--)
			{
				float trailRotation = angleOffset - (trail * trailAngleStep);
				int trailAlpha =Math.Max(1,alpha / (trail * 3));
				float trailWidth =1f + ((trailSteps - trail) * 0.7f);

				using (Pen trailPen = new Pen(Color.FromArgb(trailAlpha, colour),trailWidth))
				{
					for (int spoke = 0; spoke < 3; spoke++)
					{
						float angle = trailRotation + (spoke * 120f);
						float radians = angle * (float)Math.PI / 180f;

						PointF end = new PointF(
							center.X + (float)Math.Cos(radians) * radius,
							center.Y + (float)Math.Sin(radians) * radius);

						g.DrawLine(trailPen,center,end);
					}
				}
			}

			//
			// Spoke glow
			//
			for (int glow = 4; glow >= 1; glow--)
			{
				int glowAlpha =Math.Max(1,alpha / (glow * 2));
				using (Pen glowPen = new Pen(Color.FromArgb(glowAlpha, colour),2f + (glow * 2)))
				{
					for (int spoke = 0; spoke < 3; spoke++)
					{
						float angle =angleOffset + (spoke * 120f);
						float radians = angle * (float)Math.PI / 180f;

						PointF end = new PointF(
							center.X + (float)Math.Cos(radians) * radius,
							center.Y + (float)Math.Sin(radians) * radius);

						g.DrawLine(glowPen,center,end);
					}
				}
			}

			//
			// Main spokes
			//
			using (Pen spokePen = new Pen(Color.FromArgb(Math.Max(1, alpha),colour),3f))
			{
				for (int spoke = 0; spoke < 3; spoke++)
				{
					float angle = angleOffset + (spoke * 120f);
					float radians = angle * (float)Math.PI / 180f;

					PointF end = new PointF(
						center.X + (float)Math.Cos(radians) * radius,
						center.Y + (float)Math.Sin(radians) * radius);

					g.DrawLine(spokePen,center,end);
				}
			}

			//
			// Permanent centre ring
			//
			Pen mainPen =GDICache.GetPen(colour,ringWidth,DashStyle.Solid);
			g.DrawEllipse(mainPen, innerRect);
		}

		public static Image ClipMiniMap(int width, int height, PathInfo path, int mapScale,
			ref string extraInformation, ref List<MinimapSystem> returnMinimapSystems,
			int? animationCycleIntel,
			int? animationCycleKills,
			int? animationCycleAlts,
			int? animationCycleUniHoles,
			int? animationIncursion,
			int? animationCycleSecLimit,
			int? animationCycleSkyhooks,
			int? animationCycleWormhole,
			MinimapParameters parameters
			)
		{
			double GUI_SCALING = 30;

			StringBuilder sbExtraInformation = new StringBuilder();

			Point center = new Point((width / 2) - 1, (height / 2) - 1);

			// quick note on x/y coord calcs
			// dotlan calcs need to be inverted to make sure map displays correctly - not an error

			Image dstImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

			using (Graphics g = Graphics.FromImage(dstImage))
			{
				g.Clear(Color.Transparent);

				if (parameters.ShowBorder)
				{
					Brush br = GDICache.GetBrush(parameters.BorderColor);
					Pen p = GDICache.GetPen(parameters.BorderColor, 2);
					g.DrawRectangle(p, 0, 0, dstImage.Width, dstImage.Height);
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

					if (!minimapSystems.Any(o => o.solarSystem.Name == nextSolarSystem.Name))
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
							parameters.MapType == MapType.SDE ? (int)Math.Round((FromSystem.X - nextSolarSystem.X) / LY_SCALING) :
							(parameters.MapType == MapType.SDE2D ? (int)Math.Round((FromSystem.X2D - nextSolarSystem.X2D) / LY_SCALING) :
							(int)Math.Round((nextSolarSystem.DotlanX - FromSystem.DotlanX)))
							),
							Y = (
							parameters.MapType == MapType.SDE ? (int)Math.Round((FromSystem.Y - nextSolarSystem.Y) / LY_SCALING) :
							(parameters.MapType == MapType.SDE2D ? (int)Math.Round((FromSystem.Y2D - nextSolarSystem.Y2D) / LY_SCALING) :
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
									parameters.MapType == MapType.SDE ? new Point((int)Math.Round((FromSystem.X - previousSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - previousSystem.Y) / LY_SCALING)) :
									(parameters.MapType == MapType.SDE2D ? new Point((int)Math.Round((FromSystem.X2D - previousSystem.X2D) / LY_SCALING), (int)Math.Round((FromSystem.Y2D - previousSystem.Y2D) / LY_SCALING)) :
									new Point((int)Math.Round((previousSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((previousSystem.DotlanY - FromSystem.DotlanY))))
								),
								P2 = (
									parameters.MapType == MapType.SDE ? new Point((int)Math.Round((FromSystem.X - nextSolarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - nextSolarSystem.Y) / LY_SCALING)) :
									(parameters.MapType == MapType.SDE2D ? new Point((int)Math.Round((FromSystem.X2D - previousSystem.X2D) / LY_SCALING), (int)Math.Round((FromSystem.Y2D - previousSystem.Y2D) / LY_SCALING)) :
									new Point((int)Math.Round((nextSolarSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((nextSolarSystem.DotlanY - FromSystem.DotlanY))))
								),
							});
						}
						previousSystem = nextSolarSystem;

					}
				}

				// should we use connecting systems too ?
				if (parameters.CloseSystems > 0)
				{
					List<MinimapSystem> minimapSystemsAdditional = new List<MinimapSystem>();
					for (int Counter = 0; Counter < parameters.CloseSystems; Counter++)
					{
						foreach (var mapSystem in minimapSystems)
						{
							foreach (var connection in mapSystem.solarSystem.ConnectedTo)
							{
								var linkedSolarSystem = Program.Universe.SolarSystems[connection.ToSystemId];

								if (parameters.ShowOutOfRegion == false && linkedSolarSystem.RegionName != FromSystem.RegionName) continue; // do not add to systems

								var linkedMinimapSystem = minimapSystems.FirstOrDefault(o => o.solarSystem.Id == linkedSolarSystem.Id);

								if (!minimapSystems.Any(o => o.solarSystem.Name == linkedSolarSystem.Name) &&
									!minimapSystemsAdditional.Any(o => o.solarSystem.Name == linkedSolarSystem.Name))
								{
									minimapSystemsAdditional.Add(new MinimapSystem()
									{
										solarSystem = linkedSolarSystem,
										fromSystem = false,
										toSystem = false,
										route = false,
										textDisplay = false,
										textDisplayConnecting = (linkedSolarSystem.ConnectedTo.Any(o => o.ToSystemId == FromSystem.Id)),
										X = (
											parameters.MapType == MapType.SDE ? (int)Math.Round((FromSystem.X - linkedSolarSystem.X) / LY_SCALING) :
											(parameters.MapType == MapType.SDE2D ? (int)Math.Round((FromSystem.X2D - linkedSolarSystem.X2D) / LY_SCALING) :
											(int)Math.Round((linkedSolarSystem.DotlanX - FromSystem.DotlanX)))
										),
										Y = (
											parameters.MapType == MapType.SDE ? (int)Math.Round((FromSystem.Y - linkedSolarSystem.Y) / LY_SCALING) :
											(parameters.MapType == MapType.SDE2D ? (int)Math.Round((FromSystem.Y2D - linkedSolarSystem.Y2D) / LY_SCALING) :
											(int)Math.Round((linkedSolarSystem.DotlanY - FromSystem.DotlanY)))
										),
									});
								}

								// check if connection already saved
								if (
									(!minimapConnectingSystems.Any(o =>
									  (o.FromSystem.Id == mapSystem.solarSystem.Id && o.ToSystem.Id == connection.ToSystemId) ||
									  (o.ToSystem.Id == mapSystem.solarSystem.Id && o.FromSystem.Id == connection.ToSystemId)) )
									)
								{
									minimapConnectingSystems.Add(new MinimapConnection()
									{
										FromSystem = mapSystem.solarSystem,
										ToSystem = linkedSolarSystem,
										route = mapSystem.route && linkedMinimapSystem?.route == true,
										P1 = (
											parameters.MapType == MapType.SDE ? new Point((int)Math.Round((FromSystem.X - mapSystem.solarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - mapSystem.solarSystem.Y) / LY_SCALING)) :
											(parameters.MapType == MapType.SDE2D ? new Point((int)Math.Round((FromSystem.X2D - mapSystem.solarSystem.X2D) / LY_SCALING), (int)Math.Round((FromSystem.Y2D - mapSystem.solarSystem.Y2D) / LY_SCALING)) :
											new Point((int)Math.Round((mapSystem.solarSystem.DotlanX - FromSystem.DotlanX)), (int)Math.Round((mapSystem.solarSystem.DotlanY - FromSystem.DotlanY))))
										),

										P2 = (
											parameters.MapType == MapType.SDE ? new Point((int)Math.Round((FromSystem.X - linkedSolarSystem.X) / LY_SCALING), (int)Math.Round((FromSystem.Y - linkedSolarSystem.Y) / LY_SCALING)) :
											(parameters.MapType == MapType.SDE2D ? new Point((int)Math.Round((FromSystem.X2D - linkedSolarSystem.X2D) / LY_SCALING), (int)Math.Round((FromSystem.Y2D - linkedSolarSystem.Y2D) / LY_SCALING)) :
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

				int sizeAdjust = (int)SYSTEM_CIRCLE_SIZE + (Math.Max(Math.Max(parameters.StormSize, parameters.SecLimitCloudSize), parameters.IncursionCloudSize));

				foreach (var currentSystem in minimapSystems)
				{
					maxX = Math.Max(maxX, currentSystem.X);
					maxY = Math.Max(maxY, currentSystem.Y);
					minX = Math.Min(minX, currentSystem.X);
					minY = Math.Min(minY, currentSystem.Y);
				}

				maxX += sizeAdjust; maxY += sizeAdjust;
				minX -= sizeAdjust; minY -= sizeAdjust;
				if (minX < 0) { maxX += Math.Abs(minX); minX = 0; }
				if (minY < 0) { maxY += Math.Abs(minY); minY = 0; }

				//int maxSize = Math.Max((int)(maxX - minX), (int)(maxY - minY));
				//int pictureSize = Math.Min(srcImage.Width, srcImage.Height) - BORDER_WIDTH - TRANSPARENT_BORDER_WIDTH - (8);

				double GUI_SCALINGx = (double)(maxX - minX) / (width * (parameters.MapType == MapType.SDE ? .9 : (parameters.MapType == MapType.SDE2D ? .9 : .9)));
				double GUI_SCALINGy = (double)(maxY - minY) / (height * (parameters.MapType == MapType.SDE ? .9 : (parameters.MapType == MapType.SDE2D ? .9 : .9)));
				GUI_SCALING = Math.Max(GUI_SCALINGx, GUI_SCALINGy);

				//GUI_SCALING = (int)Math.Round((double)(maxSize) / ((pictureSize * (parameters.MapType == MapType.SDE ? .9 : (parameters.MapType == MapType.SDE2D ? .9 : .9 )) )));
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
					ox = ((width - (maxX - minX)) / 2) - BORDER_WIDTH - CIRCLE_RADIUS  - 1;
					oy = ((height - (maxY - minY)) / 2) - BORDER_WIDTH - CIRCLE_RADIUS  - 1;
					if (minX < 0) ox = ox + Math.Abs(minX);
					if (minY < 0) oy = oy + Math.Abs(minY);
				}

				// adjust origin
				Point originPoint = new Point(ox, oy);

				float textSize = parameters.DefaultTextSize == false ? (TEXT_SIZE / (float)GUI_SCALING) : MINIMUM_TEXT_SIZE;
				if (textSize < MINIMUM_TEXT_SIZE) textSize = MINIMUM_TEXT_SIZE;


				// draw incursion base if required
				if (minimapSystems.Any(o => o.solarSystem.Incursion) && parameters.IncursionShow)
				{
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					Brush incursionBrush = GDICache.GetHatchBrush(HatchStyle.Percent50, parameters.IncursionColour);

					Brush incursionOutlineBrush = GDICache.GetBrush(parameters.IncursionOutlineColour);
					Pen incursionOutlinePen = GDICache.GetPen(parameters.IncursionOutlineColour, 3);
					float incursionBlobSize = ((float)parameters.IncursionCloudSize / (float)GUI_SCALING);


					ImageAttributes incursionImageAttr = new ImageAttributes();
					SetImageAlpha(incursionImageAttr, parameters.IncursionAlphaAmount);

					using (Image incursionImage = new Bitmap(width, height, PixelFormat.Format32bppArgb))
					using (Graphics gIncursion = Graphics.FromImage(incursionImage))
					{
						MapCircles(gIncursion, minimapSystems.Where(o => o.solarSystem.Incursion).ToList(), originPoint, incursionBlobSize, incursionOutlinePen, incursionBrush, parameters.IncursionOutlineColour, animationIncursion);
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
				if (parameters.SecLimitShow && minimapSystems.Any(o => o.solarSystem.Security <= parameters.SecLimitValue && !o.solarSystem.IsWormhole))
				{
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					Brush secLimitBrush = GDICache.GetHatchBrush(HatchStyle.Percent30, parameters.SecLimitColour);

					Pen secLimitOutlinePen = GDICache.GetPen(parameters.SecLimitOutlineColour, 3);
					float secLimitBlobSize = ((float)parameters.SecLimitCloudSize / (float)GUI_SCALING);
				
					ImageAttributes secLimitImageAttr = new ImageAttributes();
					SetImageAlpha(secLimitImageAttr, parameters.SecLimitAlphaAmount);

					using (Image secLimitImage = new Bitmap(width, height, PixelFormat.Format32bppArgb))
					using (Graphics gSecLimit = Graphics.FromImage(secLimitImage))
					{
						MapCircles(gSecLimit, minimapSystems.Where(o => o.solarSystem.Security <= parameters.SecLimitValue).ToList(), originPoint, secLimitBlobSize, secLimitOutlinePen, secLimitBrush, parameters.SecLimitOutlineColour, animationCycleSecLimit);

						g.DrawImage(
							secLimitImage,
							new Rectangle(0, 0, secLimitImage.Width, secLimitImage.Height),
							0, 0, secLimitImage.Width, secLimitImage.Height,
							GraphicsUnit.Pixel,
							secLimitImageAttr
							);
					}
				}

				// Wormhole ?
				if (minimapSystems.Any(o => o.solarSystem.IsWormhole))
				{
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					Brush wormholeBrush = GDICache.GetHatchBrush(HatchStyle.Percent30, parameters.WormholeColour);

					Pen wormholeOutlinePen = GDICache.GetPen(parameters.WormholeOutlineColour, 3);
					float wormholeBlobSize = ((float)parameters.WormholeSize / (float)GUI_SCALING);

					ImageAttributes wormholeImageAttr = new ImageAttributes();
					SetImageAlpha(wormholeImageAttr, parameters.WormholeAlphaAmount);

					using (Image wormholeImage = new Bitmap(width, height, PixelFormat.Format32bppArgb))
					using (Graphics gWormhole = Graphics.FromImage(wormholeImage))
					{
						MapWormhole(gWormhole, minimapSystems.Where(o => o.solarSystem.IsWormhole).ToList(), originPoint, wormholeBlobSize, wormholeOutlinePen, wormholeBrush, parameters.WormholeOutlineColour, animationCycleWormhole);

						g.DrawImage(
							wormholeImage,
							new Rectangle(0, 0, wormholeImage.Width, wormholeImage.Height),
							0, 0, wormholeImage.Width, wormholeImage.Height,
							GraphicsUnit.Pixel,
							wormholeImageAttr
							);
					}
				}

				float blobSize = (SYSTEM_CIRCLE_SIZE / (float)GUI_SCALING);
				if (blobSize < MINIMUM_BLOB_SIZE || float.IsInfinity(blobSize))
				{
					blobSize = MINIMUM_BLOB_SIZE;
				}

				if (parameters.ShowStorm)
				{
					// storm detection
					float circleSizeStorm = parameters.StormSize / (float)GUI_SCALING;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;

					ImageAttributes stormImageAttr = new ImageAttributes();

					int stormAlphaAmount = 0;
					// Draw outlines first
					// go through systems used 3 times - draw weak first, then strong then centre
					for (int stormStrength = 0; stormStrength <= 2; stormStrength++)
					{
						using (Image stormLimitImage = new Bitmap(width, height, PixelFormat.Format32bppArgb))
						using (Graphics gStorm = Graphics.FromImage(stormLimitImage))
						{
							switch (stormStrength)
							{
								case 0: stormAlphaAmount = parameters.StormWeakOutlineAlpha; break;
								case 1: stormAlphaAmount = parameters.StormStrongOutlineAlpha; break;
								case 2: stormAlphaAmount = parameters.StormCentreOutlineAlpha; break;
							}
							SetImageAlpha(stormImageAttr, stormAlphaAmount);

							foreach (var currentSystem in minimapSystems)
							{
								Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
								lock (Program.Universe.Storms)
								{
									foreach (var storm in Program.Universe.Storms)
									{
										if (stormStrength == 0 && storm.WeakSystemIds != null && storm.WeakSystemIds.Contains(currentSystem.solarSystem.Id))
											DrawStorm(storm.StormType, "WEAK", gStorm, currentPoint, circleSizeStorm, blobSize, true, parameters.StormGlowLayers);
										if (stormStrength == 1 && storm.StrongSystemIds != null && storm.StrongSystemIds.Contains(currentSystem.solarSystem.Id))
											DrawStorm(storm.StormType, "STRONG", gStorm, currentPoint, circleSizeStorm, blobSize, true, parameters.StormGlowLayers);
										if (stormStrength == 2 && storm.SystemId == currentSystem.solarSystem.Id)
											DrawStorm(storm.StormType, "CENTRE", gStorm, currentPoint, circleSizeStorm, blobSize, true, parameters.StormGlowLayers);
									}
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
					try
					{

						// clear background - direct to main graphic
						foreach (var currentSystem in minimapSystems)
						{
							Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
							foreach (var storm in Program.Universe.Storms)
							{
								if (
									(storm?.WeakSystemIds != null && storm.WeakSystemIds.Contains(currentSystem.solarSystem.Id)) ||
									(storm?.StrongSystemIds != null && storm.StrongSystemIds.Contains(currentSystem.solarSystem.Id)) ||
									(storm?.SystemId == currentSystem.solarSystem.Id)
									)
									DrawStorm(storm.StormType, "EMPTY", g, currentPoint, circleSizeStorm, blobSize, false, 0);
							}
						}

						using (Image stormLimitImage = new Bitmap(width, height, PixelFormat.Format32bppArgb))
						using (Graphics gStorm = Graphics.FromImage(stormLimitImage))
						{
							stormAlphaAmount = parameters.StormMainAlpha;
							SetImageAlpha(stormImageAttr, stormAlphaAmount);

							foreach (var currentSystem in minimapSystems)
							{
								Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);
								foreach (var storm in Program.Universe.Storms)
								{
									if (
										(storm?.WeakSystemIds != null && storm.WeakSystemIds.Contains(currentSystem.solarSystem.Id)) ||
										(storm?.StrongSystemIds != null && storm.StrongSystemIds.Contains(currentSystem.solarSystem.Id)) ||
										(storm?.SystemId == currentSystem.solarSystem.Id)
										)
										DrawStorm(storm.StormType, "CENTRE", gStorm, currentPoint, circleSizeStorm, blobSize, false, 0);
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
					catch
					{

					}
				}


				// draw connections
				foreach (var connection in minimapConnectingSystems)
				{
					Point P1 = new Point(originPoint.X + connection.P1.X, originPoint.Y + connection.P1.Y);
					Point P2 = new Point(originPoint.X + connection.P2.X, originPoint.Y + connection.P2.Y);

					// only active routes glow
					if (connection.route)
					{
						for (int i = 4; i >= 1; i--)
						{
							Pen glowPen = GDICache.GetPen(Color.FromArgb(10 / i, parameters.LineColourRoute), i * 2, DashStyle.Solid);
							g.DrawLine(glowPen, P1, P2);
						}
					}

					Pen linePen = GDICache.GetPen(
						connection.route ? parameters.LineColourRoute : ((FromSystem.RegionName == connection.ToSystem.RegionName) ? parameters.LineColourNormal : parameters.LineColourOutOfRegion),
						connection.route ? 2 : 1,
						connection.route ? DashStyle.DashDotDot : DashStyle.Solid);

					g.DrawLine(linePen, P1, P2);

				}

				// draw system blobs
				foreach (var currentSystem in minimapSystems)
				{
					int circlesUsed = -1;

					Point currentPoint = new Point(originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y);


					// Check skyhooks
					bool skyhooksVulnerable = false;
					bool skyhooksVulnerableSoon = false;

					float addX = (float)(0 / GUI_SCALING) + (blobSize/2);
					float addY = (float)(0 / GUI_SCALING) + (blobSize/2);

					float skyhookTextSize = (float)(10 / GUI_SCALING);

					//uint animationCycleSkyhooks = 10;

					DateTime utcNow = DateTime.UtcNow;
					if (currentSystem.solarSystem.Planets != null)
					{
						foreach (var p in currentSystem.solarSystem?.Planets.Where(o => o.SkyhookVulneralbilityStart != null && o.SkyhookVulneralbilityEnd >= utcNow))
						{
							if (p.SkyhookVulneralbilityStart > utcNow) skyhooksVulnerableSoon = true;
							if (p.SkyhookVulneralbilityStart <= utcNow) skyhooksVulnerable = true;
						}
					}

					if (skyhooksVulnerableSoon)
					{
						DrawPulseIndicatorLine(g, new Point(currentPoint.X, currentPoint.Y), Color.Orange, 1 / (float)GUI_SCALING, animationCycleSkyhooks, pulseSize: (3 / (float)GUI_SCALING), maxAlpha: 120, ringWidth: 1.5f / (float)GUI_SCALING);
					}

					if (skyhooksVulnerable)
					{
						DrawPulseIndicatorLine(g, new Point(currentPoint.X , currentPoint.Y ), Color.LimeGreen, 1 / (float)GUI_SCALING, animationCycleSkyhooks, pulseSize: (3 / (float)GUI_SCALING), maxAlpha: 120, ringWidth: 1.5f/(float)GUI_SCALING);
					}

					Color systemBrushColour = parameters.SystemColourNormal;
					if (currentSystem.route) systemBrushColour = parameters.SystemColourRoute;
					if (currentSystem.fromSystem) systemBrushColour = parameters.SystemColourFrom;
					if (currentSystem.toSystem) systemBrushColour = parameters.SystemColourTo;
					Brush systemBrush = GDICache.GetBrush(systemBrushColour);
					g.FillEllipse(systemBrush, currentPoint.X - blobSize, currentPoint.Y - blobSize, blobSize * 2, blobSize * 2);

					// How about mercoxit ?
					if (currentSystem.solarSystem.Security < MERCOXIT_SECURITY && !currentSystem.solarSystem.IsWormhole)
					{
						DrawSystemName(g, currentPoint.X - 10, currentPoint.Y - 3, "✦", Color.FromArgb(50, parameters.MercoxitColour), textSize + 2);
						DrawSystemName(g, currentPoint.X - 10, currentPoint.Y - 3, "✦", parameters.MercoxitColour, textSize);
					}

					// do we have ice !
					if (currentSystem.solarSystem.IceField == true)
					{
						DrawSystemName(g, currentPoint.X + 10, currentPoint.Y - 3, "❅", Color.FromArgb(50, parameters.IceColour), textSize + 2);
						DrawSystemName(g, currentPoint.X + 10, currentPoint.Y - 3, "❅", parameters.IceColour, textSize);
					}



					// Unihole
					int uniHoleAdjust = 0;
					if (currentSystem.solarSystem.UniHole)
					{
						DrawPulseIndicator(g, currentPoint, parameters.UniHoleColour, blobSize, animationCycleUniHoles, pulseSize: (25 / (float)GUI_SCALING), maxAlpha: 80, glowLayers: 3, ringWidth: 2.0f);
						uniHoleAdjust = 1;
					}

					// DrawSystemName(g, currentPoint.X, currentPoint.Y, $"{currentSystem.solarSystem.NPCKillsDeltaLastHour}", Color.AliceBlue, textSize); 

					// active alt in sytstem ?
					if (Program.CharacterManager.Characters.Any(o => o.IsClientActive && !o.IsCurrentlActiveClient && o.LocationId == currentSystem.solarSystem.Id))
					{
						circlesUsed++;
						float circleSize = CIRCLE_INITIAL_GAP + uniHoleAdjust + ((CIRCLE_INCREASE_GAP * circlesUsed) / (float)GUI_SCALING);
						if (circleSize < MINIMUM_CIRCLE_SIZE || float.IsInfinity(circleSize)) circleSize = MINIMUM_CIRCLE_SIZE;
						DrawPulseIndicator(g, currentPoint, parameters.AltColour, blobSize + circleSize, animationCycleAlts, pulseSize: (32 / (float)GUI_SCALING), maxAlpha: 100);
					}

					// Intel alerts
					if (currentSystem.solarSystem.CountAlerts(false, false, false) > 0)
					{
						circlesUsed++;
						float circleSize = CIRCLE_INITIAL_GAP + uniHoleAdjust + ((CIRCLE_INCREASE_GAP * circlesUsed) / (float)GUI_SCALING);

						if (circleSize < MINIMUM_CIRCLE_SIZE || float.IsInfinity(circleSize)) circleSize = MINIMUM_CIRCLE_SIZE;
						DrawPulseIndicator(g, currentPoint, parameters.IntelColour, blobSize + circleSize, animationCycleIntel, pulseSize: (40 / (float)GUI_SCALING), maxAlpha: 120, glowLayers: 4, ringWidth: 2.0f);
						foreach (var a in currentSystem.solarSystem.GetAlerts(false, false, false))
						{
							sbExtraInformation.AppendLine($"{a.Entry.LogTime} {a.Entry.LineContent}");
						}
					}

					//kills ?
					if (currentSystem.solarSystem.CountKills() > 0)
					{
						circlesUsed++;
						float circleSize = CIRCLE_INITIAL_GAP + uniHoleAdjust + ((CIRCLE_INCREASE_GAP * circlesUsed) / (float)GUI_SCALING);
						if (circleSize < MINIMUM_CIRCLE_SIZE || float.IsInfinity(circleSize)) circleSize = MINIMUM_CIRCLE_SIZE;
						DrawPulseIndicator(g, currentPoint, parameters.ZKillColour, blobSize + circleSize, animationCycleKills, pulseSize: (50 / (float)GUI_SCALING), maxAlpha: 160, glowLayers: 4, ringWidth: 2.0f);
						try
						{
							foreach (var k in currentSystem.solarSystem.GetKills())
							{
								sbExtraInformation.AppendLine($"ZKILL {k.Timestamp:HH:mm:ss} {k.SolarSystemName} {k.VictimShipName} {k.VictimCharacterName}");
							}
						}
						catch
						{
						}
					}
				}

				// show all systems names ?
				if (parameters.ShowAllSystemsText)
				{
					foreach (var currentSystem in minimapSystems.Where(o => !o.textDisplayConnecting && !o.textDisplay))
					{
						DrawSystemName(g, originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y, currentSystem.solarSystem.Name, parameters.SystemColourAllText, textSize / 3 * 2, false);
					}
				}

				// show all connecting systems names
				if (parameters.ShowConnectingSystemsText)
				{
					foreach (var currentSystem in minimapSystems.Where(o => o.textDisplayConnecting && !o.textDisplay))
					{
						DrawSystemName(g, originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y, currentSystem.solarSystem.Name, parameters.SystemColourConnectingText, textSize / 4 * 3, false);
					}
				}

				// show systems name
				if (parameters.ShowSystemText)
				{
					foreach (var currentSystem in minimapSystems.Where(o => o.textDisplay))
					{
						DrawSystemName(g, originPoint.X + currentSystem.X, originPoint.Y + currentSystem.Y, currentSystem.solarSystem.Name, parameters.SystemColourText, textSize);
					}
				}


				extraInformation = sbExtraInformation.ToString();

				if (returnMinimapSystems != null)
				{
					foreach (var currentSystem in minimapSystems)
					{
						currentSystem.X += originPoint.X;
						currentSystem.Y += originPoint.Y;
					}

					lock (returnMinimapSystems)
					{
						returnMinimapSystems.Clear();
						returnMinimapSystems.AddRange(minimapSystems);
					}
				}

				return dstImage;
			}
		}

		private static void AddLogoImage(Graphics g, Image srcLogo, RectangleF Outside, RectangleF Inside, Color FillColour, Color LineColour, bool drawBox)
		{
			if (drawBox)
			{
				Brush brFill = GDICache.GetBrush(FillColour);
				Pen penLine = GDICache.GetPen(LineColour, 1);
				g.FillRectangle(brFill, Outside);
				g.DrawRectangle(penLine, Outside.Left, Outside.Top, Outside.Width, Outside.Height);
			}

			g.DrawImage(srcLogo, Inside);
		}

		// makes nice round ellipse/circle images from rectangle images
		public static Image ClipToCircle(Image srcImage, PointF center, float radius, Color backGround, Color circleColour, int circleWidth, string isk, Color iskColour, bool showBar, Color barColor, Image srcAgressorLogo, Image srcVictimLogo, int iskSize, int logoSize)
		{
			float diameter = radius * 2;

			Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(dstImage))
			{

				g.Clear(Color.Transparent);

				int adj = 3;

				// Border
				RectangleF r2 = new RectangleF(center.X - radius+adj, center.Y - radius+adj, diameter, diameter);

				// Main picture
				RectangleF r = new RectangleF(center.X - radius + circleWidth, center.Y - radius + circleWidth+adj, diameter - (circleWidth * 2), diameter - ((circleWidth+adj) * 2));

				RectangleF rVictimLogoOutside = new RectangleF(dstImage.Width - logoSize, 1, logoSize, logoSize);
				RectangleF rVictimLogoInside = new RectangleF(dstImage.Width - logoSize + 2, 3, logoSize - 2, logoSize - 2);

				RectangleF rAgressorLogoOutside = new RectangleF(1, dstImage.Height - iskSize - logoSize + 30, logoSize, logoSize);
				RectangleF rAgressorLogoInside = new RectangleF(3, dstImage.Height - iskSize - logoSize + 32, logoSize - 2, logoSize - 2);

				// enables smoothing of the edge of the circle (less pixelated)
				g.SmoothingMode = SmoothingMode.AntiAlias;

				if (backGround != Color.Transparent)
				{
					// fills background color
					//Brush br = GDICache.GetBrush(backGround);
					//g.FillRectangle(br, 0, 0, dstImage.Width, dstImage.Height);
					g.Clear(backGround);
				}

				if (circleWidth > 0)
				{
					// Draw glow first
					Color glowColour = circleColour;

					for (int i = 12; i >= 1; i--)
					{
						using (Pen glowPen = new Pen(Color.FromArgb(20, glowColour),circleWidth + (i * 4)))
						{
							g.DrawEllipse(glowPen, r);
						}
					}
				}

				// adds the new ellipse & draws the image again 
				using (GraphicsPath path = new GraphicsPath())
				{
					path.AddEllipse(r);
					g.SetClip(path);
					g.DrawImage(srcImage, 0, 0);
					g.ResetClip();
				}

				using (Pen borderPen = new Pen(Color.White, 2))
				{
					g.DrawEllipse(borderPen, r);
				}


				using (Font myfont = new Font("Arial", iskSize - 20, FontStyle.Bold))
				{
					var format = new StringFormat
					{
						Alignment = StringAlignment.Far,
						LineAlignment = StringAlignment.Center,
						Trimming = StringTrimming.None,
					};
					if (showBar) g.FillRectangle(GDICache.GetBrush(barColor), 0, dstImage.Height - iskSize, dstImage.Width, iskSize);
					g.DrawString(isk, myfont, GDICache.GetBrush(iskColour), new RectangleF(0, (dstImage.Height - iskSize), dstImage.Width, iskSize), format);
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

		public static void SetImageAlpha(ImageAttributes attributes, int alphaPercent)
		{
			var matrix = new ColorMatrix
			{
				Matrix33 = alphaPercent / 100f
			};

			attributes.SetColorMatrix(
				matrix,
				ColorMatrixFlag.Default,
				ColorAdjustType.Bitmap);
		}
	}
}
