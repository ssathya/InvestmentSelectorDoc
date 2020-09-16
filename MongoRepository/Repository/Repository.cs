using MongoDbGenericRepository;
using System;

namespace MongoRepository.Repository
{
	public class Repository<TKey> : BaseMongoRepository<TKey>, IAppRepository<TKey> where TKey : IEquatable<TKey>
	{
		public Repository(string connectionString, string databaseName = null) : base(connectionString, databaseName)
		{
		}

		public void DropCollection<TDocument>()
		{
			MongoDbContext.DropCollection<TDocument>();
		}
	}
}