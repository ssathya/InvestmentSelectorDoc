namespace MongoRepository.Model.AppModel
{
	public class IndexComponentsDB : Models.Index.IndexComponent, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
