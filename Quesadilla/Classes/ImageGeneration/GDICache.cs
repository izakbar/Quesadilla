using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Quesadilla.Classes.ImageGeneration
{
	public static class GDICache
	{
		private static readonly ConcurrentDictionary<PenCacheKey, Pen> _penCache = new ConcurrentDictionary<PenCacheKey, Pen>();
		private static readonly ConcurrentDictionary<int, SolidBrush> _brushCache = new ConcurrentDictionary<int, SolidBrush>();
		private static readonly ConcurrentDictionary<BrushCacheKey, HatchBrush> _hatchBrushCache = new ConcurrentDictionary<BrushCacheKey, HatchBrush>();

		public static void Clear()
		{
			foreach (var pen in _penCache.Values)
				pen.Dispose();

			foreach (var brush in _brushCache.Values)
				brush.Dispose();

			foreach (var brush in _hatchBrushCache.Values)
				brush.Dispose();

			_penCache.Clear();
			_brushCache.Clear();
			_hatchBrushCache.Clear();
		}

		public static SolidBrush GetBrush(Color color)
		{
			int key = color.ToArgb();

			return _brushCache.GetOrAdd(
				key,
				_ => new SolidBrush(color));
		}
		public static HatchBrush GetHatchBrush(
			HatchStyle hatchStyle,
			Color foreColor,
			Color backColor)
		{
			var key = new BrushCacheKey(
				hatchStyle,
				foreColor,
				backColor);

			return _hatchBrushCache.GetOrAdd(
				key,
				_ => new HatchBrush(
					hatchStyle,
					foreColor,
					backColor));
		}
		public static HatchBrush GetHatchBrush(HatchStyle hatchStyle, Color foreColor)
		{
			return GetHatchBrush(
				hatchStyle,
				foreColor,
				Color.Black);
		}
		public static Pen GetPen(Color color, float width, DashStyle dashStyle)
		{
			var key = new PenCacheKey(
				color,
				width,
				dashStyle);

			return _penCache.GetOrAdd(key, _ =>
			{
				return new Pen(color, width)
				{
					DashStyle = dashStyle
				};
			});
		}
		public static Pen GetPen(
			Color color,
			float width
			)
		{
			return GetPen(color, width, DashStyle.Solid);
		}
	}

	public readonly struct PenCacheKey : IEquatable<PenCacheKey>
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
	public readonly struct BrushCacheKey : IEquatable<BrushCacheKey>
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
