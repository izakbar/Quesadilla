using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool UpdateLayeredWindow(
			IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
			IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

		[DllImport("user32.dll")]
		private static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hbitmap);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(out POINT lpPoint);

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT { public int X, Y; public POINT(int x, int y) { X = x; Y = y; } }

		[StructLayout(LayoutKind.Sequential)]
		private struct SIZE { public int cx, cy; public SIZE(int cx, int cy) { this.cx = cx; this.cy = cy; } }


		[StructLayout(LayoutKind.Sequential)]
		private struct BLENDFUNCTION
		{
			public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat;
		}

		// mouse tracking
		private readonly Timer _mouseTrackTimer = new Timer();
		private Point _lastMousePos = new Point(int.MinValue, int.MinValue);
		private bool _mouseWasInside = false;
		private bool _enableMouseTracking = false;

		private const int ULW_ALPHA = 0x02;
		private const byte AC_SRC_OVER = 0x00;
		private const byte AC_SRC_ALPHA = 0x01;

		private const int WS_EX_LAYERED = 0x80000;
		private const int WS_EX_TRANSPARENT = 0x20;
		private const int WS_EX_NOACTIVATE = 0x08000000;
		private const int WS_EX_TOOLWINDOW = 0x00000080;


		public TransparentOverlayForm()
		{
			TopMost = true;
			ApplyTransparentOverlayStyle(); 
			InitMouseTracking();
		}

		protected void ApplyTransparentOverlayStyle()
		{
			if (DesignMode) return; // designer shows an opaque form; that's expected

//			BackColor = TransparentColour;
//			TransparencyKey = TransparentColour;

			FormBorderStyle = FormBorderStyle.None;
			ControlBox = false;
			MinimizeBox = false;
			MaximizeBox = false;
			ShowInTaskbar = false;
			Text = "121212";

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque, true);
			if (IsHandleCreated) RecreateHandle();


		}

		protected virtual void RenderOverlay(Graphics g) { }
		private void UpdateLayered(Bitmap bitmap)
		{
			IntPtr screenDc = GetDC(IntPtr.Zero);
			IntPtr memDc = CreateCompatibleDC(screenDc);
			IntPtr hBitmap = IntPtr.Zero;
			IntPtr oldBitmap = IntPtr.Zero;

			try
			{
				hBitmap = bitmap.GetHbitmap(Color.FromArgb(0)); // preserve per-pixel alpha
				oldBitmap = SelectObject(memDc, hBitmap);

				var size = new SIZE(bitmap.Width, bitmap.Height);
				var pointSrc = new POINT(0, 0);
				var topPos = new POINT(Left, Top);

				var blend = new BLENDFUNCTION
				{
					BlendOp = AC_SRC_OVER,
					BlendFlags = 0,
					SourceConstantAlpha = 255, // per-pixel alpha drives blending, not a global fade
					AlphaFormat = AC_SRC_ALPHA
				};

				UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSrc, 0, ref blend, ULW_ALPHA);
			}
			finally
			{
				SelectObject(memDc, oldBitmap);
				if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
				DeleteDC(memDc);
				ReleaseDC(IntPtr.Zero, screenDc);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			RenderAndUpdate();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			RenderAndUpdate();
		}

		// Because UpdateLayeredWindow bypasses the normal paint pipeline,
		// suppress the default background erase/paint to avoid flicker/artifacts.
		protected override void OnPaintBackground(PaintEventArgs e) { }

		public void RenderAndUpdate()
		{
			if (DesignMode || !IsHandleCreated) return;
			if (Width <= 0 || Height <= 0) return;

			using (var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
			{
				using (var g = Graphics.FromImage(bitmap))
				{
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
					//g.Clear(Color.Transparent); // fully transparent, not colour-keyed
					RenderOverlay(g);
				}

				UpdateLayered(bitmap);
			}
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

					cp.ExStyle |= WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
						cp.ExStyle |= WS_EX_TRANSPARENT;
				}

				return cp;
			}
		}

		protected bool EnableMouseTracking
		{
			get => _enableMouseTracking;
			set
			{
				_enableMouseTracking = value;
				_mouseTrackTimer.Enabled = value && IsHandleCreated;
			}
		}

		protected int MouseTrackIntervalMs
		{
			get => _mouseTrackTimer.Interval;
			set => _mouseTrackTimer.Interval = value;
		}

		private void InitMouseTracking()
		{
			_mouseTrackTimer.Interval = 33; // ~30 checks/sec by default
			_mouseTrackTimer.Tick += MouseTrackTimer_Tick;
		}

		private void MouseTrackTimer_Tick(object sender, EventArgs e)
		{
			if (!IsHandleCreated || !Visible) return;
			if (!GetCursorPos(out POINT screenPt)) return;

			Point clientPt = PointToClient(new Point(screenPt.X, screenPt.Y));
			bool inside = ClientRectangle.Contains(clientPt);

			if (inside)
			{
				if (clientPt != _lastMousePos)
				{
					_lastMousePos = clientPt;
					_mouseWasInside = true;
					OnOverlayMouseMove(clientPt);
				}
			}
			else if (_mouseWasInside)
			{
				_mouseWasInside = false;
				_lastMousePos = new Point(int.MinValue, int.MinValue);
				OnOverlayMouseLeave();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) _mouseTrackTimer.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (EnableMouseTracking) _mouseTrackTimer.Enabled = true;
		}

		protected virtual void OnOverlayMouseMove(Point clientLocation) { }
		protected virtual void OnOverlayMouseLeave() { }
	}
}
