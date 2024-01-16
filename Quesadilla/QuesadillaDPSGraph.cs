using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Quesadilla
{
    public partial class QuesadillaDPSGraph : Form
    {
        private Quesadilla _main;
        private Dictionary<int, CombatGraphData> _combatGraphData = new Dictionary<int, CombatGraphData>();
        public Color TransparentColour =Color.FromArgb(237, 28, 36);
        //Color.Purple;

        public QuesadillaDPSGraph(Quesadilla mainForm)
        {
            _main = mainForm;
            TopMost = _main.Configuration.DPSGraphStayOnTop;
            if ( _main.Configuration.DPSGraphTransparent )
            {
                this.TransparencyKey = this.BackColor = TransparentColour;
            }
            InitializeComponent();

            if ( _main.Configuration.DPSGraphTransparent )
            {
                this.Text = "";
                this.ControlBox = false;
                this.MinimizeBox = false;
                this.FormBorderStyle = FormBorderStyle.None;
            }


            Width = _main.Configuration.DPSGraphWindowSize.X <= 100 ? 100 : _main.Configuration.DPSGraphWindowSize.X;
            Height = _main.Configuration.DPSGraphWindowSize.Y <= 50 ? 50 : _main.Configuration.DPSGraphWindowSize.Y;

            SetScreenPosition();
        }

        public void SetScreenPosition()
        {
            // position on screen
            if ( _main.Configuration.DPSGraphTransparent )
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
                            X = (_main.currentEveWindowRect.Right / 2) - (Width / 2),
                            Y = (_main.currentEveWindowRect.Bottom / 2) - (Height / 2),
                        };
                        break;
                }
            }
            else
            {
                // use screen position
                Screen screen = Screen.FromControl(this);
                switch (_main.Configuration.DPSLocation)
                {
                    case PopupLocationType.TopLeft:
                        Location = new Point()
                        {
                            X = screen.WorkingArea.Left,
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
        private void QuesadillaDPSGraph_Load(object sender, EventArgs e)
        {
            if (_main.Configuration.DPSGraphTransparent)
            {
                this.chartDPS.BackColor = this.BackColor;
                ShowInTaskbar = false;
                this.chartDPS.AntiAliasing = AntiAliasingStyles.None;
            }
            chartDPS.Legends.First().Enabled = _main.Configuration.DPSGraphLegends;
        }

        public void UpdateDPSGraph()
        {
            int maxPoints = _main.Configuration.GraphCombatMaxPoints;
            int minSpanSize = _main.Configuration.GraphCombatMinPointSeconds;

            int LineSize = 1;
            if ( _main.Configuration.DPSGraphTransparent )
            {
                LineSize = 3;
            }

            // default to total dps
            chartDPS.Series.Clear();

            string Toon = "";

            /*
            // pick first toon in list;
            Toon = _main._combat.Keys.First().ToString();

            // pick last active dps
            DateTime LastCombat = DateTime.Now.AddDays(-1);
            if ( _main._combat.Keys.Count > 1 )
            {
                // find toon with latest combat on
                foreach(var x in _main._combat.Keys)
                {
                    if (_main._combat[x].Count > 0 )
                    {
                        if (_main._combat[x].Last().TimeAdded > LastCombat )
                        {
                            LastCombat = _main._combat[x].Last().TimeAdded;
                            Toon = x;
                        } 
                    }
                }
            }
            */

            // but wait - what if we have a currently active toon ?
            if ( _main.CurrentActiveToon != null && _main.CurrentActiveToon != "" )
            {
                Toon = _main.CurrentActiveToon;
            }

            if ( Toon == null || Toon == "" || !_main._combat.ContainsKey(Toon))
            {
                chartDPS.Visible = false;
                return;
            } else
            {
                chartDPS.Visible = true;
                // Set position based on eve window ?
                SetScreenPosition();
            }

            if ( _main.Configuration.DPSAllToons )
            {
                Toon = "*ALL*";
            }

            var toon1Received = chartDPS.Series.Add(Toon + " IN");
            toon1Received.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            toon1Received.Points.Clear();
            toon1Received.Color = Color.Red;
            toon1Received.YAxisType = AxisType.Primary;
            toon1Received.BorderWidth = LineSize;
            toon1Received.BorderDashStyle = ChartDashStyle.Solid;

            var toon1Given = chartDPS.Series.Add("OUT");
            toon1Given.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            toon1Given.Points.Clear();
            toon1Given.Color = Color.Green;
            toon1Given.YAxisType = AxisType.Primary;
            toon1Given.BorderWidth = LineSize;
            toon1Given.BorderDashStyle = ChartDashStyle.Solid;

            var toon1NeutReceived = chartDPS.Series.Add("NeutIN");
            toon1NeutReceived.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            toon1NeutReceived.Points.Clear();
            toon1NeutReceived.Color = Color.Orange;
            toon1NeutReceived.YAxisType = AxisType.Secondary;
            toon1NeutReceived.BorderWidth = LineSize;
            toon1NeutReceived.BorderDashStyle = ChartDashStyle.Solid;

            var toon1NeutGiven = chartDPS.Series.Add("NeutOUT");
            toon1NeutGiven.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            toon1NeutGiven.Points.Clear();
            toon1NeutGiven.Color = Color.YellowGreen;
            toon1NeutGiven.YAxisType = AxisType.Secondary;
            toon1NeutGiven.BorderWidth = LineSize;
            toon1NeutGiven.BorderDashStyle = ChartDashStyle.Solid;

            _combatGraphData.Clear();

            DateTime start = DateTime.Now;
            DateTime end = start;

            // work out start/end times of combat - for cuirrent/all toons
            foreach (var currToon in _main._combat.Keys)
            {
                if (currToon == Toon || Toon == "*ALL*")
                {
                    if (_main._combat[currToon].Count > 0)
                    {
                        if (_main._combat[currToon].First().TimeAdded < start) start = _main._combat[currToon].First().TimeAdded;
                        if (_main._combat[currToon].Last().TimeAdded > end) end = _main._combat[currToon].Last().TimeAdded;
                    }
                }
            }
            TimeSpan totalTime = end - start;


            DateTime curr = start;
            // check range
            if (totalTime.TotalSeconds > _main.Configuration.GraphCombatSeconds)
            {
                curr = end.AddSeconds(-1 * _main.Configuration.GraphCombatSeconds);
                totalTime = end - curr;
            }

            var span = Math.Truncate(totalTime.TotalSeconds > maxPoints ? totalTime.TotalSeconds / maxPoints : 1);
            if (span < minSpanSize) span = minSpanSize;

            foreach (var currToon in _main._combat.Keys)
            {
                if (currToon == Toon || Toon == "*ALL*")
                {
                    // setup second slots
                    foreach (var c in _main._combat[currToon].OrderBy(o => o.TimeAdded).ToList())
                    {
                        var slot = (int)Math.Truncate((c.TimeAdded - start).TotalSeconds);
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
                        }
                        else
                        {
                            _combatGraphData[slot].DamageGiven += c.DamageAmount;
                            _combatGraphData[slot].NeutsGiven += c.NeutAmount;
                        }
                    }
                }
            }

            // now go thru from start -> end in slot seconds

            var graphStartXp = (int)Math.Truncate((curr - start).TotalSeconds);
            if (graphStartXp < 0) graphStartXp = 0;
            var graphEndXp = (int)Math.Truncate((end - start).TotalSeconds);

            chartDPS.ChartAreas[0].AxisX.Minimum = graphStartXp + span + 1 > graphEndXp ? graphStartXp : graphStartXp + span + 1;
            chartDPS.ChartAreas[0].AxisX.Maximum = graphEndXp;
            chartDPS.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

            while (curr <= end)
            {
                // curr is mid point of damage
                // need to calc prev and next point - based on stepsize
                var p = (int)Math.Truncate((curr.AddSeconds(span * -1) - start).TotalSeconds);
                if (p < 0) p = 0;
                var n = (int)Math.Truncate((curr.AddSeconds(span) - start).TotalSeconds);
                // bounds check n ?

                var seconds = (int)Math.Truncate((curr - start).TotalSeconds);

                double dReceived = 0;
                double dGiven = 0;
                double nReceived = 0;
                double nGiven = 0;
                curr = curr.AddSeconds(span);
                for (var i = p; i <= n; i++)
                {
                    if (_combatGraphData.ContainsKey(i))
                    {
                        dReceived += _combatGraphData[i].DamageReceived;
                        dGiven += _combatGraphData[i].DamageGiven;
                        nReceived += _combatGraphData[i].NeutsReceived;
                        nGiven += _combatGraphData[i].NeutsGiven;
                    }
                }
                if (seconds > 0)
                {
                    toon1Received.Points.AddXY(seconds, dReceived / (span * 2 + 1));
                    toon1Given.Points.AddXY(seconds, dGiven / (span * 2 + 1));

                    toon1NeutReceived.Points.AddXY(seconds, nReceived/ (span * 2 + 1) );
                    toon1NeutGiven.Points.AddXY(seconds, nGiven / (span * 2 + 1) );
                }
            }

            // dump to csv
            if (_main.Configuration.DPSSaveCSV)
            {
                using (var file = File.CreateText(Toon + "_raw_combat.csv"))
                {
                    foreach (var a in _combatGraphData.Keys)
                    {
                        file.WriteLine(a + "," + _combatGraphData[a].ToCSV());
                    }
                }
/*
 using (var file = File.CreateText(Toon + "_raw_combat.csv"))
                {
                    foreach (var a in toon1Received.Points)
                    {
                        file.WriteLine(a.XValue.ToString() + "," + a.YValues[0].ToString() + "," + graphStartXp.ToString() + "," + graphEndXp.ToString());
                    }
                }
*/
            }
        }

        private void chartDPS_Click(object sender, EventArgs e)
        {

        }

        private void QuesadillaDPSGraph_FormClosed(object sender, FormClosedEventArgs e)
        {
            _main.DPSGraphWindow = null;
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Set the form click-through
                if (_main != null)
                {
                    if (_main.Configuration.DPSGraphTransparent)
                    {
                        cp.ExStyle |=
                            0x80000 /* WS_EX_LAYERED */ |
                            0x20 /* WS_EX_TRANSPARENT */
                            ;
                    }
                }
                return cp;
            }
        }

    }
}
