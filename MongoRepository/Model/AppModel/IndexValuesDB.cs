namespace MongoRepository.Model.AppModel
{
	public class IndexValuesDB : Models.Index.IndexValues, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
