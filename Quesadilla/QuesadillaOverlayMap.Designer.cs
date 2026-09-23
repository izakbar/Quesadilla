using System.Drawing.Imaging;

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

			ImageAttrMainMap?.Dispose();
			ImageAttrISKGraph?.Dispose();
			ImageAttrMinedGraph?.Dispose();
			ImageAttrMouseOverGraph?.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// QuesadillaOverlayMap
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.ClientSize = new System.Drawing.Size(558, 557);
			this.Enabled = false;
			this.ForeColor = System.Drawing.SystemColors.ButtonFace;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "QuesadillaOverlayMap";
			this.Text = "Map Overlay";
			this.ResumeLayout(false);

		}

		#endregion
	}
}