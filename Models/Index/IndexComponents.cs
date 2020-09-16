namespace Models.Index
{
	public class IndexComponent
	{
		public string Symbol { get; set; }
		public string Name { get; set; }
		public double Price { get; set; }
		public double Change { get; set; }
		public int Volume { get; set; }
		public int AverageVolume { get; set; }
		public double MarketCap { get; set; }
		public double DollarVolume { get; set; }
		public double Changepct { get; set; }
		public double Eps { get; set; }
	}
}