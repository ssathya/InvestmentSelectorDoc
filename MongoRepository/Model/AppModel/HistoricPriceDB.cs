using Models.AppModel;
using MongoDbGenericRepository.Attributes;

namespace MongoRepository.Model.AppModel
{
	[CollectionName("TDHistPriceDb")]
	public class HistoricPriceDB : HistoricPrice, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
