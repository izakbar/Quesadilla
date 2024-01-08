namespace Quesadilla
{
    partial class QuesadillaAlertPopup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuesadillaAlertPopup));
            this.timerAlertWindow = new System.Windows.Forms.Timer(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timerOpacityOpen = new System.Windows.Forms.Timer(this.components);
            this.AlertText = new System.Windows.Forms.Label();
            this.SystemName = new System.Windows.Forms.Label();
            this.ContextInfo = new System.Windows.Forms.Label();
            this.IntelLine = new System.Windows.Forms.Label();
            this.timerOpacityClose = new System.Windows.Forms.Timer(this.components);
            this.RegionName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // timerAlertWindow
            // 
            this.timerAlertWindow.Interval = 69420;
            this.timerAlertWindow.Tick += new System.EventHandler(this.timerAlertWindow_Tick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(5, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(105, 98);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // timerOpacityOpen
            // 
            this.timerOpacityOpen.Interval = 75;
            this.timerOpacityOpen.Tick += new System.EventHandler(this.timerOpacityOpen_Tick);
            // 
            // AlertText
            // 
            this.AlertText.AutoSize = true;
            this.AlertText.BackColor = System.Drawing.Color.Transparent;
            this.AlertText.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.AlertText.ForeColor = System.Drawing.Color.MistyRose;
            this.AlertText.Location = new System.Drawing.Point(0, 136);
            this.AlertText.Name = "AlertText";
            this.AlertText.Size = new System.Drawing.Size(301, 13);
            this.AlertText.TabIndex = 1;
            this.AlertText.Text = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            this.AlertText.Click += new System.EventHandler(this.label1_Click);
            // 
            // SystemName
            // 
            this.SystemName.BackColor = System.Drawing.Color.Transparent;
            this.SystemName.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SystemName.ForeColor = System.Drawing.Color.Red;
            this.SystemName.Location = new System.Drawing.Point(218, 9);
            this.SystemName.Name = "SystemName";
            this.SystemName.Size = new System.Drawing.Size(147, 31);
            this.SystemName.TabIndex = 2;
            this.SystemName.Text = "XXXXXXX";
            this.SystemName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.SystemName.Click += new System.EventHandler(this.SystemName_Click);
            // 
            // ContextInfo
            // 
            this.ContextInfo.BackColor = System.Drawing.Color.Transparent;
            this.ContextInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ContextInfo.ForeColor = System.Drawing.Color.Red;
            this.ContextInfo.Location = new System.Drawing.Point(169, 47);
            this.ContextInfo.Name = "ContextInfo";
            this.ContextInfo.Size = new System.Drawing.Size(192, 19);
            this.ContextInfo.TabIndex = 3;
            this.ContextInfo.Text = "XXXXXXX";
            this.ContextInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // IntelLine
            // 
            this.IntelLine.BackColor = System.Drawing.Color.Transparent;
            this.IntelLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IntelLine.ForeColor = System.Drawing.Color.SaddleBrown;
            this.IntelLine.Location = new System.Drawing.Point(117, 64);
            this.IntelLine.Name = "IntelLine";
            this.IntelLine.Size = new System.Drawing.Size(243, 19);
            this.IntelLine.TabIndex = 4;
            this.IntelLine.Text = "XXXXXXX";
            this.IntelLine.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // timerOpacityClose
            // 
            this.timerOpacityClose.Interval = 75;
            this.timerOpacityClose.Tick += new System.EventHandler(this.timerOpacityClose_Tick);
            // 
            // RegionName
            // 
            this.RegionName.BackColor = System.Drawing.Color.Transparent;
            this.RegionName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RegionName.ForeColor = System.Drawing.Color.Lime;
            this.RegionName.Location = new System.Drawing.Point(239, 37);
            this.RegionName.Name = "RegionName";
            this.RegionName.Size = new System.Drawing.Size(117, 13);
            this.RegionName.TabIndex = 5;
            this.RegionName.Text = "XXXXXXX";
            this.RegionName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RegionName.UseCompatibleTextRendering = true;
            // 
            // QuesadillaAlertPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Quesadilla.Properties.Resources.background1;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(369, 149);
            this.ControlBox = false;
            this.Controls.Add(this.RegionName);
            this.Controls.Add(this.IntelLine);
            this.Controls.Add(this.ContextInfo);
            this.Controls.Add(this.SystemName);
            this.Controls.Add(this.AlertText);
            this.Controls.Add(this.pictureBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QuesadillaAlertPopup";
            this.Opacity = 0D;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.QuesadillaAlertPopup_Load);
            this.Click += new System.EventHandler(this.QuesadillaAlertPopup_Click);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timerAlertWindow;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timerOpacityOpen;
        private System.Windows.Forms.Label AlertText;
        private System.Windows.Forms.Label SystemName;
        private System.Windows.Forms.Label ContextInfo;
        private System.Windows.Forms.Label IntelLine;
        private System.Windows.Forms.Timer timerOpacityClose;
        private System.Windows.Forms.Label RegionName;
    }
}