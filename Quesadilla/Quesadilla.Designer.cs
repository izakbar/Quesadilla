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
			this.IntelLog = new Serilog.Sinks.WinForms.RichTextBoxLogControl();
			this.ChatServerDown = new System.Windows.Forms.Label();
			this.buttonIntelSize = new System.Windows.Forms.Button();
			this.CurrentClient = new System.Windows.Forms.Label();
			this.RattingData = new System.Windows.Forms.Label();
			this.EveScoutStatus = new System.Windows.Forms.Label();
			this.MainMinimap = new System.Windows.Forms.PictureBox();
			this.AlertsOutstanding = new System.Windows.Forms.Label();
			this.ExtraInformation = new System.Windows.Forms.Label();
			this.MouseoverSystemName = new System.Windows.Forms.Label();
			this.WorkingIndicator = new System.Windows.Forms.Label();
			this.MiningData = new System.Windows.Forms.Label();
			this.TitleIcon = new System.Windows.Forms.PictureBox();
			this.TopbarPanel = new System.Windows.Forms.Panel();
			this.ButtonMinimize = new System.Windows.Forms.Button();
			this.buttonClose = new System.Windows.Forms.Button();
			this.Title = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.MainMinimap)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TitleIcon)).BeginInit();
			this.TopbarPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainTimer
			// 
			this.mainTimer.Enabled = true;
			this.mainTimer.Interval = 250;
			this.mainTimer.Tick += new System.EventHandler(this.mainTimer_Tick);
			// 
			// IntelLog
			// 
			this.IntelLog.BackColor = System.Drawing.SystemColors.Info;
			this.IntelLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.IntelLog.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.IntelLog.ForContext = "";
			this.IntelLog.Location = new System.Drawing.Point(0, 231);
			this.IntelLog.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.IntelLog.Name = "IntelLog";
			this.IntelLog.ReadOnly = true;
			this.IntelLog.Size = new System.Drawing.Size(638, 186);
			this.IntelLog.TabIndex = 0;
			this.IntelLog.Text = "";
			this.IntelLog.TextChanged += new System.EventHandler(this.intelLog_TextChanged);
			// 
			// ChatServerDown
			// 
			this.ChatServerDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ChatServerDown.ForeColor = System.Drawing.Color.Red;
			this.ChatServerDown.Location = new System.Drawing.Point(474, 17);
			this.ChatServerDown.Name = "ChatServerDown";
			this.ChatServerDown.Size = new System.Drawing.Size(152, 20);
			this.ChatServerDown.TabIndex = 3;
			this.ChatServerDown.Text = "Chat Server Down";
			this.ChatServerDown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ChatServerDown.Visible = false;
			// 
			// buttonIntelSize
			// 
			this.buttonIntelSize.BackColor = System.Drawing.SystemColors.Info;
			this.buttonIntelSize.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonIntelSize.Location = new System.Drawing.Point(597, 230);
			this.buttonIntelSize.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.buttonIntelSize.Name = "buttonIntelSize";
			this.buttonIntelSize.Size = new System.Drawing.Size(17, 18);
			this.buttonIntelSize.TabIndex = 6;
			this.buttonIntelSize.Text = "▼";
			this.buttonIntelSize.UseVisualStyleBackColor = false;
			this.buttonIntelSize.Click += new System.EventHandler(this.buttonIntelSize_Click);
			// 
			// CurrentClient
			// 
			this.CurrentClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CurrentClient.ForeColor = System.Drawing.Color.Blue;
			this.CurrentClient.Location = new System.Drawing.Point(21, 43);
			this.CurrentClient.Name = "CurrentClient";
			this.CurrentClient.Size = new System.Drawing.Size(269, 21);
			this.CurrentClient.TabIndex = 7;
			this.CurrentClient.Text = "Current Client";
			this.CurrentClient.Visible = false;
			// 
			// RattingData
			// 
			this.RattingData.BackColor = System.Drawing.Color.Transparent;
			this.RattingData.ForeColor = System.Drawing.Color.ForestGreen;
			this.RattingData.Location = new System.Drawing.Point(295, 203);
			this.RattingData.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.RattingData.Name = "RattingData";
			this.RattingData.Size = new System.Drawing.Size(118, 11);
			this.RattingData.TabIndex = 8;
			this.RattingData.Text = "999999";
			this.RattingData.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.RattingData.Visible = false;
			// 
			// EveScoutStatus
			// 
			this.EveScoutStatus.AutoSize = true;
			this.EveScoutStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.EveScoutStatus.Location = new System.Drawing.Point(0, 218);
			this.EveScoutStatus.Name = "EveScoutStatus";
			this.EveScoutStatus.Size = new System.Drawing.Size(168, 13);
			this.EveScoutStatus.TabIndex = 9;
			this.EveScoutStatus.Text = "XXXXXXXXXXXXXXXXXXXXXXX";
			// 
			// MainMinimap
			// 
			this.MainMinimap.BackColor = System.Drawing.Color.Transparent;
			this.MainMinimap.Location = new System.Drawing.Point(4, 20);
			this.MainMinimap.Name = "MainMinimap";
			this.MainMinimap.Size = new System.Drawing.Size(286, 156);
			this.MainMinimap.TabIndex = 10;
			this.MainMinimap.TabStop = false;
			this.MainMinimap.Click += new System.EventHandler(this.pMinimapImage_Click);
			this.MainMinimap.Paint += new System.Windows.Forms.PaintEventHandler(this.pMinimapImage_Paint);
			this.MainMinimap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pMinimapImage_MouseMove);
			// 
			// AlertsOutstanding
			// 
			this.AlertsOutstanding.ForeColor = System.Drawing.Color.LightCoral;
			this.AlertsOutstanding.Location = new System.Drawing.Point(295, 20);
			this.AlertsOutstanding.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.AlertsOutstanding.Name = "AlertsOutstanding";
			this.AlertsOutstanding.Size = new System.Drawing.Size(114, 11);
			this.AlertsOutstanding.TabIndex = 12;
			this.AlertsOutstanding.Text = "999999";
			this.AlertsOutstanding.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.AlertsOutstanding.Visible = false;
			// 
			// ExtraInformation
			// 
			this.ExtraInformation.BackColor = System.Drawing.Color.Transparent;
			this.ExtraInformation.ForeColor = System.Drawing.Color.Fuchsia;
			this.ExtraInformation.Location = new System.Drawing.Point(295, 43);
			this.ExtraInformation.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.ExtraInformation.Name = "ExtraInformation";
			this.ExtraInformation.Size = new System.Drawing.Size(309, 129);
			this.ExtraInformation.TabIndex = 13;
			this.ExtraInformation.Text = "999999";
			this.ExtraInformation.Visible = false;
			this.ExtraInformation.Click += new System.EventHandler(this.lblEtxraInformation_Click);
			// 
			// MouseoverSystemName
			// 
			this.MouseoverSystemName.ForeColor = System.Drawing.Color.ForestGreen;
			this.MouseoverSystemName.Location = new System.Drawing.Point(445, 203);
			this.MouseoverSystemName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.MouseoverSystemName.Name = "MouseoverSystemName";
			this.MouseoverSystemName.Size = new System.Drawing.Size(193, 11);
			this.MouseoverSystemName.TabIndex = 14;
			this.MouseoverSystemName.Text = "systemName";
			this.MouseoverSystemName.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.MouseoverSystemName.Visible = false;
			// 
			// WorkingIndicator
			// 
			this.WorkingIndicator.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WorkingIndicator.ForeColor = System.Drawing.Color.SaddleBrown;
			this.WorkingIndicator.Location = new System.Drawing.Point(625, 12);
			this.WorkingIndicator.Name = "WorkingIndicator";
			this.WorkingIndicator.Size = new System.Drawing.Size(13, 15);
			this.WorkingIndicator.TabIndex = 15;
			this.WorkingIndicator.Text = "*";
			this.WorkingIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MiningData
			// 
			this.MiningData.BackColor = System.Drawing.Color.Transparent;
			this.MiningData.ForeColor = System.Drawing.Color.ForestGreen;
			this.MiningData.Location = new System.Drawing.Point(295, 192);
			this.MiningData.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.MiningData.Name = "MiningData";
			this.MiningData.Size = new System.Drawing.Size(118, 11);
			this.MiningData.TabIndex = 16;
			this.MiningData.Text = "999999";
			this.MiningData.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.MiningData.Visible = false;
			// 
			// TitleIcon
			// 
			this.TitleIcon.Dock = System.Windows.Forms.DockStyle.Left;
			this.TitleIcon.Location = new System.Drawing.Point(0, 0);
			this.TitleIcon.Margin = new System.Windows.Forms.Padding(4);
			this.TitleIcon.Name = "TitleIcon";
			this.TitleIcon.Size = new System.Drawing.Size(20, 18);
			this.TitleIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.TitleIcon.TabIndex = 2;
			this.TitleIcon.TabStop = false;
			// 
			// TopbarPanel
			// 
			this.TopbarPanel.BackColor = System.Drawing.SystemColors.MenuBar;
			this.TopbarPanel.Controls.Add(this.ButtonMinimize);
			this.TopbarPanel.Controls.Add(this.buttonClose);
			this.TopbarPanel.Controls.Add(this.Title);
			this.TopbarPanel.Controls.Add(this.TitleIcon);
			this.TopbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopbarPanel.Location = new System.Drawing.Point(0, 0);
			this.TopbarPanel.Name = "TopbarPanel";
			this.TopbarPanel.Size = new System.Drawing.Size(638, 18);
			this.TopbarPanel.TabIndex = 17;
			this.TopbarPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopbarPanel_MouseDown);
			// 
			// ButtonMinimize
			// 
			this.ButtonMinimize.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonMinimize.FlatAppearance.BorderSize = 0;
			this.ButtonMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonMinimize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ButtonMinimize.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ButtonMinimize.Location = new System.Drawing.Point(588, 0);
			this.ButtonMinimize.Name = "ButtonMinimize";
			this.ButtonMinimize.Size = new System.Drawing.Size(25, 18);
			this.ButtonMinimize.TabIndex = 20;
			this.ButtonMinimize.Text = "-";
			this.ButtonMinimize.UseVisualStyleBackColor = true;
			this.ButtonMinimize.Click += new System.EventHandler(this.ButtonMinimize_Click);
			// 
			// buttonClose
			// 
			this.buttonClose.Dock = System.Windows.Forms.DockStyle.Right;
			this.buttonClose.FlatAppearance.BorderSize = 0;
			this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonClose.ForeColor = System.Drawing.SystemColors.ControlText;
			this.buttonClose.Location = new System.Drawing.Point(613, 0);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(25, 18);
			this.buttonClose.TabIndex = 19;
			this.buttonClose.Text = "X";
			this.buttonClose.UseVisualStyleBackColor = true;
			this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
			// 
			// Title
			// 
			this.Title.AutoSize = true;
			this.Title.Dock = System.Windows.Forms.DockStyle.Left;
			this.Title.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.Title.Location = new System.Drawing.Point(20, 0);
			this.Title.Margin = new System.Windows.Forms.Padding(1);
			this.Title.Name = "Title";
			this.Title.Padding = new System.Windows.Forms.Padding(3);
			this.Title.Size = new System.Drawing.Size(41, 19);
			this.Title.TabIndex = 4;
			this.Title.Text = "label1";
			this.Title.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// Quesadilla
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(638, 417);
			this.Controls.Add(this.TopbarPanel);
			this.Controls.Add(this.MiningData);
			this.Controls.Add(this.WorkingIndicator);
			this.Controls.Add(this.MouseoverSystemName);
			this.Controls.Add(this.ExtraInformation);
			this.Controls.Add(this.buttonIntelSize);
			this.Controls.Add(this.AlertsOutstanding);
			this.Controls.Add(this.CurrentClient);
			this.Controls.Add(this.EveScoutStatus);
			this.Controls.Add(this.RattingData);
			this.Controls.Add(this.ChatServerDown);
			this.Controls.Add(this.IntelLog);
			this.Controls.Add(this.MainMinimap);
			this.DoubleBuffered = true;
			this.ForeColor = System.Drawing.Color.Lime;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.MaximizeBox = false;
			this.Name = "Quesadilla";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Quesadilla_FormClosing);
			this.Load += new System.EventHandler(this.Quesadilla_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Quesadilla_Paint);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Quesadilla_MouseMove);
			((System.ComponentModel.ISupportInitialize)(this.MainMinimap)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TitleIcon)).EndInit();
			this.TopbarPanel.ResumeLayout(false);
			this.TopbarPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer mainTimer;
        private Serilog.Sinks.WinForms.RichTextBoxLogControl IntelLog;
        private System.Windows.Forms.Label ChatServerDown;
        private System.Windows.Forms.Button buttonIntelSize;
        private System.Windows.Forms.Label RattingData;
        private System.Windows.Forms.Label EveScoutStatus;
        private System.Windows.Forms.PictureBox MainMinimap;
        private System.Windows.Forms.Label AlertsOutstanding;
        private System.Windows.Forms.Label ExtraInformation;
        private System.Windows.Forms.Label MouseoverSystemName;
        private System.Windows.Forms.Label WorkingIndicator;
        private System.Windows.Forms.Label MiningData;
		private System.Windows.Forms.Label CurrentClient;
		private System.Windows.Forms.PictureBox TitleIcon;
		private System.Windows.Forms.Panel TopbarPanel;
		private System.Windows.Forms.Label Title;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.Button ButtonMinimize;
	}
}

