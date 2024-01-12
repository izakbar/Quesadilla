namespace Quesadilla
{
    partial class Quesadilla
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Quesadilla));
            this.mainTimer = new System.Windows.Forms.Timer(this.components);
            this.intelLog = new Serilog.Sinks.WinForms.RichTextBoxLogControl();
            this.alertsOutstanding = new System.Windows.Forms.TextBox();
            this.ChatServerDown = new System.Windows.Forms.Label();
            this.CombatFlag = new System.Windows.Forms.Label();
            this.buttonIntelSize = new System.Windows.Forms.Button();
            this.chartDPS = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chartDPS)).BeginInit();
            this.SuspendLayout();
            // 
            // mainTimer
            // 
            this.mainTimer.Enabled = true;
            this.mainTimer.Interval = 250;
            this.mainTimer.Tick += new System.EventHandler(this.mainTimer_Tick);
            // 
            // intelLog
            // 
            this.intelLog.BackColor = System.Drawing.SystemColors.Info;
            this.intelLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.intelLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.intelLog.Enabled = false;
            this.intelLog.ForContext = "";
            this.intelLog.Location = new System.Drawing.Point(0, 276);
            this.intelLog.Name = "intelLog";
            this.intelLog.Size = new System.Drawing.Size(1116, 286);
            this.intelLog.TabIndex = 0;
            this.intelLog.Text = "";
            // 
            // alertsOutstanding
            // 
            this.alertsOutstanding.BackColor = System.Drawing.SystemColors.Control;
            this.alertsOutstanding.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.alertsOutstanding.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.alertsOutstanding.ForeColor = System.Drawing.Color.Crimson;
            this.alertsOutstanding.Location = new System.Drawing.Point(0, -2);
            this.alertsOutstanding.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.alertsOutstanding.Name = "alertsOutstanding";
            this.alertsOutstanding.Size = new System.Drawing.Size(66, 60);
            this.alertsOutstanding.TabIndex = 2;
            // 
            // ChatServerDown
            // 
            this.ChatServerDown.AutoSize = true;
            this.ChatServerDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChatServerDown.ForeColor = System.Drawing.Color.Red;
            this.ChatServerDown.Location = new System.Drawing.Point(13, 100);
            this.ChatServerDown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ChatServerDown.Name = "ChatServerDown";
            this.ChatServerDown.Size = new System.Drawing.Size(223, 29);
            this.ChatServerDown.TabIndex = 3;
            this.ChatServerDown.Text = "Chat Server Down";
            this.ChatServerDown.Visible = false;
            // 
            // CombatFlag
            // 
            this.CombatFlag.AutoSize = true;
            this.CombatFlag.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CombatFlag.ForeColor = System.Drawing.Color.Red;
            this.CombatFlag.Location = new System.Drawing.Point(935, 239);
            this.CombatFlag.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CombatFlag.Name = "CombatFlag";
            this.CombatFlag.Size = new System.Drawing.Size(131, 29);
            this.CombatFlag.TabIndex = 4;
            this.CombatFlag.Text = "In Combat";
            this.CombatFlag.Visible = false;
            // 
            // buttonIntelSize
            // 
            this.buttonIntelSize.BackColor = System.Drawing.Color.Transparent;
            this.buttonIntelSize.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonIntelSize.Location = new System.Drawing.Point(1090, 274);
            this.buttonIntelSize.Name = "buttonIntelSize";
            this.buttonIntelSize.Size = new System.Drawing.Size(26, 28);
            this.buttonIntelSize.TabIndex = 6;
            this.buttonIntelSize.Text = "▼";
            this.buttonIntelSize.UseVisualStyleBackColor = false;
            this.buttonIntelSize.Click += new System.EventHandler(this.buttonIntelSize_Click);
            // 
            // chartDPS
            // 
            this.chartDPS.BackColor = System.Drawing.Color.Transparent;
            this.chartDPS.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDot;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.BackColor = System.Drawing.Color.Transparent;
            chartArea1.Name = "ChartArea1";
            this.chartDPS.ChartAreas.Add(chartArea1);
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.IsDockedInsideChartArea = false;
            legend1.LegendStyle = System.Windows.Forms.DataVisualization.Charting.LegendStyle.Row;
            legend1.Name = "Legend1";
            this.chartDPS.Legends.Add(legend1);
            this.chartDPS.Location = new System.Drawing.Point(355, -2);
            this.chartDPS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chartDPS.Name = "chartDPS";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartDPS.Series.Add(series1);
            this.chartDPS.Size = new System.Drawing.Size(743, 270);
            this.chartDPS.TabIndex = 7;
            this.chartDPS.Text = "DPS";
            this.chartDPS.Click += new System.EventHandler(this.chartDPS_Click);
            // 
            // Quesadilla
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1116, 562);
            this.ControlBox = false;
            this.Controls.Add(this.buttonIntelSize);
            this.Controls.Add(this.CombatFlag);
            this.Controls.Add(this.ChatServerDown);
            this.Controls.Add(this.alertsOutstanding);
            this.Controls.Add(this.intelLog);
            this.Controls.Add(this.chartDPS);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Quesadilla";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Quesadilla_FormClosing);
            this.Load += new System.EventHandler(this.Quesadilla_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chartDPS)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer mainTimer;
        private Serilog.Sinks.WinForms.RichTextBoxLogControl intelLog;
        private System.Windows.Forms.TextBox alertsOutstanding;
        private System.Windows.Forms.Label ChatServerDown;
        private System.Windows.Forms.Label CombatFlag;
        private System.Windows.Forms.Button buttonIntelSize;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartDPS;
    }
}

