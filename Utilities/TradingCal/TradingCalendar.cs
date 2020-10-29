using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.TradingCal
{
	public class TradingCalendar
	{
		public bool FirstTradingDayOfWeek(DateTime? date)
		{
			GetWeekTradingDays(date,
					  out DateTime dateToUse,
					  out DateTime firstDayOfWeek,
					  out DateTime lastDayOfWeek);
			var tradingDays = GetTradingDays(firstDayOfWeek, lastDayOfWeek);
			return tradingDays.First().Date.Date == dateToUse.Date;
		}

		public bool LastTradingDayOfWeek(DateTime? date)
		{
			GetWeekTradingDays(
				date,
				out DateTime dateToUse,
				out DateTime firstDayOfWeek,
				out DateTime lastDayOfWeek);
			var tradingDays = GetTradingDays(firstDayOfWeek, lastDayOfWeek);
			return tradingDays.Last().Date.Date == dateToUse.Date;
		}

		private static void GetWeekTradingDays(DateTime? date, out DateTime dateToUse, out DateTime firstDayOfWeek, out DateTime lastDayOfWeek)
		{
			if (date == null)
			{
				dateToUse = DateTime.Now;
			}
			else
			{
				dateToUse = (DateTime)date;
			}

			firstDayOfWeek = dateToUse;
			while (firstDayOfWeek.DayOfWeek != DayOfWeek.Monday)
			{
				firstDayOfWeek = firstDayOfWeek.AddDays(-1);
			}
			lastDayOfWeek = dateToUse;
			while (lastDayOfWeek.DayOfWeek != DayOfWeek.Friday)
			{
				lastDayOfWeek = lastDayOfWeek.AddDays(1);
			}
		}

		public DateTime FirstTradingDayOfMonth(int month = 0, int year = 0)
		{
			IEnumerable<TradingDay> tradingDays = TradingDaysOfMonth(ref month, ref year);
			return tradingDays.First().Date;
		}

		public TradingDay GetTradingDay()
		{
			var today = DateTime.UtcNow;
			return GetTradingDay(today);
		}

		public IEnumerable<TradingDay> GetTradingDays(DateTime day1, DateTime day2)
		{
			var startDate = day1 <= day2 ? day1 : day2;
			var endDate = day2 > day1 ? day2 : day1;
			if (endDate.Subtract(startDate).TotalDays > 365)
			{
				throw new ArgumentOutOfRangeException("Timespan more than a year");
			}

			var nonWorkingDaysH = ExchangeCalendar.GetAllNonWorkingDays(startDate.Year);
			if (endDate.Year != startDate.Year)
			{
				nonWorkingDaysH.AddRange(ExchangeCalendar.GetAllNonWorkingDays(endDate.Year));
			}
			nonWorkingDaysH = (from nwd in nonWorkingDaysH
							   where nwd.Date >= startDate
							   && nwd.Date <= endDate
							   select nwd).ToList();
			var nonWorkingDays = nonWorkingDaysH.Select(nwd => nwd.Date);
			var allDaysInSpan = ExchangeCalendar.GetDaysBetween(startDate, endDate);
			allDaysInSpan = allDaysInSpan.Except(nonWorkingDays);
			return (from d in allDaysInSpan
					select new TradingDay
					{
						BusinessDay = true,
						Date = d
					});
		}

		public DateTime LastTradingDayOfMonth(int month = 0, int year = 0)
		{
			IEnumerable<TradingDay> tradingDays = TradingDaysOfMonth(ref month, ref year);
			return tradingDays.Last().Date;
		}

		private TradingDay GetTradingDay(DateTime day)
		{
			return GetTradingDays(day, day).First();
		}

		private IEnumerable<TradingDay> TradingDaysOfMonth(ref int month, ref int year)
		{
			if (month == 0)
			{
				month = DateTime.Now.Month;
			}
			if (year == 0)
			{
				year = DateTime.Now.Year;
			}
			var firstDayOfMonth = new DateTime(year, month, 1);
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
			var tradingDays = GetTradingDays(firstDayOfMonth, lastDayOfMonth);
			return tradingDays;
		}
	}
}