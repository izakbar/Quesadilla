namespace Quesadilla
{
	partial class QuesadillaOverlayMap
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
			this.OverlayMinimap = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.OverlayMinimap)).BeginInit();
			this.SuspendLayout();
			// 
			// OverlayMinimap
			// 
			this.OverlayMinimap.BackColor = System.Drawing.Color.Transparent;
			this.OverlayMinimap.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OverlayMinimap.Location = new System.Drawing.Point(0, 0);
			this.OverlayMinimap.Name = "OverlayMinimap";
			this.OverlayMinimap.Size = new System.Drawing.Size(372, 362);
			this.OverlayMinimap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.OverlayMinimap.TabIndex = 11;
			this.OverlayMinimap.TabStop = false;
			// 
			// QuesadillaOverlayMap
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(372, 362);
			this.Controls.Add(this.OverlayMinimap);
			this.Name = "QuesadillaOverlayMap";
			this.Text = "Map Overlay";
			((System.ComponentModel.ISupportInitialize)(this.OverlayMinimap)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox OverlayMinimap;
	}
}