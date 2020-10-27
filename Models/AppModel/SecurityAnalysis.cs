namespace Models.AppModel
{
	public class SecurityAnalysis
	{

		#region Public Properties

		public double DollarVolumeAverage { get; set; }
		public double EfficiencyRatio { get; set; }
		public double Momentum { get; set; }		
		public int PietroskiFScore { get; set; }
		public string Symbol { get; set; }
		public double ThirtyDayVolatility { get; set; }
		public double VolatilityInverse { get; set; }

		#endregion Public Properties
	}
}