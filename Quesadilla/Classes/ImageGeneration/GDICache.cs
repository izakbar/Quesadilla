using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Quesadilla.Classes.ImageGeneration
{
	public class GDICache
	{
		private static readonly Dictionary<PenCacheKey, Pen> _penCache = new Dictionary<PenCacheKey, Pen>();
		private static readonly Dictionary<int, Brush> BrushCache = new Dictionary<int, Brush>();
		private static readonly Dictionary<BrushCacheKey, HatchBrush> HatchBrushCache = new Dictionary<BrushCacheKey, HatchBrush>();

		public static Brush GetBrush(Color color)
		{
			int key = color.ToArgb();

			if (!BrushCache.TryGetValue(key, out Brush brush))
			{
				brush = new SolidBrush(color);
				BrushCache[key] = brush;
			}

			return brush;
		}

		public static HatchBrush GetHatchBrush(HatchStyle hatchStyle,Color foreColor,Color backColor)
		{
			var key = new BrushCacheKey(
				hatchStyle,
				foreColor,
				backColor);

			if (!HatchBrushCache.TryGetValue(key, out HatchBrush brush))
			{
				brush = new HatchBrush(
					hatchStyle,
					foreColor,
					backColor);

				HatchBrushCache[key] = brush;
			}

			return brush;
		}
		public static HatchBrush GetHatchBrush(HatchStyle hatchStyle,Color foreColor)
		{
			return GetHatchBrush(
				hatchStyle,
				foreColor,
				Color.Black);
		}

		public static Pen GetPen(
			Color color,
			float width,
			DashStyle dashStyle)
		{
			var key = new PenCacheKey(
				color,
				width,
				dashStyle);

			if (!_penCache.TryGetValue(key, out Pen pen))
			{
				pen = new Pen(color, width);
				pen.DashStyle = dashStyle;

				_penCache[key] = pen;
			}

			return pen;
		}
		public static Pen GetPen(
			Color color,
			float width
			)
		{
			return GetPen(color, width, DashStyle.Solid);
		}

		}

		public struct PenCacheKey : IEquatable<PenCacheKey>
	{
		public readonly int Argb;
		public readonly int WidthX10;
		public readonly DashStyle DashStyle;

		public PenCacheKey(Color color, float width, DashStyle dashStyle)
		{
			Argb = color.ToArgb();
			WidthX10 = (int)Math.Round(width * 10);
			DashStyle = dashStyle;
		}

		public bool Equals(PenCacheKey other)
		{
			return Argb == other.Argb
				&& WidthX10 == other.WidthX10
				&& DashStyle == other.DashStyle;
		}

		public override bool Equals(object obj)
		{
			return obj is PenCacheKey
				&& Equals((PenCacheKey)obj);
		}
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = Argb;
				hash = (hash * 397) ^ WidthX10;
				hash = (hash * 397) ^ (int)DashStyle;
				return hash;
			}
		}
	}
	public struct BrushCacheKey : IEquatable<BrushCacheKey>
	{
		public readonly HatchStyle HatchStyle;
		public readonly int ForeColor;
		public readonly int BackColor;

		public BrushCacheKey(
			HatchStyle hatchStyle,
			Color foreColor,
			Color backColor)
		{
			HatchStyle = hatchStyle;
			ForeColor = foreColor.ToArgb();
			BackColor = backColor.ToArgb();
		}

		public bool Equals(BrushCacheKey other)
		{
			return HatchStyle == other.HatchStyle &&
				   ForeColor == other.ForeColor &&
				   BackColor == other.BackColor;
		}

		public override bool Equals(object obj)
		{
			return obj is BrushCacheKey &&
				   Equals((BrushCacheKey)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (int)HatchStyle;
				hash = (hash * 397) ^ ForeColor;
				hash = (hash * 397) ^ BackColor;
				return hash;
			}
		}
	}
}
