using MongoDbGenericRepository.Models;

namespace MongoRepository.Model
{
	public interface IAPPDoc : IDocument<string>
	{
		string Symbol { get; set; }
		long ComputeDate { get; set; }
	}
}