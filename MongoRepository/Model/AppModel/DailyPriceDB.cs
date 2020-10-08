using Models.AppModel;
using MongoDbGenericRepository.Attributes;

namespace MongoRepository.Model.AppModel
{
	[CollectionName("TDAPrices")]
	public class DailyPriceDB : DailyPrice, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
