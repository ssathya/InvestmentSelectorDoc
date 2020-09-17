using Models.AppModel;

namespace MongoRepository.Model.AppModel
{
	public class IndexValuesDB : IndexValues, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
