namespace Models.Index
{
	public class HistoricPrice
	{
		public Candle[] Candles { get; set; }
		public string Symbol { get; set; }
		public bool Empty { get; set; }
	}
	public class Candle
	{
		public decimal Open { get; set; }
		public decimal High { get; set; }
		public decimal Low { get; set; }
		public decimal Close { get; set; }
		public int Volume { get; set; }
		public long Datetime { get; set; }
	}
}