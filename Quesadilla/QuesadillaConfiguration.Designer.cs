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
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alertDemo)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbHomeSystem
            // 
            this.cmbHomeSystem.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbHomeSystem.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbHomeSystem.FormattingEnabled = true;
            this.cmbHomeSystem.Location = new System.Drawing.Point(127, 253);
            this.cmbHomeSystem.Name = "cmbHomeSystem";
            this.cmbHomeSystem.Size = new System.Drawing.Size(158, 28);
            this.cmbHomeSystem.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 256);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Home System";
            // 
            // cbStayOnTop
            // 
            this.cbStayOnTop.AutoSize = true;
            this.cbStayOnTop.Location = new System.Drawing.Point(452, 257);
            this.cbStayOnTop.Name = "cbStayOnTop";
            this.cbStayOnTop.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbStayOnTop.Size = new System.Drawing.Size(116, 24);
            this.cbStayOnTop.TabIndex = 2;
            this.cbStayOnTop.Text = "Stay on top";
            this.cbStayOnTop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbStayOnTop.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cbStayOnTop.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(890, 860);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(124, 55);
            this.buttonSave.TabIndex = 3;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(756, 860);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(124, 55);
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
            this.pictureBox1.Location = new System.Drawing.Point(756, 596);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(258, 258);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // alertDemo
            // 
            this.alertDemo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.alertDemo.Location = new System.Drawing.Point(12, 15);
            this.alertDemo.Name = "alertDemo";
            this.alertDemo.Size = new System.Drawing.Size(556, 232);
            this.alertDemo.TabIndex = 6;
            this.alertDemo.TabStop = false;
            // 
            // buttonPickSystemColour
            // 
            this.buttonPickSystemColour.Location = new System.Drawing.Point(574, 41);
            this.buttonPickSystemColour.Name = "buttonPickSystemColour";
            this.buttonPickSystemColour.Size = new System.Drawing.Size(187, 40);
            this.buttonPickSystemColour.TabIndex = 8;
            this.buttonPickSystemColour.Text = "System name colour";
            this.buttonPickSystemColour.UseVisualStyleBackColor = true;
            this.buttonPickSystemColour.Click += new System.EventHandler(this.buttonPickSystemColour_Click);
            // 
            // buttonPickContextColour
            // 
            this.buttonPickContextColour.Location = new System.Drawing.Point(574, 87);
            this.buttonPickContextColour.Name = "buttonPickContextColour";
            this.buttonPickContextColour.Size = new System.Drawing.Size(187, 40);
            this.buttonPickContextColour.TabIndex = 9;
            this.buttonPickContextColour.Text = "Context Info Colour";
            this.buttonPickContextColour.UseVisualStyleBackColor = true;
            this.buttonPickContextColour.Click += new System.EventHandler(this.buttonPickContextColour_Click);
            // 
            // buttonPickRegionColour
            // 
            this.buttonPickRegionColour.Location = new System.Drawing.Point(776, 63);
            this.buttonPickRegionColour.Name = "buttonPickRegionColour";
            this.buttonPickRegionColour.Size = new System.Drawing.Size(187, 40);
            this.buttonPickRegionColour.TabIndex = 10;
            this.buttonPickRegionColour.Text = "Region name colour";
            this.buttonPickRegionColour.UseVisualStyleBackColor = true;
            this.buttonPickRegionColour.Click += new System.EventHandler(this.buttonPickRegionColour_Click);
            // 
            // buttonPickIntelLineColour
            // 
            this.buttonPickIntelLineColour.Location = new System.Drawing.Point(776, 109);
            this.buttonPickIntelLineColour.Name = "buttonPickIntelLineColour";
            this.buttonPickIntelLineColour.Size = new System.Drawing.Size(187, 40);
            this.buttonPickIntelLineColour.TabIndex = 11;
            this.buttonPickIntelLineColour.Text = "Intel Line Colour";
            this.buttonPickIntelLineColour.UseVisualStyleBackColor = true;
            this.buttonPickIntelLineColour.Click += new System.EventHandler(this.buttonPickIntelLineColour_Click);
            // 
            // buttonPickAlertTextColour
            // 
            this.buttonPickAlertTextColour.Location = new System.Drawing.Point(574, 207);
            this.buttonPickAlertTextColour.Name = "buttonPickAlertTextColour";
            this.buttonPickAlertTextColour.Size = new System.Drawing.Size(187, 40);
            this.buttonPickAlertTextColour.TabIndex = 12;
            this.buttonPickAlertTextColour.Text = "Alert Text Colour";
            this.buttonPickAlertTextColour.UseVisualStyleBackColor = true;
            this.buttonPickAlertTextColour.Click += new System.EventHandler(this.buttonPickAlertTextColour_Click);
            // 
            // QuesadillaConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1042, 946);
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
            this.Name = "QuesadillaConfiguration";
            this.Text = "Quesadilla Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuesadillaConfiguration_FormClosing);
            this.Load += new System.EventHandler(this.QuesadillaConfiguration_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alertDemo)).EndInit();
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
    }
}