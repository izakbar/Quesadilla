namespace Quesadilla
{
    partial class QuesadillaConfiguration
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
            this.cmbHomeSystem = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.alertDemo = new System.Windows.Forms.PictureBox();
            this.colourPicker = new System.Windows.Forms.ColorDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbBackground = new System.Windows.Forms.ComboBox();
            this.trackOpacity = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.trackPopupTime = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.trackDPSGraphLineSize = new System.Windows.Forms.TrackBar();
            this.lblOpacity = new System.Windows.Forms.Label();
            this.lblPopupSeconds = new System.Windows.Forms.Label();
            this.lblDPSGraphLineSize = new System.Windows.Forms.Label();
            this.lblShowLastUnexpiredAlerts = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.trackShowLastUnexpiredAlerts = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.trackAlertPopupMinimapSizeWidth = new System.Windows.Forms.TrackBar();
            this.label8 = new System.Windows.Forms.Label();
            this.trackAlertPopupMinimapSizeHeight = new System.Windows.Forms.TrackBar();
            this.label9 = new System.Windows.Forms.Label();
            this.trackAlertPopupMinimapPositionY = new System.Windows.Forms.TrackBar();
            this.label10 = new System.Windows.Forms.Label();
            this.trackAlertPopupMinimapPositionX = new System.Windows.Forms.TrackBar();
            this.ConfigurationProperties = new System.Windows.Forms.PropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alertDemo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackPopupTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackDPSGraphLineSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackShowLastUnexpiredAlerts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAlertPopupMinimapSizeWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAlertPopupMinimapSizeHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAlertPopupMinimapPositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAlertPopupMinimapPositionX)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbHomeSystem
            // 
            this.cmbHomeSystem.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbHomeSystem.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbHomeSystem.FormattingEnabled = true;
            this.cmbHomeSystem.Location = new System.Drawing.Point(874, 370);
            this.cmbHomeSystem.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cmbHomeSystem.Name = "cmbHomeSystem";
            this.cmbHomeSystem.Size = new System.Drawing.Size(99, 21);
            this.cmbHomeSystem.TabIndex = 0;
            this.cmbHomeSystem.SelectedIndexChanged += new System.EventHandler(this.cmbHomeSystem_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(797, 372);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Home System";
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(890, 567);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(83, 36);
            this.buttonSave.TabIndex = 3;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(801, 567);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(83, 36);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Quesadilla.Properties.Resources.Quesadilla;
            this.pictureBox1.Location = new System.Drawing.Point(801, 395);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(172, 168);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // alertDemo
            // 
            this.alertDemo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.alertDemo.Location = new System.Drawing.Point(8, 10);
            this.alertDemo.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.alertDemo.Name = "alertDemo";
            this.alertDemo.Size = new System.Drawing.Size(369, 152);
            this.alertDemo.TabIndex = 6;
            this.alertDemo.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(394, 146);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Background";
            // 
            // cmbBackground
            // 
            this.cmbBackground.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbBackground.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbBackground.FormattingEnabled = true;
            this.cmbBackground.Location = new System.Drawing.Point(504, 143);
            this.cmbBackground.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cmbBackground.Name = "cmbBackground";
            this.cmbBackground.Size = new System.Drawing.Size(149, 21);
            this.cmbBackground.TabIndex = 13;
            this.cmbBackground.SelectedIndexChanged += new System.EventHandler(this.cmbBackground_SelectedIndexChanged);
            // 
            // trackOpacity
            // 
            this.trackOpacity.LargeChange = 10;
            this.trackOpacity.Location = new System.Drawing.Point(473, 10);
            this.trackOpacity.Maximum = 100;
            this.trackOpacity.Name = "trackOpacity";
            this.trackOpacity.Size = new System.Drawing.Size(150, 45);
            this.trackOpacity.TabIndex = 15;
            this.trackOpacity.TickFrequency = 10;
            this.trackOpacity.Scroll += new System.EventHandler(this.trackOpacity_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(425, 12);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Opacity";
            // 
            // trackPopupTime
            // 
            this.trackPopupTime.AccessibleDescription = "";
            this.trackPopupTime.LargeChange = 10;
            this.trackPopupTime.Location = new System.Drawing.Point(473, 40);
            this.trackPopupTime.Maximum = 60;
            this.trackPopupTime.Minimum = 1;
            this.trackPopupTime.Name = "trackPopupTime";
            this.trackPopupTime.Size = new System.Drawing.Size(150, 45);
            this.trackPopupTime.TabIndex = 17;
            this.trackPopupTime.TickFrequency = 10;
            this.trackPopupTime.Value = 1;
            this.trackPopupTime.Scroll += new System.EventHandler(this.trackPopupTime_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(408, 38);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Popup time";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(389, 68);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 39;
            this.label5.Text = "DPS Line Width";
            // 
            // trackDPSGraphLineSize
            // 
            this.trackDPSGraphLineSize.AccessibleDescription = "";
            this.trackDPSGraphLineSize.LargeChange = 10;
            this.trackDPSGraphLineSize.Location = new System.Drawing.Point(473, 68);
            this.trackDPSGraphLineSize.Maximum = 12;
            this.trackDPSGraphLineSize.Minimum = 1;
            this.trackDPSGraphLineSize.Name = "trackDPSGraphLineSize";
            this.trackDPSGraphLineSize.Size = new System.Drawing.Size(150, 45);
            this.trackDPSGraphLineSize.TabIndex = 38;
            this.trackDPSGraphLineSize.TickFrequency = 10;
            this.trackDPSGraphLineSize.Value = 1;
            this.trackDPSGraphLineSize.Scroll += new System.EventHandler(this.trackDPSGraphLineSize_Scroll);
            // 
            // lblOpacity
            // 
            this.lblOpacity.AutoSize = true;
            this.lblOpacity.Location = new System.Drawing.Point(628, 14);
            this.lblOpacity.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOpacity.Name = "lblOpacity";
            this.lblOpacity.Size = new System.Drawing.Size(25, 13);
            this.lblOpacity.TabIndex = 40;
            this.lblOpacity.Text = "999";
            // 
            // lblPopupSeconds
            // 
            this.lblPopupSeconds.AutoSize = true;
            this.lblPopupSeconds.Location = new System.Drawing.Point(628, 38);
            this.lblPopupSeconds.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPopupSeconds.Name = "lblPopupSeconds";
            this.lblPopupSeconds.Size = new System.Drawing.Size(25, 13);
            this.lblPopupSeconds.TabIndex = 41;
            this.lblPopupSeconds.Text = "999";
            // 
            // lblDPSGraphLineSize
            // 
            this.lblDPSGraphLineSize.AutoSize = true;
            this.lblDPSGraphLineSize.Location = new System.Drawing.Point(628, 68);
            this.lblDPSGraphLineSize.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDPSGraphLineSize.Name = "lblDPSGraphLineSize";
            this.lblDPSGraphLineSize.Size = new System.Drawing.Size(25, 13);
            this.lblDPSGraphLineSize.TabIndex = 42;
            this.lblDPSGraphLineSize.Text = "999";
            // 
            // lblShowLastUnexpiredAlerts
            // 
            this.lblShowLastUnexpiredAlerts.AutoSize = true;
            this.lblShowLastUnexpiredAlerts.Location = new System.Drawing.Point(628, 101);
            this.lblShowLastUnexpiredAlerts.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblShowLastUnexpiredAlerts.Name = "lblShowLastUnexpiredAlerts";
            this.lblShowLastUnexpiredAlerts.Size = new System.Drawing.Size(25, 13);
            this.lblShowLastUnexpiredAlerts.TabIndex = 45;
            this.lblShowLastUnexpiredAlerts.Text = "999";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(405, 99);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 44;
            this.label7.Text = "Alert History";
            // 
            // trackShowLastUnexpiredAlerts
            // 
            this.trackShowLastUnexpiredAlerts.LargeChange = 10;
            this.trackShowLastUnexpiredAlerts.Location = new System.Drawing.Point(473, 96);
            this.trackShowLastUnexpiredAlerts.Maximum = 25;
            this.trackShowLastUnexpiredAlerts.Minimum = 1;
            this.trackShowLastUnexpiredAlerts.Name = "trackShowLastUnexpiredAlerts";
            this.trackShowLastUnexpiredAlerts.Size = new System.Drawing.Size(150, 45);
            this.trackShowLastUnexpiredAlerts.TabIndex = 43;
            this.trackShowLastUnexpiredAlerts.TickFrequency = 10;
            this.trackShowLastUnexpiredAlerts.Value = 1;
            this.trackShowLastUnexpiredAlerts.Scroll += new System.EventHandler(this.trackShowLastUnexpiredAlerts_Scroll);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(685, 18);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 58;
            this.label6.Text = "Minimap width";
            // 
            // trackAlertPopupMinimapSizeWidth
            // 
            this.trackAlertPopupMinimapSizeWidth.LargeChange = 10;
            this.trackAlertPopupMinimapSizeWidth.Location = new System.Drawing.Point(765, 14);
            this.trackAlertPopupMinimapSizeWidth.Maximum = 9999;
            this.trackAlertPopupMinimapSizeWidth.Minimum = 1;
            this.trackAlertPopupMinimapSizeWidth.Name = "trackAlertPopupMinimapSizeWidth";
            this.trackAlertPopupMinimapSizeWidth.Size = new System.Drawing.Size(150, 45);
            this.trackAlertPopupMinimapSizeWidth.TabIndex = 57;
            this.trackAlertPopupMinimapSizeWidth.TickFrequency = 10;
            this.trackAlertPopupMinimapSizeWidth.Value = 1;
            this.trackAlertPopupMinimapSizeWidth.Scroll += new System.EventHandler(this.trackAlertPopupMinimapSizeWidth_Scroll);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(681, 42);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 13);
            this.label8.TabIndex = 60;
            this.label8.Text = "Minimap height";
            // 
            // trackAlertPopupMinimapSizeHeight
            // 
            this.trackAlertPopupMinimapSizeHeight.LargeChange = 10;
            this.trackAlertPopupMinimapSizeHeight.Location = new System.Drawing.Point(765, 38);
            this.trackAlertPopupMinimapSizeHeight.Maximum = 9999;
            this.trackAlertPopupMinimapSizeHeight.Minimum = 1;
            this.trackAlertPopupMinimapSizeHeight.Name = "trackAlertPopupMinimapSizeHeight";
            this.trackAlertPopupMinimapSizeHeight.Size = new System.Drawing.Size(150, 45);
            this.trackAlertPopupMinimapSizeHeight.TabIndex = 59;
            this.trackAlertPopupMinimapSizeHeight.TickFrequency = 10;
            this.trackAlertPopupMinimapSizeHeight.Value = 1;
            this.trackAlertPopupMinimapSizeHeight.Scroll += new System.EventHandler(this.trackAlertPopupMinimapSizeHeight_Scroll);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(703, 90);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 64;
            this.label9.Text = "Minimap Y";
            // 
            // trackAlertPopupMinimapPositionY
            // 
            this.trackAlertPopupMinimapPositionY.LargeChange = 10;
            this.trackAlertPopupMinimapPositionY.Location = new System.Drawing.Point(765, 90);
            this.trackAlertPopupMinimapPositionY.Maximum = 9999;
            this.trackAlertPopupMinimapPositionY.Name = "trackAlertPopupMinimapPositionY";
            this.trackAlertPopupMinimapPositionY.Size = new System.Drawing.Size(150, 45);
            this.trackAlertPopupMinimapPositionY.TabIndex = 63;
            this.trackAlertPopupMinimapPositionY.TickFrequency = 10;
            this.trackAlertPopupMinimapPositionY.Value = 1;
            this.trackAlertPopupMinimapPositionY.Scroll += new System.EventHandler(this.trackAlertPopupMinimapPositionY_Scroll);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(703, 66);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(56, 13);
            this.label10.TabIndex = 62;
            this.label10.Text = "Minimap X";
            // 
            // trackAlertPopupMinimapPositionX
            // 
            this.trackAlertPopupMinimapPositionX.LargeChange = 10;
            this.trackAlertPopupMinimapPositionX.Location = new System.Drawing.Point(765, 64);
            this.trackAlertPopupMinimapPositionX.Maximum = 9999;
            this.trackAlertPopupMinimapPositionX.Name = "trackAlertPopupMinimapPositionX";
            this.trackAlertPopupMinimapPositionX.Size = new System.Drawing.Size(150, 45);
            this.trackAlertPopupMinimapPositionX.TabIndex = 61;
            this.trackAlertPopupMinimapPositionX.TickFrequency = 10;
            this.trackAlertPopupMinimapPositionX.Value = 1;
            this.trackAlertPopupMinimapPositionX.Scroll += new System.EventHandler(this.trackAlertPopupMinimapPositionX_Scroll);
            // 
            // ConfigurationProperties
            // 
            this.ConfigurationProperties.Location = new System.Drawing.Point(8, 171);
            this.ConfigurationProperties.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ConfigurationProperties.Name = "ConfigurationProperties";
            this.ConfigurationProperties.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.ConfigurationProperties.Size = new System.Drawing.Size(464, 431);
            this.ConfigurationProperties.TabIndex = 94;
            this.ConfigurationProperties.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.ColourProperties_PropertyValueChanged);
            this.ConfigurationProperties.Click += new System.EventHandler(this.Properties_Click);
            // 
            // QuesadillaConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 610);
            this.Controls.Add(this.ConfigurationProperties);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.trackAlertPopupMinimapPositionY);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.trackAlertPopupMinimapPositionX);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.trackAlertPopupMinimapSizeHeight);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.trackAlertPopupMinimapSizeWidth);
            this.Controls.Add(this.lblShowLastUnexpiredAlerts);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.trackShowLastUnexpiredAlerts);
            this.Controls.Add(this.lblDPSGraphLineSize);
            this.Controls.Add(this.lblPopupSeconds);
            this.Controls.Add(this.lblOpacity);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.trackDPSGraphLineSize);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.trackPopupTime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.trackOpacity);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbBackground);
            this.Controls.Add(this.alertDemo);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbHomeSystem);
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "QuesadillaConfiguration";
            this.Text = "Quesadilla Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuesadillaConfiguration_FormClosing);
            this.Load += new System.EventHandler(this.QuesadillaConfiguration_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alertDemo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackPopupTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackDPSGraphLineSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackShowLastUnexpiredAlerts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAlertPopupMinimapSizeWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAlertPopupMinimapSizeHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAlertPopupMinimapPositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAlertPopupMinimapPositionX)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbHomeSystem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox alertDemo;
        private System.Windows.Forms.ColorDialog colourPicker;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbBackground;
        private System.Windows.Forms.TrackBar trackOpacity;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackPopupTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar trackDPSGraphLineSize;
        private System.Windows.Forms.Label lblOpacity;
        private System.Windows.Forms.Label lblPopupSeconds;
        private System.Windows.Forms.Label lblDPSGraphLineSize;
        private System.Windows.Forms.Label lblShowLastUnexpiredAlerts;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TrackBar trackShowLastUnexpiredAlerts;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar trackAlertPopupMinimapSizeWidth;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar trackAlertPopupMinimapSizeHeight;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TrackBar trackAlertPopupMinimapPositionY;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TrackBar trackAlertPopupMinimapPositionX;
        private System.Windows.Forms.PropertyGrid ConfigurationProperties;
    }
}