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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Quesadilla));
            this.mainTimer = new System.Windows.Forms.Timer(this.components);
            this.intelLog = new Serilog.Sinks.WinForms.RichTextBoxLogControl();
            this.ChatServerDown = new System.Windows.Forms.Label();
            this.buttonIntelSize = new System.Windows.Forms.Button();
            this.lblCurrentClient = new System.Windows.Forms.Label();
            this.lblISK = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pMinimapImage = new System.Windows.Forms.PictureBox();
            this.lblAlertsOutstanding = new System.Windows.Forms.Label();
            this.lblEtxraInformation = new System.Windows.Forms.Label();
            this.lblSystemName = new System.Windows.Forms.Label();
            this.working = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pMinimapImage)).BeginInit();
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
            this.intelLog.ForContext = "";
            this.intelLog.Location = new System.Drawing.Point(0, 356);
            this.intelLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.intelLog.Name = "intelLog";
            this.intelLog.ReadOnly = true;
            this.intelLog.Size = new System.Drawing.Size(957, 286);
            this.intelLog.TabIndex = 0;
            this.intelLog.Text = "";
            this.intelLog.TextChanged += new System.EventHandler(this.intelLog_TextChanged);
            // 
            // ChatServerDown
            // 
            this.ChatServerDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChatServerDown.ForeColor = System.Drawing.Color.Red;
            this.ChatServerDown.Location = new System.Drawing.Point(706, 6);
            this.ChatServerDown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ChatServerDown.Name = "ChatServerDown";
            this.ChatServerDown.Size = new System.Drawing.Size(228, 31);
            this.ChatServerDown.TabIndex = 3;
            this.ChatServerDown.Text = "Chat Server Down";
            this.ChatServerDown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ChatServerDown.Visible = false;
            // 
            // buttonIntelSize
            // 
            this.buttonIntelSize.BackColor = System.Drawing.SystemColors.Info;
            this.buttonIntelSize.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonIntelSize.Location = new System.Drawing.Point(896, 354);
            this.buttonIntelSize.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonIntelSize.Name = "buttonIntelSize";
            this.buttonIntelSize.Size = new System.Drawing.Size(26, 28);
            this.buttonIntelSize.TabIndex = 6;
            this.buttonIntelSize.Text = "▼";
            this.buttonIntelSize.UseVisualStyleBackColor = false;
            this.buttonIntelSize.Click += new System.EventHandler(this.buttonIntelSize_Click);
            // 
            // lblCurrentClient
            // 
            this.lblCurrentClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentClient.ForeColor = System.Drawing.Color.Blue;
            this.lblCurrentClient.Location = new System.Drawing.Point(3, 3);
            this.lblCurrentClient.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentClient.Name = "lblCurrentClient";
            this.lblCurrentClient.Size = new System.Drawing.Size(404, 32);
            this.lblCurrentClient.TabIndex = 7;
            this.lblCurrentClient.Text = "Current Client";
            this.lblCurrentClient.Visible = false;
            // 
            // lblISK
            // 
            this.lblISK.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblISK.Location = new System.Drawing.Point(442, 312);
            this.lblISK.Name = "lblISK";
            this.lblISK.Size = new System.Drawing.Size(171, 17);
            this.lblISK.TabIndex = 8;
            this.lblISK.Text = "999999";
            this.lblISK.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.Location = new System.Drawing.Point(0, 336);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(262, 20);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "XXXXXXXXXXXXXXXXXXXXXXX";
            // 
            // pMinimapImage
            // 
            this.pMinimapImage.BackColor = System.Drawing.Color.Transparent;
            this.pMinimapImage.Location = new System.Drawing.Point(4, 5);
            this.pMinimapImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pMinimapImage.Name = "pMinimapImage";
            this.pMinimapImage.Size = new System.Drawing.Size(429, 323);
            this.pMinimapImage.TabIndex = 10;
            this.pMinimapImage.TabStop = false;
            this.pMinimapImage.Click += new System.EventHandler(this.pMinimapImage_Click);
            this.pMinimapImage.Paint += new System.Windows.Forms.PaintEventHandler(this.pMinimapImage_Paint);
            this.pMinimapImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pMinimapImage_MouseMove);
            // 
            // lblAlertsOutstanding
            // 
            this.lblAlertsOutstanding.ForeColor = System.Drawing.Color.LightCoral;
            this.lblAlertsOutstanding.Location = new System.Drawing.Point(338, 5);
            this.lblAlertsOutstanding.Name = "lblAlertsOutstanding";
            this.lblAlertsOutstanding.Size = new System.Drawing.Size(171, 17);
            this.lblAlertsOutstanding.TabIndex = 12;
            this.lblAlertsOutstanding.Text = "999999";
            this.lblAlertsOutstanding.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblAlertsOutstanding.Visible = false;
            // 
            // lblEtxraInformation
            // 
            this.lblEtxraInformation.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblEtxraInformation.Location = new System.Drawing.Point(442, 66);
            this.lblEtxraInformation.Name = "lblEtxraInformation";
            this.lblEtxraInformation.Size = new System.Drawing.Size(464, 198);
            this.lblEtxraInformation.TabIndex = 13;
            this.lblEtxraInformation.Text = "999999";
            this.lblEtxraInformation.Visible = false;
            this.lblEtxraInformation.Click += new System.EventHandler(this.lblEtxraInformation_Click);
            // 
            // lblSystemName
            // 
            this.lblSystemName.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblSystemName.Location = new System.Drawing.Point(668, 312);
            this.lblSystemName.Name = "lblSystemName";
            this.lblSystemName.Size = new System.Drawing.Size(290, 17);
            this.lblSystemName.TabIndex = 14;
            this.lblSystemName.Text = "systemName";
            this.lblSystemName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblSystemName.Visible = false;
            // 
            // working
            // 
            this.working.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.working.ForeColor = System.Drawing.Color.SaddleBrown;
            this.working.Location = new System.Drawing.Point(938, -2);
            this.working.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.working.Name = "working";
            this.working.Size = new System.Drawing.Size(20, 23);
            this.working.TabIndex = 15;
            this.working.Text = "*";
            this.working.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Quesadilla
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(957, 642);
            this.Controls.Add(this.working);
            this.Controls.Add(this.lblSystemName);
            this.Controls.Add(this.lblEtxraInformation);
            this.Controls.Add(this.buttonIntelSize);
            this.Controls.Add(this.lblAlertsOutstanding);
            this.Controls.Add(this.lblCurrentClient);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblISK);
            this.Controls.Add(this.ChatServerDown);
            this.Controls.Add(this.intelLog);
            this.Controls.Add(this.pMinimapImage);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.Lime;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.MaximizeBox = false;
            this.Name = "Quesadilla";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Quesadilla_FormClosing);
            this.Load += new System.EventHandler(this.Quesadilla_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Quesadilla_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Quesadilla_MouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.pMinimapImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer mainTimer;
        private Serilog.Sinks.WinForms.RichTextBoxLogControl intelLog;
        private System.Windows.Forms.Label ChatServerDown;
        private System.Windows.Forms.Button buttonIntelSize;
        private System.Windows.Forms.Label lblCurrentClient;
        private System.Windows.Forms.Label lblISK;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox pMinimapImage;
        private System.Windows.Forms.Label lblAlertsOutstanding;
        private System.Windows.Forms.Label lblEtxraInformation;
        private System.Windows.Forms.Label lblSystemName;
        private System.Windows.Forms.Label working;
    }
}

