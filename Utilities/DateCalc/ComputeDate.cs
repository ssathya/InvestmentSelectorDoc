using System;

namespace Utilities.DateCalc
{
	public static class ComputeDate
	{
		public static long ComputeDateCalc()
		{
			var todayOffset = new DateTimeOffset(DateTime.UtcNow.Date);
			var todayInLong = todayOffset.ToUnixTimeSeconds();
			return todayInLong;
		}

		public static DateTimeOffset ConvertToDateTime(long utcTime)
		{
			if (utcTime < 999999999999) //before Sun Sept 09 2001 01:46:39 UTC
			{
				return DateTimeOffset.MinValue;
			}
			if (utcTime > 9999999999999)
			{
				return DateTimeOffset.FromUnixTimeMilliseconds(utcTime);
			}
			return DateTimeOffset.FromUnixTimeSeconds(utcTime);
		}
	}
}