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
            this.cbStayOnTop = new System.Windows.Forms.CheckBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.alertDemo = new System.Windows.Forms.PictureBox();
            this.colourPicker = new System.Windows.Forms.ColorDialog();
            this.buttonPickSystemColour = new System.Windows.Forms.Button();
            this.buttonPickContextColour = new System.Windows.Forms.Button();
            this.buttonPickRegionColour = new System.Windows.Forms.Button();
            this.buttonPickIntelLineColour = new System.Windows.Forms.Button();
            this.buttonPickAlertTextColour = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbBackground = new System.Windows.Forms.ComboBox();
            this.trackOpacity = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.trackPopupTime = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.lblOpacity = new System.Windows.Forms.Label();
            this.lblPopupSeconds = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioAlertCenter = new System.Windows.Forms.RadioButton();
            this.radioAlertBottomRight = new System.Windows.Forms.RadioButton();
            this.radioAlertBottomLeft = new System.Windows.Forms.RadioButton();
            this.radioAlertTopRight = new System.Windows.Forms.RadioButton();
            this.radioAlertTopLeft = new System.Windows.Forms.RadioButton();
            this.cbDPSStayOnTop = new System.Windows.Forms.CheckBox();
            this.cbDPSTransparent = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioDPSCenter = new System.Windows.Forms.RadioButton();
            this.radioDPSBottomRight = new System.Windows.Forms.RadioButton();
            this.radioDPSBottomLeft = new System.Windows.Forms.RadioButton();
            this.radioDPSTopRight = new System.Windows.Forms.RadioButton();
            this.radioDPSTopLeft = new System.Windows.Forms.RadioButton();
            this.cbDPSAll = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alertDemo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackPopupTime)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbHomeSystem
            // 
            this.cmbHomeSystem.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbHomeSystem.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbHomeSystem.FormattingEnabled = true;
            this.cmbHomeSystem.Location = new System.Drawing.Point(874, 370);
            this.cmbHomeSystem.Margin = new System.Windows.Forms.Padding(2);
            this.cmbHomeSystem.Name = "cmbHomeSystem";
            this.cmbHomeSystem.Size = new System.Drawing.Size(99, 21);
            this.cmbHomeSystem.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(798, 373);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Home System";
            // 
            // cbStayOnTop
            // 
            this.cbStayOnTop.AutoSize = true;
            this.cbStayOnTop.Location = new System.Drawing.Point(562, 10);
            this.cbStayOnTop.Margin = new System.Windows.Forms.Padding(2);
            this.cbStayOnTop.Name = "cbStayOnTop";
            this.cbStayOnTop.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbStayOnTop.Size = new System.Drawing.Size(80, 17);
            this.cbStayOnTop.TabIndex = 2;
            this.cbStayOnTop.Text = "Stay on top";
            this.cbStayOnTop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbStayOnTop.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cbStayOnTop.UseVisualStyleBackColor = true;
            this.cbStayOnTop.CheckedChanged += new System.EventHandler(this.cbStayOnTop_CheckedChanged);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(890, 567);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(2);
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
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
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
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
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
            this.alertDemo.Margin = new System.Windows.Forms.Padding(2);
            this.alertDemo.Name = "alertDemo";
            this.alertDemo.Size = new System.Drawing.Size(371, 152);
            this.alertDemo.TabIndex = 6;
            this.alertDemo.TabStop = false;
            // 
            // buttonPickSystemColour
            // 
            this.buttonPickSystemColour.Location = new System.Drawing.Point(383, 27);
            this.buttonPickSystemColour.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPickSystemColour.Name = "buttonPickSystemColour";
            this.buttonPickSystemColour.Size = new System.Drawing.Size(125, 26);
            this.buttonPickSystemColour.TabIndex = 8;
            this.buttonPickSystemColour.Text = "System name colour";
            this.buttonPickSystemColour.UseVisualStyleBackColor = true;
            this.buttonPickSystemColour.Click += new System.EventHandler(this.buttonPickSystemColour_Click);
            // 
            // buttonPickContextColour
            // 
            this.buttonPickContextColour.Location = new System.Drawing.Point(383, 57);
            this.buttonPickContextColour.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPickContextColour.Name = "buttonPickContextColour";
            this.buttonPickContextColour.Size = new System.Drawing.Size(125, 26);
            this.buttonPickContextColour.TabIndex = 9;
            this.buttonPickContextColour.Text = "Context Info Colour";
            this.buttonPickContextColour.UseVisualStyleBackColor = true;
            this.buttonPickContextColour.Click += new System.EventHandler(this.buttonPickContextColour_Click);
            // 
            // buttonPickRegionColour
            // 
            this.buttonPickRegionColour.Location = new System.Drawing.Point(517, 41);
            this.buttonPickRegionColour.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPickRegionColour.Name = "buttonPickRegionColour";
            this.buttonPickRegionColour.Size = new System.Drawing.Size(125, 26);
            this.buttonPickRegionColour.TabIndex = 10;
            this.buttonPickRegionColour.Text = "Region name colour";
            this.buttonPickRegionColour.UseVisualStyleBackColor = true;
            this.buttonPickRegionColour.Click += new System.EventHandler(this.buttonPickRegionColour_Click);
            // 
            // buttonPickIntelLineColour
            // 
            this.buttonPickIntelLineColour.Location = new System.Drawing.Point(517, 71);
            this.buttonPickIntelLineColour.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPickIntelLineColour.Name = "buttonPickIntelLineColour";
            this.buttonPickIntelLineColour.Size = new System.Drawing.Size(125, 26);
            this.buttonPickIntelLineColour.TabIndex = 11;
            this.buttonPickIntelLineColour.Text = "Intel Line Colour";
            this.buttonPickIntelLineColour.UseVisualStyleBackColor = true;
            this.buttonPickIntelLineColour.Click += new System.EventHandler(this.buttonPickIntelLineColour_Click);
            // 
            // buttonPickAlertTextColour
            // 
            this.buttonPickAlertTextColour.Location = new System.Drawing.Point(383, 135);
            this.buttonPickAlertTextColour.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPickAlertTextColour.Name = "buttonPickAlertTextColour";
            this.buttonPickAlertTextColour.Size = new System.Drawing.Size(125, 26);
            this.buttonPickAlertTextColour.TabIndex = 12;
            this.buttonPickAlertTextColour.Text = "Alert Text Colour";
            this.buttonPickAlertTextColour.UseVisualStyleBackColor = true;
            this.buttonPickAlertTextColour.Click += new System.EventHandler(this.buttonPickAlertTextColour_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(120, 171);
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
            this.cmbBackground.Location = new System.Drawing.Point(230, 168);
            this.cmbBackground.Margin = new System.Windows.Forms.Padding(2);
            this.cmbBackground.Name = "cmbBackground";
            this.cmbBackground.Size = new System.Drawing.Size(149, 21);
            this.cmbBackground.TabIndex = 13;
            this.cmbBackground.SelectedIndexChanged += new System.EventHandler(this.cmbBackground_SelectedIndexChanged);
            // 
            // trackOpacity
            // 
            this.trackOpacity.LargeChange = 10;
            this.trackOpacity.Location = new System.Drawing.Point(183, 197);
            this.trackOpacity.Maximum = 100;
            this.trackOpacity.Name = "trackOpacity";
            this.trackOpacity.Size = new System.Drawing.Size(196, 45);
            this.trackOpacity.TabIndex = 15;
            this.trackOpacity.TickFrequency = 10;
            this.trackOpacity.Scroll += new System.EventHandler(this.trackOpacity_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(135, 200);
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
            this.trackPopupTime.Location = new System.Drawing.Point(183, 226);
            this.trackPopupTime.Maximum = 100;
            this.trackPopupTime.Name = "trackPopupTime";
            this.trackPopupTime.Size = new System.Drawing.Size(196, 45);
            this.trackPopupTime.TabIndex = 17;
            this.trackPopupTime.TickFrequency = 10;
            this.trackPopupTime.Scroll += new System.EventHandler(this.trackPopupTime_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(118, 226);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Popup time";
            // 
            // lblOpacity
            // 
            this.lblOpacity.AutoSize = true;
            this.lblOpacity.Location = new System.Drawing.Point(381, 200);
            this.lblOpacity.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOpacity.Name = "lblOpacity";
            this.lblOpacity.Size = new System.Drawing.Size(43, 13);
            this.lblOpacity.TabIndex = 19;
            this.lblOpacity.Text = "Opacity";
            // 
            // lblPopupSeconds
            // 
            this.lblPopupSeconds.AutoSize = true;
            this.lblPopupSeconds.Location = new System.Drawing.Point(381, 229);
            this.lblPopupSeconds.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPopupSeconds.Name = "lblPopupSeconds";
            this.lblPopupSeconds.Size = new System.Drawing.Size(43, 13);
            this.lblPopupSeconds.TabIndex = 20;
            this.lblPopupSeconds.Text = "Opacity";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioAlertCenter);
            this.groupBox1.Controls.Add(this.radioAlertBottomRight);
            this.groupBox1.Controls.Add(this.radioAlertBottomLeft);
            this.groupBox1.Controls.Add(this.radioAlertTopRight);
            this.groupBox1.Controls.Add(this.radioAlertTopLeft);
            this.groupBox1.Location = new System.Drawing.Point(8, 168);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(98, 79);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Popup Location";
            // 
            // radioAlertCenter
            // 
            this.radioAlertCenter.AutoSize = true;
            this.radioAlertCenter.Location = new System.Drawing.Point(43, 40);
            this.radioAlertCenter.Name = "radioAlertCenter";
            this.radioAlertCenter.Size = new System.Drawing.Size(14, 13);
            this.radioAlertCenter.TabIndex = 30;
            this.radioAlertCenter.TabStop = true;
            this.radioAlertCenter.UseVisualStyleBackColor = true;
            this.radioAlertCenter.CheckedChanged += new System.EventHandler(this.radioAlertCentre_CheckedChanged);
            // 
            // radioAlertBottomRight
            // 
            this.radioAlertBottomRight.AutoSize = true;
            this.radioAlertBottomRight.Location = new System.Drawing.Point(76, 58);
            this.radioAlertBottomRight.Name = "radioAlertBottomRight";
            this.radioAlertBottomRight.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioAlertBottomRight.Size = new System.Drawing.Size(14, 13);
            this.radioAlertBottomRight.TabIndex = 29;
            this.radioAlertBottomRight.TabStop = true;
            this.radioAlertBottomRight.UseVisualStyleBackColor = true;
            this.radioAlertBottomRight.CheckedChanged += new System.EventHandler(this.radioAlertBottomRight_CheckedChanged);
            // 
            // radioAlertBottomLeft
            // 
            this.radioAlertBottomLeft.AutoSize = true;
            this.radioAlertBottomLeft.Location = new System.Drawing.Point(6, 58);
            this.radioAlertBottomLeft.Name = "radioAlertBottomLeft";
            this.radioAlertBottomLeft.Size = new System.Drawing.Size(14, 13);
            this.radioAlertBottomLeft.TabIndex = 28;
            this.radioAlertBottomLeft.TabStop = true;
            this.radioAlertBottomLeft.UseVisualStyleBackColor = true;
            this.radioAlertBottomLeft.CheckedChanged += new System.EventHandler(this.radioAlertBottomLeft_CheckedChanged);
            // 
            // radioAlertTopRight
            // 
            this.radioAlertTopRight.AutoSize = true;
            this.radioAlertTopRight.Location = new System.Drawing.Point(76, 19);
            this.radioAlertTopRight.Name = "radioAlertTopRight";
            this.radioAlertTopRight.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioAlertTopRight.Size = new System.Drawing.Size(14, 13);
            this.radioAlertTopRight.TabIndex = 27;
            this.radioAlertTopRight.TabStop = true;
            this.radioAlertTopRight.UseVisualStyleBackColor = true;
            this.radioAlertTopRight.CheckedChanged += new System.EventHandler(this.radioAlertTopRight_CheckedChanged);
            // 
            // radioAlertTopLeft
            // 
            this.radioAlertTopLeft.AutoSize = true;
            this.radioAlertTopLeft.Location = new System.Drawing.Point(6, 21);
            this.radioAlertTopLeft.Name = "radioAlertTopLeft";
            this.radioAlertTopLeft.Size = new System.Drawing.Size(14, 13);
            this.radioAlertTopLeft.TabIndex = 26;
            this.radioAlertTopLeft.TabStop = true;
            this.radioAlertTopLeft.UseVisualStyleBackColor = true;
            this.radioAlertTopLeft.CheckedChanged += new System.EventHandler(this.radioAlertTopLeft_CheckedChanged);
            // 
            // cbDPSStayOnTop
            // 
            this.cbDPSStayOnTop.AutoSize = true;
            this.cbDPSStayOnTop.Location = new System.Drawing.Point(8, 254);
            this.cbDPSStayOnTop.Margin = new System.Windows.Forms.Padding(2);
            this.cbDPSStayOnTop.Name = "cbDPSStayOnTop";
            this.cbDPSStayOnTop.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbDPSStayOnTop.Size = new System.Drawing.Size(105, 17);
            this.cbDPSStayOnTop.TabIndex = 27;
            this.cbDPSStayOnTop.Text = "DPS Stay on top";
            this.cbDPSStayOnTop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbDPSStayOnTop.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cbDPSStayOnTop.UseVisualStyleBackColor = true;
            this.cbDPSStayOnTop.CheckedChanged += new System.EventHandler(this.cbDPSStayOnTop_CheckedChanged);
            // 
            // cbDPSTransparent
            // 
            this.cbDPSTransparent.AutoSize = true;
            this.cbDPSTransparent.Location = new System.Drawing.Point(5, 275);
            this.cbDPSTransparent.Margin = new System.Windows.Forms.Padding(2);
            this.cbDPSTransparent.Name = "cbDPSTransparent";
            this.cbDPSTransparent.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbDPSTransparent.Size = new System.Drawing.Size(108, 17);
            this.cbDPSTransparent.TabIndex = 28;
            this.cbDPSTransparent.Text = "DPS Transparent";
            this.cbDPSTransparent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbDPSTransparent.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cbDPSTransparent.UseVisualStyleBackColor = true;
            this.cbDPSTransparent.CheckedChanged += new System.EventHandler(this.cbDPSTransparent_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioDPSCenter);
            this.groupBox2.Controls.Add(this.radioDPSBottomRight);
            this.groupBox2.Controls.Add(this.radioDPSBottomLeft);
            this.groupBox2.Controls.Add(this.radioDPSTopRight);
            this.groupBox2.Controls.Add(this.radioDPSTopLeft);
            this.groupBox2.Location = new System.Drawing.Point(8, 318);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(98, 79);
            this.groupBox2.TabIndex = 29;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "DPS Location";
            // 
            // radioDPSCenter
            // 
            this.radioDPSCenter.AutoSize = true;
            this.radioDPSCenter.Location = new System.Drawing.Point(43, 40);
            this.radioDPSCenter.Name = "radioDPSCenter";
            this.radioDPSCenter.Size = new System.Drawing.Size(14, 13);
            this.radioDPSCenter.TabIndex = 30;
            this.radioDPSCenter.TabStop = true;
            this.radioDPSCenter.UseVisualStyleBackColor = true;
            this.radioDPSCenter.CheckedChanged += new System.EventHandler(this.radioDPSCenter_CheckedChanged);
            // 
            // radioDPSBottomRight
            // 
            this.radioDPSBottomRight.AutoSize = true;
            this.radioDPSBottomRight.Location = new System.Drawing.Point(76, 58);
            this.radioDPSBottomRight.Name = "radioDPSBottomRight";
            this.radioDPSBottomRight.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioDPSBottomRight.Size = new System.Drawing.Size(14, 13);
            this.radioDPSBottomRight.TabIndex = 29;
            this.radioDPSBottomRight.TabStop = true;
            this.radioDPSBottomRight.UseVisualStyleBackColor = true;
            this.radioDPSBottomRight.CheckedChanged += new System.EventHandler(this.radioDPSBottomRight_CheckedChanged);
            // 
            // radioDPSBottomLeft
            // 
            this.radioDPSBottomLeft.AutoSize = true;
            this.radioDPSBottomLeft.Location = new System.Drawing.Point(6, 58);
            this.radioDPSBottomLeft.Name = "radioDPSBottomLeft";
            this.radioDPSBottomLeft.Size = new System.Drawing.Size(14, 13);
            this.radioDPSBottomLeft.TabIndex = 28;
            this.radioDPSBottomLeft.TabStop = true;
            this.radioDPSBottomLeft.UseVisualStyleBackColor = true;
            this.radioDPSBottomLeft.CheckedChanged += new System.EventHandler(this.radioDPSBottomLeft_CheckedChanged);
            // 
            // radioDPSTopRight
            // 
            this.radioDPSTopRight.AutoSize = true;
            this.radioDPSTopRight.Location = new System.Drawing.Point(76, 19);
            this.radioDPSTopRight.Name = "radioDPSTopRight";
            this.radioDPSTopRight.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioDPSTopRight.Size = new System.Drawing.Size(14, 13);
            this.radioDPSTopRight.TabIndex = 27;
            this.radioDPSTopRight.TabStop = true;
            this.radioDPSTopRight.UseVisualStyleBackColor = true;
            this.radioDPSTopRight.CheckedChanged += new System.EventHandler(this.radioDPSTopRight_CheckedChanged);
            // 
            // radioDPSTopLeft
            // 
            this.radioDPSTopLeft.AutoSize = true;
            this.radioDPSTopLeft.Location = new System.Drawing.Point(6, 21);
            this.radioDPSTopLeft.Name = "radioDPSTopLeft";
            this.radioDPSTopLeft.Size = new System.Drawing.Size(14, 13);
            this.radioDPSTopLeft.TabIndex = 26;
            this.radioDPSTopLeft.TabStop = true;
            this.radioDPSTopLeft.UseVisualStyleBackColor = true;
            this.radioDPSTopLeft.CheckedChanged += new System.EventHandler(this.radioDPSTopLeft_CheckedChanged);
            // 
            // cbDPSAll
            // 
            this.cbDPSAll.AutoSize = true;
            this.cbDPSAll.Location = new System.Drawing.Point(21, 296);
            this.cbDPSAll.Margin = new System.Windows.Forms.Padding(2);
            this.cbDPSAll.Name = "cbDPSAll";
            this.cbDPSAll.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbDPSAll.Size = new System.Drawing.Size(92, 17);
            this.cbDPSAll.TabIndex = 30;
            this.cbDPSAll.Text = "DPS All Chars";
            this.cbDPSAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbDPSAll.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cbDPSAll.UseVisualStyleBackColor = true;
            this.cbDPSAll.CheckedChanged += new System.EventHandler(this.cbDPSAll_CheckedChanged);
            // 
            // QuesadillaConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 610);
            this.Controls.Add(this.cbDPSAll);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.cbDPSTransparent);
            this.Controls.Add(this.cbDPSStayOnTop);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblPopupSeconds);
            this.Controls.Add(this.lblOpacity);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.trackPopupTime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.trackOpacity);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbBackground);
            this.Controls.Add(this.buttonPickAlertTextColour);
            this.Controls.Add(this.buttonPickIntelLineColour);
            this.Controls.Add(this.buttonPickRegionColour);
            this.Controls.Add(this.buttonPickContextColour);
            this.Controls.Add(this.buttonPickSystemColour);
            this.Controls.Add(this.alertDemo);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.cbStayOnTop);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbHomeSystem);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "QuesadillaConfiguration";
            this.Text = "Quesadilla Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuesadillaConfiguration_FormClosing);
            this.Load += new System.EventHandler(this.QuesadillaConfiguration_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alertDemo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackPopupTime)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbHomeSystem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbStayOnTop;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox alertDemo;
        private System.Windows.Forms.ColorDialog colourPicker;
        private System.Windows.Forms.Button buttonPickSystemColour;
        private System.Windows.Forms.Button buttonPickContextColour;
        private System.Windows.Forms.Button buttonPickRegionColour;
        private System.Windows.Forms.Button buttonPickIntelLineColour;
        private System.Windows.Forms.Button buttonPickAlertTextColour;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbBackground;
        private System.Windows.Forms.TrackBar trackOpacity;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackPopupTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblOpacity;
        private System.Windows.Forms.Label lblPopupSeconds;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioAlertCenter;
        private System.Windows.Forms.RadioButton radioAlertBottomRight;
        private System.Windows.Forms.RadioButton radioAlertBottomLeft;
        private System.Windows.Forms.RadioButton radioAlertTopRight;
        private System.Windows.Forms.RadioButton radioAlertTopLeft;
        private System.Windows.Forms.CheckBox cbDPSStayOnTop;
        private System.Windows.Forms.CheckBox cbDPSTransparent;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioDPSCenter;
        private System.Windows.Forms.RadioButton radioDPSBottomRight;
        private System.Windows.Forms.RadioButton radioDPSBottomLeft;
        private System.Windows.Forms.RadioButton radioDPSTopRight;
        private System.Windows.Forms.RadioButton radioDPSTopLeft;
        private System.Windows.Forms.CheckBox cbDPSAll;
    }
}