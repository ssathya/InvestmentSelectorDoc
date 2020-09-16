namespace Models.Index
{
	public class SecurityAnalysis
	{
		public string Symbol { get; set; }
		public double Momentum { get; set; }
		public double EfficiencyRatio { get; set; }
		public double ThirtyDayVolatility { get; set; }
		public double VolatilityInverse { get; set; }
		public string PietroskiFScore { get; set; }
	}
}