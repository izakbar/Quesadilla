using Quesadilla.Classes;
using Quesadilla.Classes.Extensions;
using Quesadilla.Classes.GraphData;
using Quesadilla.Classes.ImageGeneration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Quesadilla
{
	public partial class QuesadillaOverlayDPS : TransparentOverlayForm
	{
		private Quesadilla _main;
		private Image _sourceDPSImage = null;
		public Chart dpsChart = new Chart() { Name = "DPSChart" };
		private Dictionary<int, CombatGraphData> _combatGraphData = new Dictionary<int, CombatGraphData>();
		private ImageAttributes ImageAttrDPS = new ImageAttributes();
		private Timer chartTimer = null;
		private readonly Font axisFont = new Font("Consolas", 10F, FontStyle.Bold);

		public QuesadillaOverlayDPS(Quesadilla mainForm) : base()
		{
			_main = mainForm;
			InitializeComponent();
			TopMost = true;

			Text = "DPS";
			Width = Math.Max(100, Program.MainConfiguration.OverlayDPSWindowSize.Width);
			Height = Math.Max(50, Program.MainConfiguration.OverlayDPSWindowSize.Height); 
			SetScreenPosition();

			Controls.Add(dpsChart);

			dpsChart.Width = Math.Max(Program.MainConfiguration.OverlayDPSWindowSize.Width, 100);
			dpsChart.Height = Math.Max(Program.MainConfiguration.OverlayDPSWindowSize.Height, 50);

			ImageGenerator.SetImageAlpha(ImageAttrDPS,Program.MainConfiguration.OverlayDPSGraphAlpha);
			_sourceDPSImage = new Bitmap(dpsChart.Width, dpsChart.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

			SetupDPSChart();
		}

		private void SetupDPSChart()
		{
			ChartArea chartArea = new ChartArea("ISK");
			chartArea.BackColor = Color.Transparent;
			dpsChart.ChartAreas.Add(chartArea);
			dpsChart.BackColor = Color.Transparent;

			dpsChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			dpsChart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

			dpsChart.ChartAreas[0].AxisX.IsMarginVisible = false;

			dpsChart.Dock = DockStyle.Fill;
			dpsChart.BorderlineWidth = 0;
			dpsChart.BorderlineDashStyle = ChartDashStyle.DashDot;
			dpsChart.AntiAliasing = AntiAliasingStyles.All;
			dpsChart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

			chartTimer = new Timer()
			{
				Interval = 1000,
				Enabled = false,
			};
			chartTimer.Tick += (s, e) => UpdateDPSChart();
			chartTimer.Start();
		}

		private Series AddChartSeries(
			Chart chart,
			string name,
			SeriesChartType type,
			Color color,
			AxisType axisType,
			int lineSize,
			ChartDashStyle dashStyle)
		{
			var series = new Series
			{
				Name = name,
				ChartType = type,
				Color = color,
				YAxisType = axisType,
				BorderWidth = lineSize,
				BorderDashStyle = dashStyle
			};

			series.Points.Clear();

			chart.Series.Add(series);
			return series;
		}


		private void UpdateDPSChart()
		{
			using (Graphics gDPS = Graphics.FromImage(_sourceDPSImage))
			{
				gDPS.Clear(Color.Transparent);
			}

			dpsChart.Series.Clear();
			string Toon = "";

			if (!string.IsNullOrEmpty(_main.CurrentActiveToon))
			{
				Toon = _main.CurrentActiveToon;
			}

			if (string.IsNullOrEmpty(Toon) || !Program.GraphDataManager.GetCombatDataKeys().Contains(Toon))
			{
				dpsChart.Visible = false;
				return;
			}
			else
			{
				dpsChart.Visible = true;
			}

			if (Program.MainConfiguration.DPSAllToons)
			{
				Toon = "*ALL*";
			}

			int maxPoints = Program.MainConfiguration.GraphCombatMaxPoints;
			int minSpanSize = Program.MainConfiguration.GraphCombatMinPointSeconds;

			int LineSize = Program.MainConfiguration.DPSGraphLineSize;

			var toon1Received = AddChartSeries(dpsChart,$"{Toon} IN",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphDPSInColour,AxisType.Primary,LineSize,ChartDashStyle.Solid);
			var toon1Given = AddChartSeries(dpsChart,"OUT",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphDPSOutColour,AxisType.Primary,LineSize,ChartDashStyle.Solid);
			var toon1NeutReceived = AddChartSeries(dpsChart,"NeutIN",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphNeutInColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1NeutGiven = AddChartSeries(dpsChart,"NeutOUT",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphNeutOutColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1ShieldReceived = AddChartSeries(dpsChart,"LogiS IN",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphLogiShieldsInColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1ShieldGiven = AddChartSeries(dpsChart,"LogiS OUT",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphLogiShieldsOutColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1ArmorReceived = AddChartSeries(dpsChart,"LogiA IN",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphLogiArmorInColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1ArmorGiven = AddChartSeries(dpsChart,"LogiA OUT",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphLogiArmorOutColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1HullReceived = AddChartSeries(dpsChart,"LogiH IN",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphLogiHullInColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1HullGiven = AddChartSeries(dpsChart,"LogiH OUT",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphLogiHullOutColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1CapReceived = AddChartSeries(dpsChart,"LogiC IN",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphLogiCapacitorInColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);
			var toon1CapGiven = AddChartSeries(dpsChart,"LogiC OUT",SeriesChartType.Spline,Program.MainConfiguration.DPSGraphLogiCapacitorOutColour,AxisType.Secondary,LineSize,ChartDashStyle.Solid);

			dpsChart.ChartAreas[0].AxisX.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
			dpsChart.ChartAreas[0].AxisX.InterlacedColor = System.Drawing.Color.Lime;
			//dpsChart.ChartAreas[0].AxisX.LabelStyle.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dpsChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			dpsChart.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
			dpsChart.ChartAreas[0].AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
			dpsChart.ChartAreas[0].AxisY.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
			dpsChart.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.DimGray;
			dpsChart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
			dpsChart.ChartAreas[0].AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
			dpsChart.ChartAreas[0].AxisY2.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
			dpsChart.ChartAreas[0].AxisY2.MajorGrid.LineColor = System.Drawing.Color.DimGray;
			dpsChart.ChartAreas[0].AxisY2.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;

			dpsChart.ChartAreas[0].AxisY2.IsLabelAutoFit = false;
			dpsChart.ChartAreas[0].AxisY.IsLabelAutoFit = false;
			dpsChart.ChartAreas[0].AxisY2.IsLabelAutoFit = false;

			dpsChart.ChartAreas[0].AxisY.LabelStyle.Font = axisFont;
			dpsChart.ChartAreas[0].AxisY2.LabelStyle.Font = axisFont;

			_combatGraphData.Clear();

			DateTime end = DateTime.Now;
			DateTime curr = end.AddSeconds(-1 * Program.MainConfiguration.GraphCombatSeconds);
			DateTime start = curr;

			var span = 10; // 10 second buckets - this is the smoothing factor for the graph - 10 seconds is a good compromise between smoothness and responsiveness

			foreach (var currToon in Program.GraphDataManager.GetCombatDataKeys())
			{
				if (currToon == Toon || Toon == "*ALL*")
				{
					// setup second slots
					foreach (var c in Program.GraphDataManager.GetCombatDataLines(currToon) /*.OrderBy(o => o.TimeAdded).ToList() */ )
					{
						var slot = (int)Math.Truncate((c.TimeAdded.TrimToSeconds(1) - start.TrimToSeconds(1)).TotalSeconds);
						if (slot < 0) continue;
						if (!_combatGraphData.ContainsKey(slot))
						{
							// slot does not exist - create
							_combatGraphData[slot] = new CombatGraphData()
							{
								DamageGiven = 0,
								DamageReceived = 0,
								NeutsGiven = 0,
								NeutsReceived = 0,
							};
						}
						if (c.Received)
						{
							_combatGraphData[slot].DamageReceived += c.DamageAmount;
							_combatGraphData[slot].NeutsReceived += c.NeutAmount;
							_combatGraphData[slot].ShieldReceived += c.ShieldAmount;
							_combatGraphData[slot].ArmorReceived += c.ArmorAmount;
							_combatGraphData[slot].HullReceived += c.HullAmount;
							_combatGraphData[slot].CapReceived += c.CapAmount;
						}
						else
						{
							_combatGraphData[slot].DamageGiven += c.DamageAmount;
							_combatGraphData[slot].NeutsGiven += c.NeutAmount;
							_combatGraphData[slot].ShieldGiven += c.ShieldAmount;
							_combatGraphData[slot].ArmorGiven += c.ArmorAmount;
							_combatGraphData[slot].HullGiven += c.HullAmount;
							_combatGraphData[slot].CapGiven += c.CapAmount;
						}
					}
				}
			}

			var graphStartXp = (int)Math.Truncate((curr - start).TotalSeconds);
			if (graphStartXp < 0) graphStartXp = 0;
			var graphEndXp = (int)Math.Truncate((end - start).TotalSeconds);

			dpsChart.ChartAreas[0].AxisX.Minimum = graphStartXp + span + 1 > graphEndXp ? graphStartXp : graphStartXp + span + 1;
			dpsChart.ChartAreas[0].AxisX.Maximum = graphEndXp;
			dpsChart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

			double maxDReceived = 0; double maxDGiven = 0;
			//while (curr <= end)
			while (curr <= end)
			{
				// curr is mid point of damage
				// need to calc prev and next point - based on stepsize
				var p = (int)Math.Truncate((curr.AddSeconds(span * -1) - start).TotalSeconds);
				if (p < 0) p = 0;
				var n = (int)Math.Truncate((curr.AddSeconds(span) - start).TotalSeconds);
				// bounds check n ?

				var seconds = (int)Math.Truncate((curr - start).TotalSeconds);

				double dReceived = 0; double dGiven = 0;
				double nReceived = 0; double nGiven = 0;

				double sReceived = 0; double sGiven = 0;
				double aReceived = 0; double aGiven = 0;
				double hReceived = 0; double hGiven = 0;
				double cReceived = 0; double cGiven = 0;

				curr = curr.AddSeconds(span);
				for (var i = p; i <= n; i++)
				{
					if (_combatGraphData.ContainsKey(i))
					{
						dReceived += _combatGraphData[i].DamageReceived;
						dGiven += _combatGraphData[i].DamageGiven;

						nReceived += _combatGraphData[i].NeutsReceived;
						nGiven += _combatGraphData[i].NeutsGiven;

						sReceived += _combatGraphData[i].ShieldReceived;
						sGiven += _combatGraphData[i].ShieldGiven;

						aReceived += _combatGraphData[i].ArmorReceived;
						aGiven += _combatGraphData[i].ArmorGiven;

						hReceived += _combatGraphData[i].HullReceived;
						hGiven += _combatGraphData[i].HullGiven;

						cReceived += _combatGraphData[i].CapReceived;
						cGiven += _combatGraphData[i].CapGiven;
					}
				}
				if (seconds > 0)
				{
					var _span_divide = (span * 2)+1;
					if (dReceived > 0)
					{
						var dpsReceived = dReceived / _span_divide;
						toon1Received.Points.AddXY(seconds, dpsReceived);
						maxDReceived = Math.Max(dpsReceived, maxDReceived);
					}
					if (dGiven > 0)
					{
						var dpsGiven = dGiven / _span_divide;
						toon1Given.Points.AddXY(seconds, dpsGiven);
						maxDGiven = Math.Max(dpsGiven, maxDGiven);
					}

					if (nReceived > 0) toon1NeutReceived.Points.AddXY(seconds, nReceived / _span_divide);
					if (nGiven > 0) toon1NeutGiven.Points.AddXY(seconds, nGiven / _span_divide);

					if (sReceived > 0) toon1ShieldReceived.Points.AddXY(seconds, sReceived / _span_divide);
					if (sGiven > 0) toon1ShieldGiven.Points.AddXY(seconds, sGiven / _span_divide);

					if (aReceived > 0) toon1ArmorReceived.Points.AddXY(seconds, aReceived / _span_divide);
					if (aGiven > 0) toon1ArmorGiven.Points.AddXY(seconds, aGiven / _span_divide);

					if (hReceived > 0) toon1HullReceived.Points.AddXY(seconds, hReceived / _span_divide);
					if (hGiven > 0) toon1HullGiven.Points.AddXY(seconds, hGiven / _span_divide);

					if (cReceived > 0) toon1CapReceived.Points.AddXY(seconds, cReceived / _span_divide);
					if (cGiven > 0) toon1CapGiven.Points.AddXY(seconds, cGiven / _span_divide);

				}
			}

			bool secondY = false;
			foreach (var s in dpsChart.Series)
			{
				if (s.Points.Count == 0)
				{
					s.Enabled = false;
				}
				else
				{
					s.Enabled = true;
					if (s.YAxisType == AxisType.Secondary)
					{
						secondY = true;
					}
				}
			}

			dpsChart.ChartAreas[0].Axes[0].LabelStyle.Format = "{0:0,.#}K";
			dpsChart.ChartAreas[0].Axes[1].LabelStyle.Format = "{0:0,.#}K";
			dpsChart.ChartAreas[0].Axes[2].LabelStyle.Format = "{0:0,.#}K";
			dpsChart.ChartAreas[0].Axes[3].LabelStyle.Format = "{0:0,.#}K";


			// sort out YLabels 
			if (maxDGiven > 1500 || maxDReceived > 1500)
			{
				dpsChart.ChartAreas[0].Axes[1].LabelStyle.Format = "{0:0,.#}K";
				dpsChart.ChartAreas[0].AxisY.Interval =
					Math.Ceiling((dpsChart.ChartAreas[0].AxisY.Maximum / 3.0) / 1000.0) * 1000.0;

			}
			else
			{
				dpsChart.ChartAreas[0].Axes[1].LabelStyle.Format = "";

				var interval = Math.Max(100,Math.Ceiling((dpsChart.ChartAreas[0].AxisY.Maximum / 3.0) / 100.0) * 100.0);
				dpsChart.ChartAreas[0].AxisY.Interval = interval;
			}

			if (secondY)
			{
				dpsChart.ChartAreas[0].Axes[3].Enabled = AxisEnabled.True;
			}
			else
			{
				dpsChart.ChartAreas[0].Axes[3].Enabled = AxisEnabled.False;
			}

			if (_sourceDPSImage == null ||
				_sourceDPSImage.Width != dpsChart.Width ||
				_sourceDPSImage.Height != dpsChart.Height)
			{
				_sourceDPSImage?.Dispose();

				_sourceDPSImage = new Bitmap(dpsChart.Width, dpsChart.Height, PixelFormat.Format32bppArgb);
			}

			dpsChart.DrawToBitmap(
				(Bitmap)_sourceDPSImage,
				new Rectangle(0, 0, _sourceDPSImage.Width, _sourceDPSImage.Height));

			// dump to csv
			if (Program.MainConfiguration.DPSSaveCSV)
			{
				using (var file = File.CreateText((Toon == "*ALL*" ? "All" : Toon) + "_raw_combat.csv"))
				{
					foreach (var a in _combatGraphData.Keys)
					{
						file.WriteLine(a + "," + _combatGraphData[a].ToCSV());
					}
				}
			}
			RenderAndUpdate();
		}

		protected override void RenderOverlay(Graphics g)
		{
			if (_sourceDPSImage != null)
			{
				g.DrawImage(
					_sourceDPSImage,
					new Rectangle(0, 0, Width, Height),
					0, 0, _sourceDPSImage.Width, _sourceDPSImage.Height,
					GraphicsUnit.Pixel,
					ImageAttrDPS
				);
			}
		}
		public void SetScreenPosition()
		{
			// position on screen
			Location = new Point()
			{
				X = _main.currentEveWindowRect.Left + Program.MainConfiguration.OverlayDPSWindowPosition.X,
				Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + Program.MainConfiguration.OverlayDPSWindowPosition.Y,
			};
		}
	}
}
