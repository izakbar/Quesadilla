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
			this.pAlertImage = new System.Windows.Forms.PictureBox();
			this.timerOpacityOpen = new System.Windows.Forms.Timer(this.components);
			this.AlertText = new System.Windows.Forms.Label();
			this.SystemName = new System.Windows.Forms.Label();
			this.ContextInfo = new System.Windows.Forms.Label();
			this.IntelLine = new System.Windows.Forms.Label();
			this.timerOpacityClose = new System.Windows.Forms.Timer(this.components);
			this.RegionName = new System.Windows.Forms.Label();
			this.ShipName = new System.Windows.Forms.Label();
			this.pMinimapImage = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pAlertImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pMinimapImage)).BeginInit();
			this.SuspendLayout();
			// 
			// timerAlertWindow
			// 
			this.timerAlertWindow.Interval = 69420;
			this.timerAlertWindow.Tick += new System.EventHandler(this.timerAlertWindow_Tick);
			// 
			// pAlertImage
			// 
			this.pAlertImage.BackColor = System.Drawing.Color.Transparent;
			this.pAlertImage.Image = ((System.Drawing.Image)(resources.GetObject("pAlertImage.Image")));
			this.pAlertImage.Location = new System.Drawing.Point(470, 138);
			this.pAlertImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pAlertImage.Name = "pAlertImage";
			this.pAlertImage.Size = new System.Drawing.Size(80, 88);
			this.pAlertImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pAlertImage.TabIndex = 0;
			this.pAlertImage.TabStop = false;
			this.pAlertImage.Click += new System.EventHandler(this.pAlertImage_Click);
			this.pAlertImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseDown);
			this.pAlertImage.MouseEnter += new System.EventHandler(this.pAlertImage_MouseEnter);
			this.pAlertImage.MouseLeave += new System.EventHandler(this.pAlertImage_MouseLeave);
			this.pAlertImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseMove);
			this.pAlertImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseMove);
			// 
			// timerOpacityOpen
			// 
			this.timerOpacityOpen.Interval = 75;
			this.timerOpacityOpen.Tick += new System.EventHandler(this.timerOpacityOpen_Tick);
			// 
			// AlertText
			// 
			this.AlertText.BackColor = System.Drawing.Color.Transparent;
			this.AlertText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AlertText.ForeColor = System.Drawing.Color.MistyRose;
			this.AlertText.Location = new System.Drawing.Point(0, 205);
			this.AlertText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.AlertText.Name = "AlertText";
			this.AlertText.Size = new System.Drawing.Size(501, 18);
			this.AlertText.TabIndex = 1;
			this.AlertText.Text = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
			this.AlertText.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.AlertText.Visible = false;
			this.AlertText.Click += new System.EventHandler(this.label1_Click);
			this.AlertText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseDown);
			this.AlertText.MouseMove += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseMove);
			this.AlertText.MouseUp += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseUp);
			// 
			// SystemName
			// 
			this.SystemName.BackColor = System.Drawing.Color.Transparent;
			this.SystemName.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SystemName.ForeColor = System.Drawing.Color.Red;
			this.SystemName.Location = new System.Drawing.Point(128, 6);
			this.SystemName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.SystemName.Name = "SystemName";
			this.SystemName.Size = new System.Drawing.Size(422, 48);
			this.SystemName.TabIndex = 2;
			this.SystemName.Text = "XXXXXXX";
			this.SystemName.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.SystemName.Visible = false;
			this.SystemName.Click += new System.EventHandler(this.SystemName_Click);
			this.SystemName.MouseDown += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseDown);
			this.SystemName.MouseMove += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseMove);
			this.SystemName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseUp);
			// 
			// ContextInfo
			// 
			this.ContextInfo.BackColor = System.Drawing.Color.Transparent;
			this.ContextInfo.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ContextInfo.ForeColor = System.Drawing.Color.Red;
			this.ContextInfo.Location = new System.Drawing.Point(3, 72);
			this.ContextInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.ContextInfo.Name = "ContextInfo";
			this.ContextInfo.Size = new System.Drawing.Size(538, 29);
			this.ContextInfo.TabIndex = 3;
			this.ContextInfo.Text = "XXXXXXX";
			this.ContextInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ContextInfo.Visible = false;
			this.ContextInfo.Click += new System.EventHandler(this.ContextInfo_Click);
			this.ContextInfo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseDown);
			this.ContextInfo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseMove);
			this.ContextInfo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseUp);
			// 
			// IntelLine
			// 
			this.IntelLine.BackColor = System.Drawing.Color.Transparent;
			this.IntelLine.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IntelLine.ForeColor = System.Drawing.Color.SaddleBrown;
			this.IntelLine.Location = new System.Drawing.Point(186, 98);
			this.IntelLine.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.IntelLine.Name = "IntelLine";
			this.IntelLine.Size = new System.Drawing.Size(354, 88);
			this.IntelLine.TabIndex = 4;
			this.IntelLine.Text = "XXXXXXX";
			this.IntelLine.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.IntelLine.Visible = false;
			this.IntelLine.Click += new System.EventHandler(this.IntelLine_Click);
			this.IntelLine.MouseDown += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseDown);
			this.IntelLine.MouseEnter += new System.EventHandler(this.Zoom_MouseEnter);
			this.IntelLine.MouseLeave += new System.EventHandler(this.Zoom_MouseLeave);
			this.IntelLine.MouseMove += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseMove);
			this.IntelLine.MouseUp += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseUp);
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
			this.RegionName.Location = new System.Drawing.Point(358, 55);
			this.RegionName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.RegionName.Name = "RegionName";
			this.RegionName.Size = new System.Drawing.Size(176, 20);
			this.RegionName.TabIndex = 5;
			this.RegionName.Text = "XXXXXXX";
			this.RegionName.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.RegionName.UseCompatibleTextRendering = true;
			this.RegionName.Visible = false;
			this.RegionName.MouseDown += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseDown);
			this.RegionName.MouseMove += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseMove);
			this.RegionName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseUp);
			// 
			// ShipName
			// 
			this.ShipName.BackColor = System.Drawing.Color.Transparent;
			this.ShipName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ShipName.ForeColor = System.Drawing.Color.Lime;
			this.ShipName.Location = new System.Drawing.Point(252, 185);
			this.ShipName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.ShipName.Name = "ShipName";
			this.ShipName.Size = new System.Drawing.Size(219, 20);
			this.ShipName.TabIndex = 6;
			this.ShipName.Text = "XXXXXXX";
			this.ShipName.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.ShipName.UseCompatibleTextRendering = true;
			this.ShipName.Visible = false;
			this.ShipName.Click += new System.EventHandler(this.ShipName_Click);
			// 
			// pMinimapImage
			// 
			this.pMinimapImage.BackColor = System.Drawing.Color.Transparent;
			this.pMinimapImage.Location = new System.Drawing.Point(48, 2);
			this.pMinimapImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pMinimapImage.Name = "pMinimapImage";
			this.pMinimapImage.Size = new System.Drawing.Size(232, 205);
			this.pMinimapImage.TabIndex = 7;
			this.pMinimapImage.TabStop = false;
			this.pMinimapImage.Click += new System.EventHandler(this.pMinimap_Click);
			this.pMinimapImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pMinimapImage_MouseMove);
			// 
			// QuesadillaAlertPopup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = global::Quesadilla.Properties.Resources.bg_paulmart2;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.ClientSize = new System.Drawing.Size(550, 231);
			this.ControlBox = false;
			this.Controls.Add(this.pMinimapImage);
			this.Controls.Add(this.ShipName);
			this.Controls.Add(this.RegionName);
			this.Controls.Add(this.pAlertImage);
			this.Controls.Add(this.IntelLine);
			this.Controls.Add(this.ContextInfo);
			this.Controls.Add(this.SystemName);
			this.Controls.Add(this.AlertText);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "QuesadillaAlertPopup";
			this.Opacity = 0D;
			this.Text = "Alert!";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.QuesadillaAlertPopup_Load);
			this.Shown += new System.EventHandler(this.QuesadillaAlertPopup_Shown);
			this.Click += new System.EventHandler(this.QuesadillaAlertPopup_Click);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.QuesadillaAlertPopup_Paint);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.QuesadillaAlertPopup_MouseUp);
			((System.ComponentModel.ISupportInitialize)(this.pAlertImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pMinimapImage)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timerAlertWindow;
        private System.Windows.Forms.PictureBox pAlertImage;
        private System.Windows.Forms.Timer timerOpacityOpen;
        private System.Windows.Forms.Label AlertText;
        private System.Windows.Forms.Label SystemName;
        private System.Windows.Forms.Label ContextInfo;
        private System.Windows.Forms.Label IntelLine;
        private System.Windows.Forms.Timer timerOpacityClose;
        private System.Windows.Forms.Label RegionName;
        private System.Windows.Forms.Label ShipName;
        private System.Windows.Forms.PictureBox pMinimapImage;
    }
}