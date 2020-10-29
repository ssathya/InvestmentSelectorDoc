using Models.SlickChartModels;
using MongoDbGenericRepository.Attributes;

namespace MongoRepository.Model.AppModel
{
	[CollectionName("SlicChartFirmNames")]
	public class SlickChartFirmNamesDB : SlickChartFirmNames, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
