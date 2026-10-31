using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Extensions
{
	public static class DateTimeExtensions
	{
		public static DateTime TrimSeconds(this DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
		}
		public static DateTime TrimMilliseconds(this DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind);
		}

		public static DateTime TrimToSeconds(this DateTime dt, int seconds)
		{
			if (seconds <= 0)
				throw new ArgumentOutOfRangeException(nameof(seconds));

			long ticks = TimeSpan.FromSeconds(seconds).Ticks;

			return new DateTime(
				dt.Ticks - (dt.Ticks % ticks),
				dt.Kind);
		}
	}
}