using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Quesadilla.Classes
{
	public class TransparentOverlayForm : Form
	{
		/// <summary>
		/// The colorkey used for transparency. Pick something your rendering
		/// will never legitimately produce (avoid clean colors like pure
		/// magenta/black if your palette has gradients that could pass
		/// through them). Set this BEFORE calling ApplyTransparentOverlayStyle().
		/// </summary>
		public Color TransparentColour { get; set; } = Color.FromArgb(255, 3, 254, 1);
		
		public TransparentOverlayForm()
		{
			TopMost = true;
			ApplyTransparentOverlayStyle(); // <-- this was missing; without it TransparencyKey is never set
		}

		protected void ApplyTransparentOverlayStyle()
		{
			if (DesignMode) return; // designer shows an opaque form; that's expected

			BackColor = TransparentColour;
			TransparencyKey = TransparentColour;

			FormBorderStyle = FormBorderStyle.None;
			ControlBox = false;
			MinimizeBox = false;
			MaximizeBox = false;
			ShowInTaskbar = false;
			Text = "121212";

			if (IsHandleCreated) RecreateHandle();
		}

		// Never steals focus from the EVE client when shown.
		protected override bool ShowWithoutActivation => true;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;

				if (!DesignMode)
				{

					const int WS_EX_LAYERED = 0x80000;
					const int WS_EX_TRANSPARENT = 0x20;
					const int WS_EX_NOACTIVATE = 0x08000000;
					const int WS_EX_TOOLWINDOW = 0x00000080; // hide from alt-tab/taskbar

					cp.ExStyle |= WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
						cp.ExStyle |= WS_EX_TRANSPARENT;
				}

				return cp;
			}
		}
	}
}
