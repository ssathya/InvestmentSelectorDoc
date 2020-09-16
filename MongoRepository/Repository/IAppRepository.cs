using MongoDbGenericRepository;
using System;

namespace MongoRepository.Repository
{
	public interface IAppRepository<TKey> : IBaseMongoRepository<TKey> where TKey : IEquatable<TKey>
	{
		void DropCollection<TDocument>();
	}
}