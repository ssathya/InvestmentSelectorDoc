namespace Models.Index
{
	public class DailyPrice
	{
		public string AssetType { get; set; }
		public string AssetMainType { get; set; }
		public string Cusip { get; set; }
		public string AssetSubType { get; set; }
		public string Symbol { get; set; }
		public string Description { get; set; }
		public decimal BidPrice { get; set; }
		public int BidSize { get; set; }
		public string BidId { get; set; }
		public decimal AskPrice { get; set; }
		public int AskSize { get; set; }
		public string AskId { get; set; }
		public decimal LastPrice { get; set; }
		public int LastSize { get; set; }
		public string LastId { get; set; }
		public decimal OpenPrice { get; set; }
		public decimal HighPrice { get; set; }
		public decimal LowPrice { get; set; }
		public string BidTick { get; set; }
		public decimal ClosePrice { get; set; }
		public decimal NetChange { get; set; }
		public int TotalVolume { get; set; }
		public long QuoteTimeInLong { get; set; }
		public long TradeTimeInLong { get; set; }
		public decimal Mark { get; set; }
		public string Exchange { get; set; }
		public string ExchangeName { get; set; }
		public bool Marginable { get; set; }
		public bool Shortable { get; set; }
		public decimal Volatility { get; set; }
		public int Digits { get; set; }
#pragma warning disable IDE1006 // Naming Styles
		public decimal _52WkHigh { get; set; }
		public decimal _52WkLow { get; set; }
#pragma warning restore IDE1006 // Naming Styles
		public int NAV { get; set; }
		public decimal PeRatio { get; set; }
		public decimal DivAmount { get; set; }
		public decimal DivYield { get; set; }
		public string DivDate { get; set; }
		public string SecurityStatus { get; set; }
		public decimal RegularMarketLastPrice { get; set; }
		public int RegularMarketLastSize { get; set; }
		public decimal RegularMarketNetChange { get; set; }
		public long RegularMarketTradeTimeInLong { get; set; }
		public decimal NetPercentChangeInDouble { get; set; }
		public decimal MarkChangeInDouble { get; set; }
		public decimal MarkPercentChangeInDouble { get; set; }
		public decimal RegularMarketPercentChangeInDouble { get; set; }
		public bool Delayed { get; set; }
	}
}