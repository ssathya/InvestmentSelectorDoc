using System;

namespace Utilities.TradingCal
{
	public class TradingDay
	{
		public DateTime Date { get; set; }
		public bool BusinessDay { get; set; }
		public bool PublicHoliday { get; set; }
		public bool Weekend { get; set; }
	}
}
