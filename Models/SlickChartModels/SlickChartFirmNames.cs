using System;

namespace Models.SlickChartModels
{
	[Flags]
	public enum IndexNames
	{
		None = 0b_0000_0000,
		SnP = 0b_0000_0001,
		Nasdaq = 0b_0000_0010,
		Index = 0b_0000_0100,
		Both = SnP | Nasdaq
	}
	public class SlickChartFirmNames
	{
		public string Symbol { get; set; }
		public string Company { get; set; }
		public IndexNames Index { get; set; }
	}
}
