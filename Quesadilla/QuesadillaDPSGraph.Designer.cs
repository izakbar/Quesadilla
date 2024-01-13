namespace Quesadilla
{
    partial class QuesadillaDPSGraph
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chartDPS = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chartDPS)).BeginInit();
            this.SuspendLayout();
            // 
            // chartDPS
            // 
            this.chartDPS.BackColor = System.Drawing.Color.Transparent;
            this.chartDPS.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDot;
            this.chartDPS.BorderlineWidth = 0;
            chartArea2.AxisX.InterlacedColor = System.Drawing.Color.Lime;
            chartArea2.AxisX.IsLabelAutoFit = false;
            chartArea2.AxisX.LabelStyle.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            chartArea2.AxisX.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
            chartArea2.AxisX.MajorGrid.Enabled = false;
            chartArea2.AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chartArea2.AxisY.IsLabelAutoFit = false;
            chartArea2.AxisY.LabelStyle.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisY.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
            chartArea2.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea2.AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea2.AxisY2.IsLabelAutoFit = false;
            chartArea2.AxisY2.LabelStyle.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            chartArea2.AxisY2.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
            chartArea2.AxisY2.MajorGrid.Enabled = false;
            chartArea2.BackColor = System.Drawing.Color.Transparent;
            chartArea2.Name = "ChartArea1";
            this.chartDPS.ChartAreas.Add(chartArea2);
            this.chartDPS.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.BackColor = System.Drawing.Color.Transparent;
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend2.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            legend2.ForeColor = System.Drawing.Color.Tomato;
            legend2.IsDockedInsideChartArea = false;
            legend2.IsTextAutoFit = false;
            legend2.LegendStyle = System.Windows.Forms.DataVisualization.Charting.LegendStyle.Row;
            legend2.Name = "Legend1";
            this.chartDPS.Legends.Add(legend2);
            this.chartDPS.Location = new System.Drawing.Point(0, 0);
            this.chartDPS.Name = "chartDPS";
            series2.ChartArea = "ChartArea1";
            series2.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            series2.LabelForeColor = System.Drawing.Color.Tomato;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartDPS.Series.Add(series2);
            this.chartDPS.Size = new System.Drawing.Size(559, 167);
            this.chartDPS.TabIndex = 8;
            this.chartDPS.Text = "DPS";
            this.chartDPS.Click += new System.EventHandler(this.chartDPS_Click);
            // 
            // QuesadillaDPSGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 167);
            this.Controls.Add(this.chartDPS);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "QuesadillaDPSGraph";
            this.Text = "Queadilla DPS";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.QuesadillaDPSGraph_FormClosed);
            this.Load += new System.EventHandler(this.QuesadillaDPSGraph_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chartDPS)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chartDPS;
    }
}