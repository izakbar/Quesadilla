namespace Quesadilla
{
    partial class QuesadillaSplashScreen
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuesadillaSplashScreen));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.paulmart = new System.Windows.Forms.PictureBox();
			this.corp = new System.Windows.Forms.PictureBox();
			this.alliance = new System.Windows.Forms.PictureBox();
			this.izakbar = new System.Windows.Forms.PictureBox();
			this.timerOpen = new System.Windows.Forms.Timer(this.components);
			this.VersionNumber = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.paulmart)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.corp)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.alliance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.izakbar)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Quesadilla.Properties.Resources.Quesadilla;
			this.pictureBox1.Location = new System.Drawing.Point(12, 12);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(256, 255);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// richTextBox1
			// 
			this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Enabled = false;
			this.richTextBox1.Location = new System.Drawing.Point(274, 12);
			this.richTextBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(632, 372);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
			this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
			// 
			// paulmart
			// 
			this.paulmart.Image = global::Quesadilla.Properties.Resources.Paulmart_small;
			this.paulmart.Location = new System.Drawing.Point(87, 274);
			this.paulmart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.paulmart.Name = "paulmart";
			this.paulmart.Size = new System.Drawing.Size(182, 49);
			this.paulmart.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.paulmart.TabIndex = 4;
			this.paulmart.TabStop = false;
			// 
			// corp
			// 
			this.corp.Image = global::Quesadilla.Properties.Resources.therapy;
			this.corp.Location = new System.Drawing.Point(832, 388);
			this.corp.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.corp.Name = "corp";
			this.corp.Size = new System.Drawing.Size(85, 88);
			this.corp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.corp.TabIndex = 5;
			this.corp.TabStop = false;
			this.corp.Click += new System.EventHandler(this.corp_Click);
			// 
			// alliance
			// 
			this.alliance.Image = global::Quesadilla.Properties.Resources.Insidiouslogo;
			this.alliance.Location = new System.Drawing.Point(742, 388);
			this.alliance.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.alliance.Name = "alliance";
			this.alliance.Size = new System.Drawing.Size(84, 88);
			this.alliance.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.alliance.TabIndex = 6;
			this.alliance.TabStop = false;
			this.alliance.Click += new System.EventHandler(this.alliance_Click);
			// 
			// izakbar
			// 
			this.izakbar.BackColor = System.Drawing.Color.Transparent;
			this.izakbar.Image = ((System.Drawing.Image)(resources.GetObject("izakbar.Image")));
			this.izakbar.Location = new System.Drawing.Point(12, 274);
			this.izakbar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.izakbar.Name = "izakbar";
			this.izakbar.Size = new System.Drawing.Size(69, 98);
			this.izakbar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.izakbar.TabIndex = 7;
			this.izakbar.TabStop = false;
			this.izakbar.Click += new System.EventHandler(this.izakbar_Click);
			// 
			// timerOpen
			// 
			this.timerOpen.Enabled = true;
			this.timerOpen.Interval = 50;
			this.timerOpen.Tick += new System.EventHandler(this.timerOpen_Tick);
			// 
			// VersionNumber
			// 
			this.VersionNumber.AutoSize = true;
			this.VersionNumber.Location = new System.Drawing.Point(32, 32);
			this.VersionNumber.Name = "VersionNumber";
			this.VersionNumber.Size = new System.Drawing.Size(75, 20);
			this.VersionNumber.TabIndex = 8;
			this.VersionNumber.Text = "XXXXXX";
			// 
			// QuesadillaSplashScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(916, 482);
			this.ControlBox = false;
			this.Controls.Add(this.VersionNumber);
			this.Controls.Add(this.izakbar);
			this.Controls.Add(this.alliance);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.corp);
			this.Controls.Add(this.paulmart);
			this.Controls.Add(this.richTextBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "QuesadillaSplashScreen";
			this.Opacity = 0D;
			this.Load += new System.EventHandler(this.QuesadillaSplashScreen_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.paulmart)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.corp)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.alliance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.izakbar)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.PictureBox paulmart;
        private System.Windows.Forms.PictureBox corp;
        private System.Windows.Forms.PictureBox alliance;
        private System.Windows.Forms.PictureBox izakbar;
        private System.Windows.Forms.Timer timerOpen;
        private System.Windows.Forms.Label VersionNumber;
    }
}