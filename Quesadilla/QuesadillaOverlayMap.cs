using EVEStandard.Models;
using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Extensions;
using Quesadilla.Classes.GraphData;
using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Quesadilla
{
	public partial class QuesadillaOverlayMap : TransparentOverlayForm
	{
		private Quesadilla _main;
		private AnimationState AnimationStateKills;
		private AnimationState AnimationStateIntel;
		private AnimationState AnimationStateAlts;
		private AnimationState AnimationStateUniHoles;

		private Image _sourceMapImage = null;
		private Bitmap _sourceIskPerMinuteImage = null;

		private List<MinimapSystem> MinimapSystems = new List<MinimapSystem>();
		private List<MinimapSystem> mouseOverMinimapSystems = new List<MinimapSystem>();
		private Point mouseOverPoint;
		private int mapScale = 100;

		public Label RattingData = new Label() { Name = "RattingData" };
		public Label MiningData = new Label() { Name = "MiningData" };
		public Label EveScoutStatus = new Label() { Name = "EveScoutStatus" };
		public PictureBox Minimap = new PictureBox() { Name = "Minimap" };

		public Chart iskPerMinuteChart = new Chart() { Name = "IskPerMinuteChart" };
		private Timer iskPerMinuteTimer = null;
		private Dictionary<int, IskGraphData> _iskGraphData = new Dictionary<int, IskGraphData>();


		private ImageAttributes ImageAttrMainMap = new ImageAttributes();
		private ImageAttributes ImageAttrISKGraph = new ImageAttributes();

		public QuesadillaOverlayMap(Quesadilla mainForm) : base()
		{
			_main = mainForm;
			InitializeComponent();

			Width = Program.MainConfiguration.OverlayMapWindowSize.Width <= 100 ? 100 : Program.MainConfiguration.OverlayMapWindowSize.Width;
			Height = Program.MainConfiguration.OverlayMapWindowSize.Height <= 50 ? 50 : Program.MainConfiguration.OverlayMapWindowSize.Height;

			AnimationStateKills = new AnimationState(state: 0, direction: -1, maximum: 36, pulse: false);
			AnimationStateIntel = new AnimationState(state: 1, direction: 1, maximum: 18, pulse: false);
			AnimationStateAlts = new AnimationState(state: 2, direction: -1, maximum: 12, pulse: false);
			AnimationStateUniHoles = new AnimationState(state: 3, direction: -1, maximum: 36, pulse: true);

			EnableMouseTracking = true;
			SetScreenPosition();

			this.Controls.Add(RattingData);
			this.Controls.Add(MiningData);
			this.Controls.Add(EveScoutStatus);
			this.Controls.Add(Minimap);

			this.Controls.Add(iskPerMinuteChart);

			Program.MainConfiguration.ApplyConfiguration(this, Program.MainConfiguration.OverlayWindowItemLocations, 0, false, false, true);

			mapScale = Program.MainConfiguration.OverlayMapMapScale;
			if (mapScale == 0) mapScale = 100;


			// need these alpha values to be configurable
			ColorMatrix colourMatrix = new ColorMatrix();
			colourMatrix.Matrix33 = (float)((float)70 / (float)100);  // alpha !
			ImageAttrMainMap.SetColorMatrix(colourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			colourMatrix = new ColorMatrix();
			colourMatrix.Matrix33 = (float)((float)50 / (float)100);  // alpha !
			ImageAttrISKGraph.SetColorMatrix(colourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			SetupIskPerMinuteChart();
		}


		private void SetupIskPerMinuteChart()
		{
			ChartArea chartArea = new ChartArea("ISK");
			chartArea.BackColor = Color.Transparent;
			iskPerMinuteChart.ChartAreas.Add(chartArea);
			iskPerMinuteChart.BackColor = Color.Transparent;

			iskPerMinuteChart.ChartAreas[0].AxisY.LineDashStyle = ChartDashStyle.Dash;
			iskPerMinuteChart.ChartAreas[0].AxisY.LineColor = Color.Red;
			iskPerMinuteChart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Purple;
			iskPerMinuteChart.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.LightGoldenrodYellow;

			iskPerMinuteChart.ChartAreas[0].AxisX.LineDashStyle = ChartDashStyle.Dash;
			iskPerMinuteChart.ChartAreas[0].AxisX.LineColor = Color.Yellow;
			iskPerMinuteChart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Green;
			iskPerMinuteChart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

			_sourceIskPerMinuteImage = new Bitmap(iskPerMinuteChart.Width, iskPerMinuteChart.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

			iskPerMinuteTimer = new Timer()
			{
				Interval = 3000,
				Enabled = false,
			};
			iskPerMinuteTimer.Tick += (s, e) => UpdateIskPerMinuteChart();
			iskPerMinuteTimer.Start();
		}

		private void UpdateIskPerMinuteChart()
		{
			iskPerMinuteChart.Series.Clear();
			string Toon = "";

			if (_main.CurrentActiveToon != null && _main.CurrentActiveToon != "")
			{
				Toon = _main.CurrentActiveToon;
			}

			if (Toon == null || Toon == "" || !Program.ISK.ContainsKey(Toon))
			{
				iskPerMinuteChart.Visible = false;
				return;
			}
			else
			{
				iskPerMinuteChart.Visible = true;
			}

			if (Program.MainConfiguration.DPSAllToons)
			{
				Toon = "*ALL*";
			}

			int maxPoints = Program.MainConfiguration.GraphCombatMaxPoints;
			int minSpanSize = Program.MainConfiguration.GraphCombatMinPointSeconds;

			int LineSize = Program.MainConfiguration.DPSGraphLineSize;

			var toonIskIn = iskPerMinuteChart.Series.Add(Toon + " ISK");
			toonIskIn.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
			toonIskIn.Points.Clear();
			toonIskIn.Color = Color.Green;
			toonIskIn.YAxisType = AxisType.Primary;
			toonIskIn.XAxisType = AxisType.Primary;
			//toonIskIn.BorderWidth = LineSize;
			toonIskIn.BorderDashStyle = ChartDashStyle.Solid;

			_iskGraphData.Clear();

			DateTime start = DateTime.Now;
			DateTime end = start;

			// work out start/end times of combat - for cuirrent/all toons
			foreach (var currToon in Program.ISK.Keys)
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					if (Program.ISK[currToon].Count > 0)
					{
						if (Program.ISK[currToon].First().TimeAdded < start) start = Program.ISK[currToon].First().TimeAdded;
						if (Program.ISK[currToon].Last().TimeAdded > end) end = Program.ISK[currToon].Last().TimeAdded;
					}
				}
			}
			TimeSpan totalTime = end - start;

			var span = 60;

			DateTime curr = start;
			curr = DateTime.Now.AddSeconds(-1 * span * 30);
			start = curr;

			foreach (var currToon in Program.ISK.Keys)
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					// setup second slots
					foreach (var c in Program.ISK[currToon].OrderBy(o => o.TimeAdded).ToList())
					{
						var slot = (int)Math.Truncate((c.TimeAdded.TrimSeconds() - start.TrimSeconds()).TotalSeconds / span);
						if (!_iskGraphData.ContainsKey(slot))
						{
							// slot does not exist - create
							_iskGraphData[slot] = new IskGraphData()
							{
								IskAmount = 0,
							};
						}
						if (c.IskAmount != 0)
						{
							_iskGraphData[slot].IskAmount += c.IskAmount;
						}
					}
				}
			}

			// now go thru from start -> end in slot seconds

			var graphStartXp = (int)Math.Truncate((curr - start).TotalSeconds / span);
			if (graphStartXp < 0) graphStartXp = 0;
			var graphEndXp = (int)Math.Truncate((end - start).TotalSeconds / span);

			iskPerMinuteChart.ChartAreas[0].AxisX.Minimum = graphStartXp + 1 > graphEndXp ? graphStartXp : graphStartXp + 1;
			iskPerMinuteChart.ChartAreas[0].AxisX.Maximum = graphEndXp;

			double maxIsk = 0;

			while (curr <= end)
			{
				var minutes = (int)Math.Truncate((curr - start).TotalSeconds / span);
				double Isk = 0;
				if (_iskGraphData.ContainsKey(minutes))
				{
					Isk += _iskGraphData[minutes].IskAmount;
				}

				if (Isk > maxIsk)
				{
					maxIsk = Isk;
				}

				curr = curr.AddSeconds(span);
				toonIskIn.Points.AddXY(minutes, Isk);
			}

			double iskBoundry1 = 1000000;
			double iskBoundry2 = 5000000;
			double iskBoundry3 = 10000000;
			for (int i = 0; i < toonIskIn.Points.Count; i++)
			{
				var pointIsk = toonIskIn.Points[i].YValues[0];
				if (pointIsk > iskBoundry3) {
					toonIskIn.Points[i].Color = Color.Red; continue;
				}
				if (pointIsk > iskBoundry2) {
					toonIskIn.Points[i].Color = Color.Orange; continue;
				}
				toonIskIn.Points[i].Color = Color.Green;
			}

			iskPerMinuteChart.ChartAreas[0].AxisY.Interval = iskPerMinuteChart.ChartAreas[0].AxisY.Maximum / 3;

			if (maxIsk > 1000000)
			{
				iskPerMinuteChart.ChartAreas[0].Axes[1].LabelStyle.Format = "{0:0,,.#}M";
			}
			else if (maxIsk > 1000)
				{
					iskPerMinuteChart.ChartAreas[0].Axes[1].LabelStyle.Format = "{0:0,.#}K";
				}
				else
				{
					iskPerMinuteChart.ChartAreas[0].Axes[1].LabelStyle.Format = "";
				}
		


			// dump to csv
			if (Program.MainConfiguration.DPSSaveCSV)
			{
				using (var file = File.CreateText(Toon + "_raw_isk.csv"))
				{
					foreach (var a in _iskGraphData.Keys)
					{
						file.WriteLine(a + "," + _iskGraphData[a].ToCSV());
					}
				}
			}

			// Store graph to image
			_sourceIskPerMinuteImage.Dispose();
			_sourceIskPerMinuteImage = new Bitmap(iskPerMinuteChart.Width, iskPerMinuteChart.Height);

			iskPerMinuteChart.DrawToBitmap(_sourceIskPerMinuteImage, new Rectangle(0, 0, _sourceIskPerMinuteImage.Width, _sourceIskPerMinuteImage.Height));
		}

		public void SetScreenPosition()
		{
			// position on screen
			Location = new Point()
			{
				X = _main.currentEveWindowRect.Left + Program.MainConfiguration.OverlayMapWindowPosition.X,
				Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + Program.MainConfiguration.OverlayMapWindowPosition.Y,
			};
		}

		protected override void RenderOverlay(Graphics g)
		{

			if (_sourceMapImage != null)
			{
				g.DrawImage(
					_sourceMapImage,
					new Rectangle(Minimap.Left, Minimap.Top, Minimap.Width, Minimap.Height), 
					0, 0, _sourceMapImage.Width, _sourceMapImage.Height,                     
					GraphicsUnit.Pixel,                                                      
					ImageAttrMainMap                                                         
				);

				ImageGenerator.PaintDrawText(g, EveScoutStatus);
			}

			if (mouseOverMinimapSystems.Count > 0)
			{
				//var brushBackground = new SolidBrush(Color.Navy);
				var brush = new SolidBrush(Program.MainConfiguration.OverlayMinimapMouseoverSystemColour);
				var font = new Font("Segoe UI", 12f, FontStyle.Regular);

				//Size size = TextRenderer.MeasureText($"{mouseOverMinimapSystems[0].solarSystem.Name}", font);
				//g.FillRectangle(brushBackground, mouseOverPoint.X - 1, mouseOverPoint.Y, size.Width + 1, size.Height);

				g.DrawString($"{mouseOverMinimapSystems[0].solarSystem.Name}", font, brush, mouseOverPoint);
			}

			ImageGenerator.PaintDrawText(g, RattingData);
			ImageGenerator.PaintDrawText(g, MiningData);

			if (_sourceIskPerMinuteImage != null)
			{
				g.DrawImage(
					_sourceIskPerMinuteImage,
					new Rectangle(iskPerMinuteChart.Left, iskPerMinuteChart.Top, iskPerMinuteChart.Width, iskPerMinuteChart.Height),
					0, 0, _sourceIskPerMinuteImage.Width, _sourceIskPerMinuteImage.Height,                                        
					GraphicsUnit.Pixel,
					ImageAttrISKGraph
				);
			}

		}

		public void UpdateMapData()
		{
			string Toon = "";
			if (_main.CurrentActiveToon != null && _main.CurrentActiveToon != "")
			{
				Toon = _main.CurrentActiveToon;
			}

			if (Toon == null || Toon == "")
			{
				//				OverlayMinimap.Visible = false;
				return;
			}
			else
			{
				//				OverlayMinimap.Visible = true;
				// Set position based on eve window ?
				SetScreenPosition();
			}

			var c = Program.CharacterList.Where(o => o.CharacterName == Toon).FirstOrDefault();

			PathInfo _dummyPath = null;
			if (c != null)
			{
				_dummyPath = new PathInfo()
				{
					FromSystemName = c.Location,
					FromSystemId = c.LocationId,
					ToSystemName = c.Location,
					ToSystemId = c.LocationId,
					ToSystemRegionName = "",
					PathSystems = new List<int> { c.LocationId },
				};
			}

			string extraInformation = "";

			AnimationStateKills.Advance();
			AnimationStateIntel.Advance();
			AnimationStateAlts.Advance();
			AnimationStateUniHoles.Advance();

			Image minimapImage = ImageGenerator.ClipMiniMap(new Bitmap(Minimap.Width, Minimap.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb), _dummyPath,
				Program.MainConfiguration.OverlayMinimapJumps, Program.MainConfiguration.OverlayMapMapScale, Program.MainConfiguration.MinimapRawXY, ref extraInformation, ref MinimapSystems,
				Program.MainConfiguration.OverlayMinimapShowSystemText, Program.MainConfiguration.OverlayMinimapShowAllSystemText, Program.MainConfiguration.OverlayMinimapShowConnectingSystemText,
				Program.MainConfiguration.OverlayShowMinimapBorder, Program.MainConfiguration.MinimapBorderColour,
				Program.MainConfiguration.OverlayMinimapLineColourNormal,
				Program.MainConfiguration.OverlayMinimapLineColourOutOfRegion,
				Program.MainConfiguration.OverlayMinimapLineColourRoute,
				Program.MainConfiguration.OverlayMinimapSystemColourNormal,
				Program.MainConfiguration.OverlayMinimapSystemColourRoute,
				Program.MainConfiguration.OverlayMinimapSystemColourFrom,
				Program.MainConfiguration.OverlayMinimapSystemColourTo,
				Program.MainConfiguration.OverlayMinimapSystemColourText, Program.MainConfiguration.OverlayMinimapAllSystemColourText, Program.MainConfiguration.OverlayMinimapConnectingSystemColourText,
				Program.MainConfiguration.OverlayMinimapZKillColour,
				Program.MainConfiguration.OverlayMinimapIntelColour,
				Program.MainConfiguration.OverlayMinimapAltPositionColour,
				Program.MainConfiguration.OverlayMinimapUniHoleColour,
				Program.MainConfiguration.OverlayMinimapIncursionColour,
				Program.MainConfiguration.OverlayMinimapIncursionOutlineColour,
				Program.MainConfiguration.OverlayMinimapSecLimitColour,
				Program.MainConfiguration.OverlayMinimapSecLimitOutlineColour,
				Program.MainConfiguration.OverlayMinimapIceColour,
				Program.MainConfiguration.OverlayMinimapMercoxitColour,
					AnimationStateIntel.Value(),
					AnimationStateKills.Value(),
					AnimationStateAlts.Value(),
					AnimationStateUniHoles.Value(),
				false,
				Program.MainConfiguration.OverlayMinimapShowOutOfRegion,
				Program.MainConfiguration.OverlayMinimapSecLimit,
				Program.MainConfiguration.OverlayMinimapIncursionAlpha,
				Program.MainConfiguration.OverlayMinimapIncursionCloudSize,
				Program.MainConfiguration.OverlayMinimapSecLimitAlpha,
				Program.MainConfiguration.OverlayMinimapSecLimitCloudSize,
				Program.MainConfiguration.OverlayMinimapConnectionGap,
				Program.MainConfiguration.OverlayMinimapSecLimitValue
				);
			//                Bitmap minimapBitmap = new Bitmap(minimapImage);
			//                minimapBitmap.MakeTransparent(minimapBitmap.GetPixel(0, 0));
			_sourceMapImage = minimapImage;
			RenderAndUpdate();
		}



		protected override void OnOverlayMouseMove(Point clientLocation)
		{
			var hovered = Program.Universe.MouseOverSystem(clientLocation.X, clientLocation.Y, MinimapSystems);

			if (hovered.Count() > 0)
			{
				foreach (var s in hovered)
					Program.Universe.GetSystemInfo((int)s.solarSystem.Id);
			}

			// Only redraw if the hover set actually changed, to avoid re-rendering
			// on every single poll tick while the mouse is merely sitting still inside.
			if (!mouseOverMinimapSystems.SequenceEqual(hovered))
			{
				mouseOverMinimapSystems = hovered;
				mouseOverPoint = new Point(clientLocation.X + 6, clientLocation.Y + 4);
				RenderAndUpdate(); // replaces the old Invalidate() — painting is manual now, Invalidate does nothing
			}
		}

		protected override void OnOverlayMouseLeave()
		{
			if (mouseOverMinimapSystems.Count > 0)
			{
				mouseOverMinimapSystems.Clear();
				RenderAndUpdate();
			}
		}
	}
}
