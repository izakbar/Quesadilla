using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.InterOp
{
	public static class User32NativeMethods
	{
		public const int ULW_ALPHA = 0x02;
		public const byte AC_SRC_OVER = 0x00;
		public const byte AC_SRC_ALPHA = 0x01;

		public const int WS_EX_LAYERED = 0x80000;
		public const int WS_EX_TRANSPARENT = 0x20;
		public const int WS_EX_NOACTIVATE = 0x08000000;
		public const int WS_EX_TOOLWINDOW = 0x00000080;

		public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();


		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool UpdateLayeredWindow(
			IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
			IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		public static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hbitmap);

		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out POINT lpPoint);

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT { public int X, Y; public POINT(int x, int y) { X = x; Y = y; } }

		[StructLayout(LayoutKind.Sequential)]
		public struct SIZE { public int cx, cy; public SIZE(int cx, int cy) { this.cx = cx; this.cy = cy; } }


		[StructLayout(LayoutKind.Sequential)]
		public struct BLENDFUNCTION
		{
			public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFOHEADER
		{
			public uint biSize;
			public int biWidth;
			public int biHeight;
			public short biPlanes;
			public short biBitCount;
			public uint biCompression;
			public uint biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;
		}

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

		public const uint BI_RGB = 0;
		public const uint DIB_RGB_COLORS = 0;

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFO
		{
			public BITMAPINFOHEADER bmiHeader;
		}

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();
		public static IntPtr GetForegroundWindowHandle()
		{
			return GetForegroundWindow();
		}


		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
		public static bool SetForegroundWindowHandle(IntPtr hWnd)
		{
			return SetForegroundWindow(hWnd);
		}
		public struct Rect
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);
		[DllImport("user32.dll")]
		public static extern bool GetClientRect(IntPtr hwnd, ref Rect rectangle);
		[DllImport("user32.dll")]
		public static extern bool ClientToScreen(IntPtr hwnd, ref Point point);
		public static bool GetWindowDimensions(IntPtr hwnd, ref Rect rectangle)
		{
			var result = GetWindowRect(hwnd, ref rectangle);
			return result;
		}
		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
		public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

		public const UInt32 SWP_NOSIZE = 0x0001;
		public  const UInt32 SWP_NOMOVE = 0x0002;
		public const UInt32 SWP_NOACTIVATE = 0x0010;

		public static void SendWindowBack(IntPtr w)
		{
			//var hWnd = new WindowInteropHelper(window).Handle;
			SetWindowPos(w, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
		}
		public static void SendWindowTop(IntPtr w)
		{
			//var hWnd = new WindowInteropHelper(window).Handle;
			SetWindowPos(w, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
		}


		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern uint RegisterWindowMessage(string message);
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);


	}
}
