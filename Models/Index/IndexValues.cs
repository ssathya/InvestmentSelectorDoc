namespace Models.Index
{
	public class IndexValues
	{
		public string Symbol { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public decimal Change { get; set; }
		public decimal Changepct { get; set; }
		public decimal Avg200MA { get; set; }
		public decimal Avg50MA { get; set; }
		public decimal Pct50ovr200 { get; set; }
	}
}