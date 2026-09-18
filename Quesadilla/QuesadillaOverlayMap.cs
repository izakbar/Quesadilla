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
		private AnimationManager _animationManager = new AnimationManager();
		private Image _sourceMapImage = null;
		private Bitmap _sourceIskPerMinuteImage = null;
		private Bitmap _sourceMinedPerMinuteImage = null;
		private Bitmap _mouseOverImage = null;

		private List<MinimapSystem> MinimapSystems = new List<MinimapSystem>();
		private List<MinimapSystem> mouseOverMinimapSystems = new List<MinimapSystem>();
		private Point mouseOverPoint;

		// "Controls"
		public Label RattingData = new Label() { Name = "RattingData" };
		public Label MiningData = new Label() { Name = "MiningData" };
		public Label EveScoutStatus = new Label() { Name = "EveScoutStatus" };
		public PictureBox Minimap = new PictureBox() { Name = "Minimap" };
		public Panel MouseOver = new Panel() { Name = "MouseOver" };
		public Label MouseOverSystemName = new Label() { Name = "MouseOverSystemName" };
		public Label MouseOverSecurity = new Label() { Name = "MouseOverSecurity" };
		public Label MouseOverSkyhookInfo = new Label() { Name = "MouseOverSkyhookInfo" };
		public Label MouseOverNPCDelta = new Label() { Name = "MouseOverNPCDelta" };
		public Label MouseOverKills = new Label() { Name = "MouseOverKills" };
		public Label MouseOverAlerts = new Label() { Name = "MouseOverAlerts" };
		public Label MouseOverJumpsPerHour = new Label() { Name = "MouseOverJumpsPerHour" };
		public Label MouseOverKillsPerHour = new Label() { Name = "MouseOverKillsPerHour" };

		public Chart iskPerMinuteChart = new Chart() { Name = "IskPerMinuteChart" };
		public Chart minedPerMinuteChart = new Chart() { Name = "MinedPerMinuteChart" };
		private Timer xPerMinuteTimer = null;
		private Dictionary<int, IskGraphData> _iskGraphData = new Dictionary<int, IskGraphData>();
		private Dictionary<int, MinedGraphData> _minedGraphData = new Dictionary<int, MinedGraphData>();

		private ImageAttributes ImageAttrMainMap = new ImageAttributes();
		private ImageAttributes ImageAttrISKGraph = new ImageAttributes();
		private ImageAttributes ImageAttrMinedGraph = new ImageAttributes();
		private ImageAttributes ImageAttrMouseOverGraph = new ImageAttributes();
		private MinimapParameters _minimapParameters = new MinimapParameters();

		public QuesadillaOverlayMap(Quesadilla mainForm) : base()
		{
			_main = mainForm;
			InitializeComponent();

			Width = Math.Max(Program.MainConfiguration.OverlayMapWindowSize.Width, 100);
			Height = Math.Max(Program.MainConfiguration.OverlayMapWindowSize.Height, 50);

			EnableMouseTracking = true;
			SetScreenPosition();

			Controls.AddRange(new Control[] {
				RattingData,
				MiningData,
				EveScoutStatus,
				Minimap,
				iskPerMinuteChart,
				minedPerMinuteChart,
				MouseOver, MouseOverSystemName, MouseOverSecurity, MouseOverSkyhookInfo, MouseOverNPCDelta, MouseOverKills, MouseOverAlerts, MouseOverJumpsPerHour, MouseOverKillsPerHour
			});

			Program.MainConfiguration.ApplyConfiguration(this, Program.MainConfiguration.OverlayWindowItemLocations, 0, false, false, true);

			ImageGenerator.SetImageAlpha(ImageAttrMainMap, Program.MainConfiguration.OverlayMinimapAlpha);
			ImageGenerator.SetImageAlpha(ImageAttrISKGraph, Program.MainConfiguration.OverlayISKGraphAlpha);
			ImageGenerator.SetImageAlpha(ImageAttrMinedGraph, Program.MainConfiguration.OverlayMinedGraphAlpha);
			ImageGenerator.SetImageAlpha(ImageAttrMouseOverGraph, Program.MainConfiguration.OverlayMouseOverGraphAlpha);

			SetupPerMinuteCharts();
			UpdateMinimapParameters();

			_mouseOverImage = new Bitmap(MouseOver.Width,MouseOver.Height,PixelFormat.Format32bppPArgb);
		}
		public void UpdateMinimapParameters()
		{
			_minimapParameters.StormCentreOutlineAlpha = Program.MainConfiguration.OverlayMinimapStormCentreOutlineAlpha;
			_minimapParameters.StormStrongOutlineAlpha = Program.MainConfiguration.OverlayMinimapStormStrongOutlineAlpha;
			_minimapParameters.StormWeakOutlineAlpha = Program.MainConfiguration.OverlayMinimapStormWeakOutlineAlpha;
			_minimapParameters.StormMainAlpha = Program.MainConfiguration.OverlayMinimapStormMainAlpha;
			_minimapParameters.ShowStorm = Program.MainConfiguration.OverlayMinimapShowStorm;
			_minimapParameters.StormGlowLayers = Program.MainConfiguration.OverlayMinimapStormGlowLayers;

			_minimapParameters.SecLimitValue = Program.MainConfiguration.OverlayMinimapSecLimitValue;
			_minimapParameters.SecLimitAlphaAmount = Program.MainConfiguration.OverlayMinimapSecLimitAlpha;
			_minimapParameters.SecLimitCloudSize = Program.MainConfiguration.OverlayMinimapSecLimitCloudSize;
			_minimapParameters.SecLimitShow = Program.MainConfiguration.OverlayMinimapSecLimitShow;
			_minimapParameters.SecLimitColour = Program.MainConfiguration.OverlayMinimapSecLimitColour;
			_minimapParameters.SecLimitOutlineColour = Program.MainConfiguration.OverlayMinimapSecLimitOutlineColour;
			_minimapParameters.StormSize = Program.MainConfiguration.OverlayMinimapStormSize;

			_minimapParameters.IncursionAlphaAmount = Program.MainConfiguration.OverlayMinimapIncursionAlpha;
			_minimapParameters.IncursionCloudSize = Program.MainConfiguration.OverlayMinimapIncursionCloudSize;
			_minimapParameters.IncursionColour = Program.MainConfiguration.OverlayMinimapIncursionColour;
			_minimapParameters.IncursionOutlineColour = Program.MainConfiguration.OverlayMinimapIncursionOutlineColour;
			_minimapParameters.IncursionShow = Program.MainConfiguration.OverlayMinimapIncursionShow;

			_minimapParameters.BorderColor = Program.MainConfiguration.OverlayMinimapBorderColour;
			_minimapParameters.LineColourNormal = Program.MainConfiguration.OverlayMinimapLineColourNormal;
			_minimapParameters.LineColourOutOfRegion = Program.MainConfiguration.OverlayMinimapLineColourOutOfRegion;
			_minimapParameters.LineColourRoute = Program.MainConfiguration.OverlayMinimapLineColourRoute;
			_minimapParameters.SystemColourNormal = Program.MainConfiguration.OverlayMinimapSystemColourNormal;
			_minimapParameters.SystemColourRoute = Program.MainConfiguration.OverlayMinimapSystemColourRoute;
			_minimapParameters.SystemColourFrom = Program.MainConfiguration.OverlayMinimapSystemColourFrom;	
			_minimapParameters.SystemColourTo = Program.MainConfiguration.OverlayMinimapSystemColourTo;
			_minimapParameters.SystemColourText = Program.MainConfiguration.OverlayMinimapSystemColourText;
			_minimapParameters.SystemColourAllText = Program.MainConfiguration.OverlayMinimapColourAllText;
			_minimapParameters.SystemColourConnectingText = Program.MainConfiguration.OverlayMinimapColourConnectingText;
			_minimapParameters.ZKillColour = Program.MainConfiguration.OverlayMinimapZKillColour;
			_minimapParameters.IntelColour = Program.MainConfiguration.OverlayMinimapIntelColour;
			_minimapParameters.AltColour = Program.MainConfiguration.OverlayMinimapAltColour;
			_minimapParameters.UniHoleColour = Program.MainConfiguration.OverlayMinimapUniHoleColour;
			_minimapParameters.IceColour = Program.MainConfiguration.OverlayMinimapIceColour;
			_minimapParameters.MercoxitColour = Program.MainConfiguration.OverlayMinimapMercoxitColour;

			_minimapParameters.CloseSystems = Program.MainConfiguration.OverlayMinimapCloseSystems;
			_minimapParameters.MapType = Program.MainConfiguration.OverlayMinimapMapType;
			_minimapParameters.ShowSystemText = Program.MainConfiguration.OverlayMinimapShowSystemText;
			_minimapParameters.ShowAllSystemsText = Program.MainConfiguration.OverlayMinimapShowAllSystemText;
			_minimapParameters.ShowConnectingSystemsText = Program.MainConfiguration.OverlayMinimapShowConnectingSystemsText;
			_minimapParameters.ShowBorder = Program.MainConfiguration.OverlayMinimapShowBorder;
			_minimapParameters.DefaultTextSize = false;
			_minimapParameters.ShowOutOfRegion = Program.MainConfiguration.OverlayMinimapShowOutOfRegion;
		}
		private void ConfigureChart(Chart chart, string areaName)
		{
			var chartArea = new ChartArea(areaName)
			{
				BackColor = Color.Transparent
			};

			chart.ChartAreas.Add(chartArea);
			chart.BackColor = Color.Transparent;

			chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			chart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
			chart.ChartAreas[0].AxisX.IsMarginVisible = false;
		}

		private void SetupPerMinuteCharts()
		{
			ConfigureChart(iskPerMinuteChart, "ISK");
			ConfigureChart(minedPerMinuteChart, "Mining");

			_sourceIskPerMinuteImage = new Bitmap(
				iskPerMinuteChart.Width,
				iskPerMinuteChart.Height,
				PixelFormat.Format32bppPArgb);

			_sourceMinedPerMinuteImage = new Bitmap(
				minedPerMinuteChart.Width,
				minedPerMinuteChart.Height,
				PixelFormat.Format32bppPArgb);

			xPerMinuteTimer = new Timer
			{
				Interval = 5000
			};

			xPerMinuteTimer.Tick += (s, e) => UpdatePerMinuteChart();
			xPerMinuteTimer.Start();
		}


		private void UpdateIskPerMinuteChart()
		{
			iskPerMinuteChart.Series.Clear();
			string Toon = "";

			if (!string.IsNullOrEmpty(_main.CurrentActiveToon))
			{
				Toon = _main.CurrentActiveToon;
			}

			if (string.IsNullOrEmpty(Toon) || !Program.GraphDataManager.GetIskDataKeys().Contains(Toon))
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
			foreach(var currToon in Program.GraphDataManager.GetIskDataKeys())
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					var isk = Program.GraphDataManager.GetIskDataLines(currToon);
					if (isk.Any())
					{
						if (isk.First().TimeAdded < start) start = isk.First().TimeAdded;
						if (isk.Last().TimeAdded > end) end = isk.Last().TimeAdded;
					}
				}
			}
			var span = 60;

			DateTime curr = start;
			curr = end.AddSeconds(-1 * span * 30);
			start = curr;

			foreach (var currToon in Program.GraphDataManager.GetIskDataKeys())
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					// setup second slots
					foreach (var c in Program.GraphDataManager.GetIskDataLines(currToon)  /*.OrderBy(o => o.TimeAdded).ToList()*/ )
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
			if (_sourceIskPerMinuteImage == null ||
				_sourceIskPerMinuteImage.Width != iskPerMinuteChart.Width || _sourceIskPerMinuteImage.Height != iskPerMinuteChart.Height)
			{
				_sourceIskPerMinuteImage?.Dispose();

				_sourceIskPerMinuteImage = new Bitmap(iskPerMinuteChart.Width,iskPerMinuteChart.Height);
			}
			using (var g = Graphics.FromImage(_sourceIskPerMinuteImage))
			{
				g.Clear(Color.Transparent);
			}

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

			if (!string.IsNullOrEmpty(_main.CurrentActiveToon))
			{
				Toon = _main.CurrentActiveToon;
			}

			if (string.IsNullOrEmpty(Toon) || !Program.GraphDataManager.GetMiningDataKeys().Contains(Toon))
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

			var toonMinedCritical = minedPerMinuteChart.Series.Add(Toon + " Critical");
			toonMinedCritical.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedArea;
			toonMinedCritical.Points.Clear();
			toonMinedCritical.Color = Program.MainConfiguration.OverlayMinedGraphCriticalColour; ;
			toonMinedCritical.XAxisType = AxisType.Primary;
			toonMinedCritical["PointWidth"] = "1";
			toonMinedCritical.BorderDashStyle = ChartDashStyle.Solid;

			toonMinedCritical.YAxisType = toonMined.YAxisType;
			toonWasted.YAxisType = toonMined.YAxisType;


			_minedGraphData.Clear();

			DateTime start = DateTime.Now;
			DateTime end = start;

			// work out start/end times of combat - for cuirrent/all toons
			foreach (var currToon in Program.GraphDataManager.GetMiningDataKeys())
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					var md = Program.GraphDataManager.GetMiningDataLines(currToon);
					if (md.Any())
					{
						if (md.First().TimeAdded < start) start = md.First().TimeAdded;
						if (md.Last().TimeAdded > end) end = md.Last().TimeAdded;
					}
				}
			}

			var span = 60;

			DateTime curr = start;
			curr = end.AddSeconds(-1 * span * 30);
			start = curr;


			foreach (var currToon in Program.GraphDataManager.GetMiningDataKeys())
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					// setup second slots
					foreach (var c in Program.GraphDataManager.GetMiningDataLines(currToon) /*.OrderBy(o => o.TimeAdded).ToList() */ )
					{
						var slot = (int)Math.Truncate((c.TimeAdded.TrimSeconds() - start.TrimSeconds()).TotalSeconds / span);
						if (!_minedGraphData.ContainsKey(slot))
						{
							// slot does not exist - create
							_minedGraphData[slot] = new MinedGraphData()
							{
								MinedAmount = 0,
								WastageAmount = 0,
								CriticalAmount = 0
							};
						}
						if (c.MiningAmount != 0 || c.WastageAmount != 0)
						{
							_minedGraphData[slot].WastageAmount += c.WastageAmount;
							if (c.Critical)
								_minedGraphData[slot].CriticalAmount += c.MiningAmount;
							else
								_minedGraphData[slot].MinedAmount += c.MiningAmount;
						}
					}
				}
			}

			int maxMined = 0; int totalMined = 0;
			int maxCritical = 0; int totalCritical = 0;
			int maxWasted = 0; int totalWasted = 0;
			while (curr <= end)
			{
				var minutes = (int)Math.Truncate((curr - start).TotalSeconds / span);
				int mined = 0; int wasted = 0; int critical = 0;
				if (_minedGraphData.ContainsKey(minutes))
				{
					mined += _minedGraphData[minutes].MinedAmount;
					wasted += _minedGraphData[minutes].WastageAmount;
					critical += _minedGraphData[minutes].CriticalAmount;
				}

				if (mined > maxMined) maxMined = mined;
				if (critical > maxCritical) maxCritical = critical;
				if (wasted > maxWasted) maxWasted = wasted;

				curr = curr.AddSeconds(span);
				toonMined.Points.AddXY(minutes, mined);
				toonMinedCritical.Points.AddXY(minutes, critical);
				toonWasted.Points.AddXY(minutes, wasted);
				totalMined += mined + critical ;totalWasted += wasted;
			}

			double div = 100;
			if (maxMined > 1000) div = 100;
			if (maxMined > 10000) div = 1000;
			if (maxMined > 100000) div = 10000;
			if (maxMined > 1000000) div = 100000;
			double interval = Math.Ceiling((maxMined + maxCritical) / 4.0 / div) * div;

			int axis = 1;
			if (Program.MainConfiguration.OverlayMinedGraphYLegendRight)
			{
				minedPerMinuteChart.ChartAreas[0].AxisY2.Interval = interval;
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

				minedPerMinuteChart.ChartAreas[0].AxisY.Interval = interval; 
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
			if (_sourceMinedPerMinuteImage == null ||
				_sourceMinedPerMinuteImage.Width != minedPerMinuteChart.Width || _sourceMinedPerMinuteImage.Height != minedPerMinuteChart.Height)
			{
				_sourceMinedPerMinuteImage?.Dispose();

				_sourceMinedPerMinuteImage = new Bitmap(minedPerMinuteChart.Width, minedPerMinuteChart.Height);
			}
			using (var g = Graphics.FromImage(_sourceMinedPerMinuteImage))
			{
				g.Clear(Color.Transparent);
			}

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


		public string TimeSpanToFriendlyString(TimeSpan ts)
		{
			var parts = new List<string>();

			if (ts.Days > 0)
				parts.Add($"{ts.Days} day{(ts.Days == 1 ? "" : "s")}");

			if (ts.Hours > 0)
				parts.Add($"{ts.Hours}hr{(ts.Hours == 1 ? "" : "s")}");

			if (ts.TotalHours >= 1)
			{
				if (ts.Minutes > 0)
					parts.Add($"{ts.Minutes}min{(ts.Minutes == 1 ? "" : "s")}");
			}
			else
			{
				if (ts.Minutes > 0)
					parts.Add($"{ts.Minutes}min{(ts.Minutes == 1 ? "" : "s")}");

				if (ts.Seconds > 0 || parts.Count == 0)
					parts.Add($"{ts.Seconds}sec{(ts.Seconds == 1 ? "" : "s")}");
			}
			return string.Join(" ", parts);
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

			if (mouseOverMinimapSystems.Count > 0)
			{

				var currentSystem = mouseOverMinimapSystems[0];

				//var brushBackground = new SolidBrush(Color.Navy);
				var brush = GDICache.GetBrush(Program.MainConfiguration.OverlayMinimapMouseoverSystemColour);
				using (var font = new Font("Segoe UI", 12f, FontStyle.Regular))
					g.DrawString($"{currentSystem.solarSystem.Name}", font, brush, mouseOverPoint);

				if (MouseOver.Visible)
				{
					using (var gMouseOver = Graphics.FromImage(_mouseOverImage))
					{
						gMouseOver.Clear(Color.Transparent);
						var borderPen = GDICache.GetPen(MouseOver.ForeColor, 1);
						var FillBrush = GDICache.GetBrush(MouseOver.BackColor);
						gMouseOver.FillRectangle(FillBrush, 0, 0, MouseOver.Width - 1, MouseOver.Height - 1);


						using (Pen glow1 = new Pen(Color.FromArgb(60, MouseOver.ForeColor)))
						using (Pen glow2 = new Pen(Color.FromArgb(30, MouseOver.ForeColor)))
						using (Pen glow3 = new Pen(Color.FromArgb(15, MouseOver.ForeColor)))
						{
							// Outer border
							gMouseOver.DrawRectangle(borderPen, 0, 0, MouseOver.Width - 1, MouseOver.Height - 1);

							// Inner glow
							gMouseOver.DrawRectangle(glow1, 1, 1, MouseOver.Width - 3, MouseOver.Height - 3);
							gMouseOver.DrawRectangle(glow2, 2, 2, MouseOver.Width - 5, MouseOver.Height - 5);
							gMouseOver.DrawRectangle(glow3, 3, 3, MouseOver.Width - 7, MouseOver.Height - 7);
						}

						//gMouseOver.DrawRectangle(borderPen, 0, 0, MouseOver.Width - 1, MouseOver.Height - 1);


						gMouseOver.DrawString($"{currentSystem.solarSystem.Name}", MouseOverSystemName.Font, GDICache.GetBrush(MouseOverSystemName.ForeColor), MouseOverSystemName.Left, MouseOverSystemName.Top);
						gMouseOver.DrawString($"{Math.Round(currentSystem.solarSystem.Security, 1).ToString("F1")}", MouseOverSecurity.Font, GDICache.GetBrush(MouseOverSecurity.ForeColor), MouseOverSecurity.Left, MouseOverSecurity.Top);

						gMouseOver.DrawString($"{currentSystem.solarSystem.GetNPCKillsDeltaLastHour()}", MouseOverNPCDelta.Font, GDICache.GetBrush(MouseOverNPCDelta.ForeColor), MouseOverNPCDelta.Left, MouseOverNPCDelta.Top);
						gMouseOver.DrawString($"{currentSystem.solarSystem.CountKills()}", MouseOverKills.Font, GDICache.GetBrush(MouseOverKills.ForeColor), MouseOverKills.Left, MouseOverKills.Top);
						gMouseOver.DrawString($"{currentSystem.solarSystem.CountAlerts(false,false,false)}", MouseOverAlerts.Font, GDICache.GetBrush(MouseOverAlerts.ForeColor), MouseOverAlerts.Left, MouseOverAlerts.Top);

						gMouseOver.DrawString($"J:{currentSystem.solarSystem.JumpsPerHour}", MouseOverJumpsPerHour.Font, GDICache.GetBrush(MouseOverJumpsPerHour.ForeColor), MouseOverJumpsPerHour.Left, MouseOverJumpsPerHour.Top);
						gMouseOver.DrawString($"K:{currentSystem.solarSystem.ShipKillsPerHour}/{currentSystem.solarSystem.PodKillsPerHour}", MouseOverKillsPerHour.Font, GDICache.GetBrush(MouseOverKillsPerHour.ForeColor), MouseOverKillsPerHour.Left, MouseOverJumpsPerHour.Top);

						// Skyhooks ?
						DateTime utcNow = DateTime.UtcNow;
						DateTime? skyhookExpiry = null;
						string skyhookinfo = "";

						if (currentSystem.solarSystem.Planets != null)
						{
							foreach (var p in currentSystem.solarSystem?.Planets.Where(o => o.SkyhookVulneralbilityStart != null && o.SkyhookVulneralbilityEnd >= utcNow))
							{

								if (
									(p.SkyhookVulneralbilityStart > utcNow || p.SkyhookVulneralbilityStart <= utcNow) &&
									(skyhookExpiry == null || skyhookExpiry > p.SkyhookVulneralbilityEnd)
									)
								{
									skyhookExpiry = p.SkyhookVulneralbilityEnd;
									skyhookinfo = $"P{p.PlanetName} {p.PlanetTypeName.Replace("Planet", "")}";
									if (p.SkyhookVulneralbilityStart > utcNow)
									{
										TimeSpan difference = (DateTime)p.SkyhookVulneralbilityStart - utcNow;
										skyhookinfo += $" raid in\n{TimeSpanToFriendlyString(difference)}";
									}
									if (p.SkyhookVulneralbilityStart <= utcNow)
									{
										TimeSpan difference = (DateTime)p.SkyhookVulneralbilityEnd - utcNow;
										skyhookinfo += $" raid for\n{TimeSpanToFriendlyString(difference)}";
									}
								}
							}
							if (skyhookExpiry != null)
							{
								gMouseOver.DrawString($"{skyhookinfo}", MouseOverSkyhookInfo.Font, GDICache.GetBrush(MouseOverSkyhookInfo.ForeColor), MouseOverSkyhookInfo.Left, MouseOverSkyhookInfo.Top);
							}
						}







						var x = MouseOver.Left;
						var y = MouseOver.Top;
						if (x == -1 && y == -1)
						{
							// calculate x/y based on mouse posisiton and size of window
							x = mouseOverPoint.X+2; y = mouseOverPoint.Y+2;
							if (x + MouseOver.Width > Width) x = Width - MouseOver.Width - 2;
							if (y + MouseOver.Height > Height) y = Height - MouseOver.Height - 2;
						}

						// Draw MouseOver -> main canvas now
						g.DrawImage(
							_mouseOverImage,
							new Rectangle(x, y, MouseOver.Width, MouseOver.Height),
							0, 0, _mouseOverImage.Width, _mouseOverImage.Height,
							GraphicsUnit.Pixel,
							ImageAttrMouseOverGraph
						);

					}
				}
			}



		}

		public void UpdateMapData()
		{
			string Toon = "";
			if (!string.IsNullOrEmpty(_main.CurrentActiveToon))
				{
				Toon = _main.CurrentActiveToon;
			}

			if (string.IsNullOrEmpty(Toon))
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

			var c = Program.CharacterManager.Get(Toon);
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

			Image minimapImage = ImageGenerator.ClipMiniMap(new Bitmap(Minimap.Width, Minimap.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb), _dummyPath, Program.MainConfiguration.OverlayMapMapScale,
				ref extraInformation, ref MinimapSystems,
				_animationManager[AnimationType.Intel].GetNextValue(),
				_animationManager[AnimationType.Kills].GetNextValue(),
				_animationManager[AnimationType.Alts].GetNextValue(),
				_animationManager[AnimationType.UniHoles].GetNextValue(),
				_animationManager[AnimationType.Incursion].GetNextValue(),
				_animationManager[AnimationType.SecLimit].GetNextValue(),
				_animationManager[AnimationType.Skyhooks].GetNextValue(),

				_minimapParameters
				);

			var oldImage = _sourceMapImage;
			_sourceMapImage = minimapImage;
			oldImage?.Dispose();
			RenderAndUpdate();
		}


		protected override void OnOverlayMouseMove(Point clientLocation)
		{
			var hovered = Program.Universe.MouseOverSystem(
				clientLocation.X,
				clientLocation.Y,
				MinimapSystems);

			if (!mouseOverMinimapSystems.SequenceEqual(hovered))
			{
				foreach (var s in hovered)
				{
					Program.Universe.GetSystemInfo((int)s.solarSystem.Id);
				}

				mouseOverMinimapSystems = hovered;
				mouseOverPoint = new Point(
					clientLocation.X + 6,
					clientLocation.Y + 4);

				RenderAndUpdate();
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
