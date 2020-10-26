using MongoRepository.Model;
using System;
using System.Threading.Tasks;

namespace MongoRepository.Setup
{
	public static class DBValuesSetup
	{

		#region Private Fields

		private static long computeDate;
		private static TimeZoneInfo linuxTimeZoneInfo;
		private static TimeZoneInfo windowsTimeZoneInfo;

		#endregion Private Fields

		#region Public Methods

		/// <summary>
		/// Computes the run date.
		/// </summary>
		/// <returns></returns>
		public static DateTimeOffset ComputeRunDate()
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
			//Make it 1 or 2 A.M. at NY based on DST
			nowTimeAtNY = TimeZoneInfo.ConvertTimeToUtc(nowTimeAtNY, est)
				.Date.AddHours(6);
			//nowTimeAtNY = TimeZoneInfo.ConvertTime(nowTimeAtNY, est).Date.AddSeconds(1);
			var nowTimeAtNYOffset = new DateTimeOffset(nowTimeAtNY);
			return nowTimeAtNYOffset;
		}

		/// <summary>
		/// Creates the index.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="appRepository">The application repository.</param>
		public static async Task CreateIndex<T>(Repository.IAppRepository<string> appRepository) where T : IAPPDoc
		{
			var indexCreatedflg = false;
			var indexNames = await appRepository.GetIndexesNamesAsync<T>();
			foreach (var indexName in indexNames)
			{
				if (indexName.Contains("Symbol"))
				{
					indexCreatedflg = true;
					break;
				}
			}
			if (!indexCreatedflg)
			{
				await appRepository.CreateAscendingIndexAsync<T>(x => x.Symbol);
			}
			return;
		}

		/// <summary>
		/// Sets the application document values.
		/// </summary>
		/// <param name="aPPDoc">a pp document.</param>
		/// <returns></returns>
		public static IAPPDoc SetAppDocValues(IAPPDoc aPPDoc)
		{
			aPPDoc.Version++;
			if (computeDate == 0)
			{
				computeDate = ComputeRunDate().ToUnixTimeSeconds();
			}
			aPPDoc.ComputeDate = computeDate;
			return aPPDoc;
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Obtains the time zone information.
		/// </summary>
		/// <param name="timeZoneStr">The time zone string.</param>
		/// <returns></returns>
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

		#endregion Private Methods
	}
}