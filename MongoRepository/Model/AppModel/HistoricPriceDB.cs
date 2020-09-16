namespace MongoRepository.Model.AppModel
{
	public class HistoricPriceDB : Models.Index.HistoricPrice, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
