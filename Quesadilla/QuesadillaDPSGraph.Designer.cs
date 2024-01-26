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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chartDPS = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chartDPS)).BeginInit();
            this.SuspendLayout();
            // 
            // chartDPS
            // 
            this.chartDPS.BackColor = System.Drawing.Color.Transparent;
            this.chartDPS.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDot;
            this.chartDPS.BorderlineWidth = 0;
            chartArea1.AxisX.InterlacedColor = System.Drawing.Color.Lime;
            chartArea1.AxisX.IsLabelAutoFit = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
            chartArea1.AxisY.LabelStyle.Format = "{0:0,.#}K";
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.DimGray;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea1.AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisY2.IsLabelAutoFit = false;
            chartArea1.AxisY2.LabelStyle.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY2.LabelStyle.ForeColor = System.Drawing.Color.Tomato;
            chartArea1.AxisY2.LabelStyle.Format = "{0:0,}K";
            chartArea1.AxisY2.MajorGrid.Enabled = false;
            chartArea1.AxisY2.MajorGrid.LineColor = System.Drawing.Color.DimGray;
            chartArea1.AxisY2.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.BackColor = System.Drawing.Color.Transparent;
            chartArea1.Name = "ChartArea1";
            this.chartDPS.ChartAreas.Add(chartArea1);
            this.chartDPS.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Font = new System.Drawing.Font("Consolas", 9F);
            legend1.ForeColor = System.Drawing.Color.Tomato;
            legend1.IsDockedInsideChartArea = false;
            legend1.IsTextAutoFit = false;
            legend1.LegendStyle = System.Windows.Forms.DataVisualization.Charting.LegendStyle.Row;
            legend1.Name = "Legend1";
            this.chartDPS.Legends.Add(legend1);
            this.chartDPS.Location = new System.Drawing.Point(0, 0);
            this.chartDPS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chartDPS.Name = "chartDPS";
            series1.ChartArea = "ChartArea1";
            series1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            series1.LabelForeColor = System.Drawing.Color.Tomato;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartDPS.Series.Add(series1);
            this.chartDPS.Size = new System.Drawing.Size(838, 257);
            this.chartDPS.TabIndex = 8;
            this.chartDPS.Text = "DPS";
            this.chartDPS.Click += new System.EventHandler(this.chartDPS_Click);
            // 
            // QuesadillaDPSGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 257);
            this.Controls.Add(this.chartDPS);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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