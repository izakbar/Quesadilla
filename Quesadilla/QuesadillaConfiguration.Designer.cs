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
			this.buttonSave = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.colourPicker = new System.Windows.Forms.ColorDialog();
			this.MiddlePanel = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.txtFilterProperties = new System.Windows.Forms.TextBox();
			this.cmbFilterCategory = new System.Windows.Forms.ComboBox();
			this.ConfigurationProperties = new System.Windows.Forms.PropertyGrid();
			this.TopPanel = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.cmbBackground = new System.Windows.Forms.ComboBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cmbHomeSystem = new System.Windows.Forms.ComboBox();
			this.alertDemo = new System.Windows.Forms.PictureBox();
			this.BottomPanel = new System.Windows.Forms.Panel();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.MiddlePanel.SuspendLayout();
			this.panel4.SuspendLayout();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.alertDemo)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonSave
			// 
			this.buttonSave.Location = new System.Drawing.Point(98, 6);
			this.buttonSave.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(83, 25);
			this.buttonSave.TabIndex = 3;
			this.buttonSave.Text = "Apply";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// MiddlePanel
			// 
			this.MiddlePanel.Controls.Add(this.panel4);
			this.MiddlePanel.Controls.Add(this.ConfigurationProperties);
			this.MiddlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddlePanel.Location = new System.Drawing.Point(0, 237);
			this.MiddlePanel.Name = "MiddlePanel";
			this.MiddlePanel.Size = new System.Drawing.Size(651, 417);
			this.MiddlePanel.TabIndex = 97;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.txtFilterProperties);
			this.panel4.Controls.Add(this.cmbFilterCategory);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(651, 30);
			this.panel4.TabIndex = 102;
			// 
			// txtFilterProperties
			// 
			this.txtFilterProperties.Location = new System.Drawing.Point(4, 5);
			this.txtFilterProperties.Name = "txtFilterProperties";
			this.txtFilterProperties.Size = new System.Drawing.Size(154, 20);
			this.txtFilterProperties.TabIndex = 102;
			this.txtFilterProperties.TextChanged += new System.EventHandler(this.txtFilterProperties_TextChanged);
			// 
			// cmbFilterCategory
			// 
			this.cmbFilterCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbFilterCategory.FormattingEnabled = true;
			this.cmbFilterCategory.Location = new System.Drawing.Point(170, 4);
			this.cmbFilterCategory.Name = "cmbFilterCategory";
			this.cmbFilterCategory.Size = new System.Drawing.Size(175, 21);
			this.cmbFilterCategory.TabIndex = 101;
			this.cmbFilterCategory.SelectedIndexChanged += new System.EventHandler(this.cmbFilterCategory_SelectedIndexChanged);
			// 
			// ConfigurationProperties
			// 
			this.ConfigurationProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationProperties.Location = new System.Drawing.Point(0, 0);
			this.ConfigurationProperties.Margin = new System.Windows.Forms.Padding(2);
			this.ConfigurationProperties.Name = "ConfigurationProperties";
			this.ConfigurationProperties.PropertySort = System.Windows.Forms.PropertySort.Categorized;
			this.ConfigurationProperties.Size = new System.Drawing.Size(651, 417);
			this.ConfigurationProperties.TabIndex = 95;
			this.ConfigurationProperties.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.ConfigurationProperties_PropertyValueChanged);
			this.ConfigurationProperties.SelectedObjectsChanged += new System.EventHandler(this.ConfigurationProperties_SelectedObjectsChanged);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.label2);
			this.TopPanel.Controls.Add(this.cmbBackground);
			this.TopPanel.Controls.Add(this.pictureBox1);
			this.TopPanel.Controls.Add(this.label1);
			this.TopPanel.Controls.Add(this.cmbHomeSystem);
			this.TopPanel.Controls.Add(this.alertDemo);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = new System.Drawing.Point(0, 0);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = new System.Drawing.Size(651, 237);
			this.TopPanel.TabIndex = 100;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(399, 208);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(65, 13);
			this.label2.TabIndex = 48;
			this.label2.Text = "Background";
			// 
			// cmbBackground
			// 
			this.cmbBackground.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.cmbBackground.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.cmbBackground.FormattingEnabled = true;
			this.cmbBackground.Location = new System.Drawing.Point(495, 205);
			this.cmbBackground.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.cmbBackground.Name = "cmbBackground";
			this.cmbBackground.Size = new System.Drawing.Size(149, 21);
			this.cmbBackground.TabIndex = 47;
			this.cmbBackground.SelectedIndexChanged += new System.EventHandler(this.cmbBackground_SelectedIndexChanged);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Quesadilla.Properties.Resources.Quesadilla;
			this.pictureBox1.Location = new System.Drawing.Point(16, 32);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(172, 168);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 10;
			this.pictureBox1.TabStop = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "Home System";
			// 
			// cmbHomeSystem
			// 
			this.cmbHomeSystem.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.cmbHomeSystem.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.cmbHomeSystem.FormattingEnabled = true;
			this.cmbHomeSystem.Location = new System.Drawing.Point(89, 7);
			this.cmbHomeSystem.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.cmbHomeSystem.Name = "cmbHomeSystem";
			this.cmbHomeSystem.Size = new System.Drawing.Size(99, 21);
			this.cmbHomeSystem.TabIndex = 8;
			this.cmbHomeSystem.SelectedIndexChanged += new System.EventHandler(this.cmbHomeSystem_SelectedIndexChanged);
			// 
			// alertDemo
			// 
			this.alertDemo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.alertDemo.Location = new System.Drawing.Point(198, 7);
			this.alertDemo.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.alertDemo.Name = "alertDemo";
			this.alertDemo.Size = new System.Drawing.Size(446, 193);
			this.alertDemo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.alertDemo.TabIndex = 7;
			this.alertDemo.TabStop = false;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.buttonCancel);
			this.BottomPanel.Controls.Add(this.buttonSave);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = new System.Drawing.Point(0, 654);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = new System.Drawing.Size(651, 39);
			this.BottomPanel.TabIndex = 98;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(11, 6);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(83, 25);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "Close";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// QuesadillaConfiguration
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(651, 693);
			this.Controls.Add(this.MiddlePanel);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.BottomPanel);
			this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.Name = "QuesadillaConfiguration";
			this.Text = "Quesadilla Configuration";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuesadillaConfiguration_FormClosing);
			this.Load += new System.EventHandler(this.QuesadillaConfiguration_Load);
			this.ResizeEnd += new System.EventHandler(this.QuesadillaConfiguration_ResizeEnd);
			this.MiddlePanel.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.alertDemo)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ColorDialog colourPicker;
		private System.Windows.Forms.Panel MiddlePanel;
		private System.Windows.Forms.PropertyGrid ConfigurationProperties;
		private System.Windows.Forms.Panel BottomPanel;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Panel TopPanel;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbHomeSystem;
		private System.Windows.Forms.PictureBox alertDemo;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cmbBackground;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.TextBox txtFilterProperties;
		private System.Windows.Forms.ComboBox cmbFilterCategory;
	}
}