using MongoRepository.Model;
using System;

namespace MongoRepository.Setup
{
	public static class DBValuesSetup
	{
		private static TimeZoneInfo linuxTimeZoneInfo;
		private static TimeZoneInfo windowsTimeZoneInfo;

		public static IAPPDoc SetAppDocValues(IAPPDoc aPPDoc)
		{
			aPPDoc.Version++;
			aPPDoc.ComputeDate = ComputeRunDate().ToUnixTimeSeconds();
			return aPPDoc;
		}

		private static DateTimeOffset ComputeRunDate()
		{
			TimeZoneInfo est;
			string timeZoneLinux = "America/New_York";
			string timeZoneWindows = "Eastern Standard Time";
			if (linuxTimeZoneInfo == null && windowsTimeZoneInfo == null)
			{
				linuxTimeZoneInfo ??= ObtainTimeZoneInfo(timeZoneLinux);
				windowsTimeZoneInfo ??= ObtainTimeZoneInfo(timeZoneWindows);
			}
			est = linuxTimeZoneInfo ?? windowsTimeZoneInfo;

			var nowTimeAtNY = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
			nowTimeAtNY = TimeZoneInfo.ConvertTimeToUtc(nowTimeAtNY, est)
				.Date.AddSeconds(1);
			//nowTimeAtNY = TimeZoneInfo.ConvertTime(nowTimeAtNY, est).Date.AddSeconds(1);
			var nowTimeAtNYOffset = new DateTimeOffset(nowTimeAtNY);
			return nowTimeAtNYOffset;
		}

		private static TimeZoneInfo ObtainTimeZoneInfo(string timeZoneStr)
		{
			try
			{
				var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneStr);
				return tzi;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}