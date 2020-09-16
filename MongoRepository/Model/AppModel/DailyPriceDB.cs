namespace MongoRepository.Model.AppModel
{
	public class DailyPriceDB : Models.Index.DailyPrice, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
