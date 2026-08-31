using EVEStandard.Models;
using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Extensions;
using Quesadilla.Classes.GraphData;
using Quesadilla.Classes.ImageGeneration;
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

		AnimationState AnimationStateIncursion;
		AnimationState AnimationStateSeclimit;
		AnimationState AnimationStateStormElectric;
		AnimationState AnimationStateStormGamma;
		AnimationState AnimationStateStormPlasma;
		AnimationState AnimationStateStormExotic;

		private Image _sourceMapImage = null;
		private Bitmap _sourceIskPerMinuteImage = null;
		private Bitmap _sourceMinedPerMinuteImage = null;

		private List<MinimapSystem> MinimapSystems = new List<MinimapSystem>();
		private List<MinimapSystem> mouseOverMinimapSystems = new List<MinimapSystem>();
		private Point mouseOverPoint;
		private int mapScale = 100;

		public Label RattingData = new Label() { Name = "RattingData" };
		public Label MiningData = new Label() { Name = "MiningData" };
		public Label EveScoutStatus = new Label() { Name = "EveScoutStatus" };
		public PictureBox Minimap = new PictureBox() { Name = "Minimap" };

		public Chart iskPerMinuteChart = new Chart() { Name = "IskPerMinuteChart" };
		public Chart minedPerMinuteChart = new Chart() { Name = "MinedPerMinuteChart" };
		private Timer xPerMinuteTimer = null;
		private Dictionary<int, IskGraphData> _iskGraphData = new Dictionary<int, IskGraphData>();
		private Dictionary<int, MinedGraphData> _minedGraphData = new Dictionary<int, MinedGraphData>();


		private ImageAttributes ImageAttrMainMap = new ImageAttributes();
		private ImageAttributes ImageAttrISKGraph = new ImageAttributes();
		private ImageAttributes ImageAttrMinedGraph = new ImageAttributes();

		public QuesadillaOverlayMap(Quesadilla mainForm) : base()
		{
			_main = mainForm;
			InitializeComponent();

			Width = Program.MainConfiguration.OverlayMapWindowSize.Width <= 100 ? 100 : Program.MainConfiguration.OverlayMapWindowSize.Width;
			Height = Program.MainConfiguration.OverlayMapWindowSize.Height <= 50 ? 50 : Program.MainConfiguration.OverlayMapWindowSize.Height;

			AnimationStateKills = new AnimationState(state: 0, direction: -1, maximum: 36, pulse: false);
			AnimationStateIntel = new AnimationState(state: 1, direction: 1, maximum: 18, pulse: false);
			AnimationStateAlts = new AnimationState(state: 2, direction: -1, maximum: 12, pulse: false);
			AnimationStateUniHoles = new AnimationState(state: 3, direction: -1, maximum: 18, pulse: true);

			AnimationStateIncursion = new AnimationState(state: 1, direction: 1, maximum: 8, pulse: true);
			AnimationStateSeclimit = new AnimationState(state: 3, direction: -1, maximum: 8, pulse: true);

			AnimationStateStormElectric = new AnimationState(state: 3, direction: -1, maximum: 10, pulse: true);
			AnimationStateStormGamma = new AnimationState(state: 6, direction: -1, maximum: 10, pulse: true);
			AnimationStateStormPlasma = new AnimationState(state: 9, direction: -1, maximum: 10, pulse: true);
			AnimationStateStormExotic = new AnimationState(state: 0, direction: -1, maximum: 10, pulse: true);

			EnableMouseTracking = true;
			SetScreenPosition();

			this.Controls.Add(RattingData);
			this.Controls.Add(MiningData);
			this.Controls.Add(EveScoutStatus);
			this.Controls.Add(Minimap);

			this.Controls.Add(iskPerMinuteChart);
			this.Controls.Add(minedPerMinuteChart);

			Program.MainConfiguration.ApplyConfiguration(this, Program.MainConfiguration.OverlayWindowItemLocations, 0, false, false, true);

			mapScale = Program.MainConfiguration.OverlayMapMapScale;
			if (mapScale == 0) mapScale = 100;


			// need these alpha values to be configurable
			ColorMatrix colourMatrix = new ColorMatrix();
			colourMatrix.Matrix33 = (float)((float)Program.MainConfiguration.OverlayMinimapAlpha / (float)100);  // alpha !
			ImageAttrMainMap.SetColorMatrix(colourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			colourMatrix = new ColorMatrix();
			colourMatrix.Matrix33 = (float)((float)Program.MainConfiguration.OverlayISKGraphAlpha / (float)100);  // alpha !
			ImageAttrISKGraph.SetColorMatrix(colourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			colourMatrix = new ColorMatrix();
			colourMatrix.Matrix33 = (float)((float)Program.MainConfiguration.OverlayMinedGraphAlpha / (float)100);  // alpha !
			ImageAttrMinedGraph.SetColorMatrix(colourMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			SetupPerMinuteCharts();
		}

		private void SetupPerMinuteCharts()
		{
			ChartArea chartArea = new ChartArea("ISK");
			chartArea.BackColor = Color.Transparent;
			iskPerMinuteChart.ChartAreas.Add(chartArea);
			iskPerMinuteChart.BackColor = Color.Transparent;

			iskPerMinuteChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			iskPerMinuteChart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

			iskPerMinuteChart.ChartAreas[0].AxisX.IsMarginVisible = false;

			_sourceIskPerMinuteImage = new Bitmap(iskPerMinuteChart.Width, iskPerMinuteChart.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);


			chartArea = new ChartArea("Mining");
			chartArea.BackColor = Color.Transparent;
			minedPerMinuteChart.ChartAreas.Add(chartArea);
			minedPerMinuteChart.BackColor = Color.Transparent;

			minedPerMinuteChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			minedPerMinuteChart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

			minedPerMinuteChart.ChartAreas[0].AxisX.IsMarginVisible = false;

			_sourceMinedPerMinuteImage = new Bitmap(minedPerMinuteChart.Width, minedPerMinuteChart.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);


			xPerMinuteTimer = new Timer()
			{
				Interval = 5000,
				Enabled = false,
			};
			xPerMinuteTimer.Tick += (s, e) => UpdatePerMinuteChart();
			xPerMinuteTimer.Start();
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

			if (Program.MainConfiguration.OverlayISKGraphAllToons)
			{
				Toon = "*ALL*";
			}

			var toonIskIn = iskPerMinuteChart.Series.Add(Toon + " ISK");
			toonIskIn.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
			toonIskIn.Points.Clear();
			toonIskIn.Color = Color.Green;
			if (Program.MainConfiguration.OverlayISKGraphYLegendRight)
			{
				toonIskIn.YAxisType = AxisType.Secondary;
			}
			else
			{
				toonIskIn.YAxisType = AxisType.Primary;
			}
			toonIskIn.XAxisType = AxisType.Primary;
			toonIskIn["PointWidth"] = "1";

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


			double maxIsk = 0; // default to 20m being highest value
			double totalIsk = 0;
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
				totalIsk += Isk;
			}

			// show rag for different values
			double iskBoundry1 = Program.MainConfiguration.OverlayISKGraphLowBoundry;
			double iskBoundry2 = Program.MainConfiguration.OverlayISKGraphMediumBoundry;
			double iskBoundry3 = Program.MainConfiguration.OverlayISKGraphHighBoundry;
			for (int i = 0; i < toonIskIn.Points.Count; i++)
			{
				var pointIsk = toonIskIn.Points[i].YValues[0];
				if (pointIsk > iskBoundry3)
				{
					toonIskIn.Points[i].Color = Program.MainConfiguration.OverlayISKGraphHighBoundryColour; continue;
				}
				if (pointIsk > iskBoundry2)
				{
					toonIskIn.Points[i].Color = Program.MainConfiguration.OverlayISKGraphMediumBoundryColour; continue;
				}
				toonIskIn.Points[i].Color = Program.MainConfiguration.OverlayISKGraphLowBoundryColour;
			}

			int axis = 1;
			if (Program.MainConfiguration.OverlayISKGraphYLegendRight)
			{
				iskPerMinuteChart.ChartAreas[0].AxisY2.Interval = 5000000; //iskPerMinuteChart.ChartAreas[0].AxisY.Maximum / 3;
				iskPerMinuteChart.ChartAreas[0].AxisY2.IsReversed = Program.MainConfiguration.OverlayISKGraphYReversed;

				iskPerMinuteChart.ChartAreas[0].AxisY2.LineDashStyle = ChartDashStyle.Dash;
				iskPerMinuteChart.ChartAreas[0].AxisY2.LineColor = Color.Red;
				iskPerMinuteChart.ChartAreas[0].AxisY2.MajorGrid.LineColor = Program.MainConfiguration.OverlayISKGraphYColour;
				iskPerMinuteChart.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Program.MainConfiguration.OverlayISKGraphLabelColour;
				iskPerMinuteChart.ChartAreas[0].AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

				axis = 3;
			}
			else
			{

				iskPerMinuteChart.ChartAreas[0].AxisY.LineDashStyle = ChartDashStyle.Dash;
				iskPerMinuteChart.ChartAreas[0].AxisY.LineColor = Color.Red;
				iskPerMinuteChart.ChartAreas[0].AxisY.MajorGrid.LineColor = Program.MainConfiguration.OverlayISKGraphYColour;
				iskPerMinuteChart.ChartAreas[0].AxisY.LabelStyle.ForeColor = Program.MainConfiguration.OverlayISKGraphLabelColour;
				iskPerMinuteChart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

				iskPerMinuteChart.ChartAreas[0].AxisY.Interval = 5000000; //iskPerMinuteChart.ChartAreas[0].AxisY.Maximum / 3;
				iskPerMinuteChart.ChartAreas[0].AxisY.IsReversed = Program.MainConfiguration.OverlayISKGraphYReversed;
			}

			if (maxIsk > 1000000)
			{
				iskPerMinuteChart.ChartAreas[0].Axes[axis].LabelStyle.Format = "{0:0,,.#}M";
			}
			else if (maxIsk > 1000)
			{
				iskPerMinuteChart.ChartAreas[0].Axes[axis].LabelStyle.Format = "{0:0,.#}K";
			}
			else
			{
				iskPerMinuteChart.ChartAreas[0].Axes[axis].LabelStyle.Format = "";
			}


			// Store graph to image
			if (_sourceIskPerMinuteImage != null) _sourceIskPerMinuteImage.Dispose();
			if (totalIsk > 0)
			{
				_sourceIskPerMinuteImage = new Bitmap(iskPerMinuteChart.Width, iskPerMinuteChart.Height);
				iskPerMinuteChart.DrawToBitmap(_sourceIskPerMinuteImage, new Rectangle(0, 0, _sourceIskPerMinuteImage.Width, _sourceIskPerMinuteImage.Height));
			}
			else
			{
				iskPerMinuteChart.Visible = false;
			}
		}

		private void UpdateMinedPerMinuteChart()
		{
			minedPerMinuteChart.Series.Clear();
			string Toon = "";

			if (_main.CurrentActiveToon != null && _main.CurrentActiveToon != "")
			{
				Toon = _main.CurrentActiveToon;
			}

			if (Toon == null || Toon == "" || !Program.Mining.ContainsKey(Toon))
			{
				minedPerMinuteChart.Visible = false;
				return;
			}
			else
			{
				minedPerMinuteChart.Visible = true;
			}

			if (Program.MainConfiguration.OverlayMinedGraphAllToons)
			{
				Toon = "*ALL*";
			}


			var toonWasted = minedPerMinuteChart.Series.Add(Toon + " Wasted");
			toonWasted.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedArea;
			toonWasted.Points.Clear();
			toonWasted.Color = Program.MainConfiguration.OverlayMinedGraphWastedColour;
			toonWasted.XAxisType = AxisType.Primary;
			toonWasted["PointWidth"] = "1";
			toonWasted.BorderDashStyle = ChartDashStyle.Solid;

			var toonMined = minedPerMinuteChart.Series.Add(Toon + " Mined");
			toonMined.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedArea;
			toonMined.Points.Clear();
			toonMined.Color = Program.MainConfiguration.OverlayMinedGraphMinedColour; ;
			if (Program.MainConfiguration.OverlayMinedGraphYLegendRight)
			{
				toonMined.YAxisType = AxisType.Secondary;
			}
			else
			{
				toonMined.YAxisType = AxisType.Primary;
			}
			toonMined.XAxisType = AxisType.Primary;
			toonMined["PointWidth"] = "1";
			toonMined.BorderDashStyle = ChartDashStyle.Solid;

			toonWasted.YAxisType = toonMined.YAxisType;


			_minedGraphData.Clear();

			DateTime start = DateTime.Now;
			DateTime end = start;

			// work out start/end times of combat - for cuirrent/all toons
			foreach (var currToon in Program.Mining.Keys)
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					if (Program.Mining[currToon].Count > 0)
					{
						if (Program.Mining[currToon].First().TimeAdded < start) start = Program.Mining[currToon].First().TimeAdded;
						if (Program.Mining[currToon].Last().TimeAdded > end) end = Program.Mining[currToon].Last().TimeAdded;
					}
				}
			}
			TimeSpan totalTime = end - start;

			var span = 60;

			DateTime curr = start;
			curr = DateTime.Now.AddSeconds(-1 * span * 30);
			start = curr;


			Random r  = new Random();

			foreach (var currToon in Program.Mining.Keys)
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					// setup second slots
					foreach (var c in Program.Mining[currToon].OrderBy(o => o.TimeAdded).ToList())
					{
						var slot = (int)Math.Truncate((c.TimeAdded.TrimSeconds() - start.TrimSeconds()).TotalSeconds / span);
						if (!_minedGraphData.ContainsKey(slot))
						{
							// slot does not exist - create
							_minedGraphData[slot] = new MinedGraphData()
							{
								MinedAmount = 0,
								WastageAmount = 0,
							};
						}
						if (c.MiningAmount != 0 || c.WastageAmount != 0)
						{
							_minedGraphData[slot].MinedAmount += c.MiningAmount;
							_minedGraphData[slot].WastageAmount += c.WastageAmount;
							// ignore minint item for now
						}
					}
				}
			}


			int maxMined = 0; int totalMined = 0;
			int maxWasted = 0; int totalWasted = 0;
			while (curr <= end)
			{
				var minutes = (int)Math.Truncate((curr - start).TotalSeconds / span);
				int mined = 0; int wasted = 0;
				if (_iskGraphData.ContainsKey(minutes))
				{
					mined += _minedGraphData[minutes].MinedAmount;
					wasted += _minedGraphData[minutes].WastageAmount;
				} 

				if (mined > maxMined)
				{
					maxMined = mined;
				}

				if (wasted > maxWasted)
				{
					maxWasted = wasted;
				}

				curr = curr.AddSeconds(span);
				toonMined.Points.AddXY(minutes, mined);
				toonWasted.Points.AddXY(minutes, wasted);
				totalMined += mined;totalWasted += wasted;
			}

			int axis = 1;
			if (Program.MainConfiguration.OverlayMinedGraphYLegendRight)
			{
				minedPerMinuteChart.ChartAreas[0].AxisY2.Interval = 1000;
				minedPerMinuteChart.ChartAreas[0].AxisY2.IsReversed = Program.MainConfiguration.OverlayMinedGraphYReversed;

				minedPerMinuteChart.ChartAreas[0].AxisY2.LineDashStyle = ChartDashStyle.Dash;
				minedPerMinuteChart.ChartAreas[0].AxisY2.LineColor = Color.Red;
				minedPerMinuteChart.ChartAreas[0].AxisY2.MajorGrid.LineColor = Program.MainConfiguration.OverlayMinedGraphYColour;
				minedPerMinuteChart.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Program.MainConfiguration.OverlayMinedGraphLabelColour;
				minedPerMinuteChart.ChartAreas[0].AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

				axis = 3;
			}
			else
			{
				minedPerMinuteChart.ChartAreas[0].AxisY.LineDashStyle = ChartDashStyle.Dash;
				minedPerMinuteChart.ChartAreas[0].AxisY.LineColor = Color.Red;
				minedPerMinuteChart.ChartAreas[0].AxisY.MajorGrid.LineColor = Program.MainConfiguration.OverlayMinedGraphYColour;
				minedPerMinuteChart.ChartAreas[0].AxisY.LabelStyle.ForeColor = Program.MainConfiguration.OverlayMinedGraphLabelColour;
				minedPerMinuteChart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

				minedPerMinuteChart.ChartAreas[0].AxisY.Interval = 1000; //minedPerMinuteChart.ChartAreas[0].AxisY.Maximum / 3;
				minedPerMinuteChart.ChartAreas[0].AxisY.IsReversed = Program.MainConfiguration.OverlayMinedGraphYReversed;
			}

			if (maxMined > 1000000)
			{
				minedPerMinuteChart.ChartAreas[0].Axes[axis].LabelStyle.Format = "{0:0,,.#}M";
			}
			else if (maxMined > 1000)
			{
				minedPerMinuteChart.ChartAreas[0].Axes[axis].LabelStyle.Format = "{0:0,.#}K";
			}
			else
			{
				minedPerMinuteChart.ChartAreas[0].Axes[axis].LabelStyle.Format = "";
			}


			// Store graph to image
			if (_sourceMinedPerMinuteImage != null) _sourceMinedPerMinuteImage.Dispose();
			if (totalMined > 0)
			{
				_sourceMinedPerMinuteImage = new Bitmap(minedPerMinuteChart.Width, minedPerMinuteChart.Height);
				minedPerMinuteChart.DrawToBitmap(_sourceMinedPerMinuteImage, new Rectangle(0, 0, _sourceMinedPerMinuteImage.Width, _sourceMinedPerMinuteImage.Height));
			}
			else
			{
				minedPerMinuteChart.Visible = false;
			}
		}

		private void UpdatePerMinuteChart()
		{
			UpdateIskPerMinuteChart();
			UpdateMinedPerMinuteChart();
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
				var brush = GDICache.GetBrush(Program.MainConfiguration.OverlayMinimapMouseoverSystemColour);
				var font = new Font("Segoe UI", 12f, FontStyle.Regular);

				//Size size = TextRenderer.MeasureText($"{mouseOverMinimapSystems[0].solarSystem.Name}", font);
				//g.FillRectangle(brushBackground, mouseOverPoint.X - 1, mouseOverPoint.Y, size.Width + 1, size.Height);

				g.DrawString($"{mouseOverMinimapSystems[0].solarSystem.Name}", font, brush, mouseOverPoint);
			}

			ImageGenerator.PaintDrawText(g, RattingData);
			ImageGenerator.PaintDrawText(g, MiningData);

			if (_sourceIskPerMinuteImage != null)
			{
				try
				{
					g.DrawImage(
						_sourceIskPerMinuteImage,
						new Rectangle(iskPerMinuteChart.Left, iskPerMinuteChart.Top, iskPerMinuteChart.Width, iskPerMinuteChart.Height),
						0, 0, _sourceIskPerMinuteImage.Width, _sourceIskPerMinuteImage.Height,
						GraphicsUnit.Pixel,
						ImageAttrISKGraph
					);
				}
				catch { }
			}

			if (_sourceMinedPerMinuteImage != null)
			{
				try
				{
					g.DrawImage(
						_sourceMinedPerMinuteImage,
						new Rectangle(minedPerMinuteChart.Left, minedPerMinuteChart.Top, minedPerMinuteChart.Width, minedPerMinuteChart.Height),
						0, 0, _sourceMinedPerMinuteImage.Width, _sourceMinedPerMinuteImage.Height,
						GraphicsUnit.Pixel,
						ImageAttrMinedGraph
					);
				}
				catch { }
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
				//SetScreenPosition();
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
					AnimationStateIntel.GetNextValue(),
					AnimationStateKills.GetNextValue(),
					AnimationStateAlts.GetNextValue(),
					AnimationStateUniHoles.GetNextValue(),
					AnimationStateIncursion.GetNextValue(),
					AnimationStateSeclimit.GetNextValue(),
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
