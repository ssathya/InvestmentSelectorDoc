using Models.HouseKeeping;
using MongoDbGenericRepository.Attributes;

namespace MongoRepository.Model.HouseKeeping
{
	[CollectionName("RunDateSave")]
	public class RunDateSaveDB : RunDateSave, IAPPDoc
	{
		public string Symbol 
		{ 
			get { return ProcessName; } 
			set { ProcessName = value; } 
		}
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}