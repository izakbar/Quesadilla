using Quesadilla.Classes.InterOp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
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


		// mouse tracking
		private readonly Timer _mouseTrackTimer = new Timer();
		private Point _lastMousePos = new Point(int.MinValue, int.MinValue);
		private bool _mouseWasInside = false;
		private bool _enableMouseTracking = false;

		private IntPtr _memDc;
		private IntPtr _dibBitmap;
		private IntPtr _dibBits;
		private Bitmap _bitmap;
		private IntPtr _oldBitmap;

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
		public void UpdateLayered(
				IntPtr hwnd,
				int left,
				int top)
			{
			IntPtr screenDc = User32NativeMethods.GetDC(IntPtr.Zero);

			try
			{
				var dst = new User32NativeMethods.POINT(left, top);
				var src = new User32NativeMethods.POINT(0, 0);
				
				var size = new User32NativeMethods.SIZE(_bitmap.Width, _bitmap.Height);

				var blend = new User32NativeMethods.BLENDFUNCTION
				{
					BlendOp = User32NativeMethods.AC_SRC_OVER,
					BlendFlags = 0,
					SourceConstantAlpha = 255, // per-pixel alpha drives blending, not a global fade
					AlphaFormat = User32NativeMethods.AC_SRC_ALPHA
				};

				if (!User32NativeMethods.UpdateLayeredWindow(hwnd, screenDc, ref dst, ref size, _memDc, ref src, 0, ref blend, User32NativeMethods.ULW_ALPHA))
				{
					
				}
			}
			finally
			{
				User32NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			RenderAndUpdate();
		}

		private void DestroySurface()
		{
			_bitmap?.Dispose();
			_bitmap = null;

			if (_oldBitmap != IntPtr.Zero)
			{
				User32NativeMethods.SelectObject(_memDc, _oldBitmap);
				_oldBitmap = IntPtr.Zero;
			}

			if (_dibBitmap != IntPtr.Zero)
			{
				User32NativeMethods.DeleteObject(_dibBitmap);
				_dibBitmap = IntPtr.Zero;
			}

			_dibBits = IntPtr.Zero;
		}


		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if (_memDc != IntPtr.Zero &&
				Width > 0 &&
				Height > 0)
			{
				CreateSurface(Width, Height);
			}

			RenderAndUpdate();
		}

		// Because UpdateLayeredWindow bypasses the normal paint pipeline,
		// suppress the default background erase/paint to avoid flicker/artifacts.
		protected override void OnPaintBackground(PaintEventArgs e) { }

		public void RenderAndUpdate()
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(RenderAndUpdate));
				return;
			}

			if (DesignMode || !IsHandleCreated) return;
			if (Width <= 0 || Height <= 0) return;
			if (_bitmap == null) return;


			using (var g = Graphics.FromImage(_bitmap))
			{
				g.Clear(Color.Transparent);
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;             
				RenderOverlay(g);
			}

			UpdateLayered(Handle, Left, Top);
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

					cp.ExStyle |= User32NativeMethods.WS_EX_LAYERED | User32NativeMethods.WS_EX_NOACTIVATE | User32NativeMethods.WS_EX_TOOLWINDOW;
						cp.ExStyle |= User32NativeMethods.WS_EX_TRANSPARENT;
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
			if (!User32NativeMethods.GetCursorPos(out User32NativeMethods.POINT screenPt)) return;

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
			if (disposing)
			{
				DestroySurface();

				_mouseTrackTimer.Tick -= MouseTrackTimer_Tick;
				_mouseTrackTimer.Dispose();
			}

			if (_memDc != IntPtr.Zero)
			{
				User32NativeMethods.DeleteDC(_memDc);
				_memDc = IntPtr.Zero;
			}
			base.Dispose(disposing);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			DestroySurface();

				if (_memDc != IntPtr.Zero)
				{
					User32NativeMethods.DeleteDC(_memDc);
					_memDc = IntPtr.Zero;
				}
				_memDc = User32NativeMethods.CreateCompatibleDC(IntPtr.Zero);
			if (Width > 0 && Height > 0)
			{

				CreateSurface(Width, Height);
			}

			if (EnableMouseTracking) _mouseTrackTimer.Enabled = true;
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			DestroySurface();

			if (_memDc != IntPtr.Zero)
			{
				User32NativeMethods.DeleteDC(_memDc);
				_memDc = IntPtr.Zero;
			}

			base.OnHandleDestroyed(e);
		}

		public void Resize(int width, int height)
		{
			CreateSurface(width, height);
		}


		private void CreateSurface(int width, int height)
		{
			DestroySurface();

			var bmi = new User32NativeMethods.BITMAPINFO();

			bmi.bmiHeader.biSize =
				(uint)Marshal.SizeOf(typeof(User32NativeMethods.BITMAPINFOHEADER));

			bmi.bmiHeader.biWidth = width;

			// top-down bitmap
			bmi.bmiHeader.biHeight = -height;

			bmi.bmiHeader.biPlanes = 1;
			bmi.bmiHeader.biBitCount = 32;
			bmi.bmiHeader.biCompression = User32NativeMethods.BI_RGB;

			_dibBitmap = User32NativeMethods.CreateDIBSection(
				IntPtr.Zero,
				ref bmi,
				User32NativeMethods.DIB_RGB_COLORS,
				out _dibBits,
				IntPtr.Zero,
				0);

			if (_dibBitmap == IntPtr.Zero)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			_oldBitmap = User32NativeMethods.SelectObject(_memDc,_dibBitmap);

			_bitmap = new Bitmap(
				width,
				height,
				width * 4,
				PixelFormat.Format32bppPArgb,
				_dibBits);
		}

		protected virtual void OnOverlayMouseMove(Point clientLocation) { }
		protected virtual void OnOverlayMouseLeave() { }
	}
}
