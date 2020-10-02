using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MongoRepository.Repository;
using MongoRepository.Setup;
using System;
using System.Threading.Tasks;
using UpdateIndexFilesInS3.AppProcessing;
using Utilities.AppEnv;

namespace UpdateIndexFilesInS3
{
	internal class Program
	{
		#region Public Methods

		public async static Task Main(string[] args)
		{
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var rscf = serviceProvider.GetService<IReadSCFile>();
			var result = await rscf.ExtractValuesFromMasterSheet();
			if (result)
			{
				result = await rscf.StoreValuesToDb();
			}
			Console.WriteLine(result ? "Success" : "Failed");
			Console.ReadKey();
		}

		#endregion Public Methods


		#region Private Methods

		private static void ConfigureServices(ServiceCollection serviceCollection)
		{
			SetApplicationEnvVar.SetEnvVariablesFromS3();

			var mongoURL = EnvHandler.GetApiKey("InvestDb");
			serviceCollection.AddScoped<IAppRepository<string>>(r => new Repository<string>(mongoURL));

			serviceCollection.AddAutoMapper(typeof(AMProfile));
			MapperConfiguration config = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<AMProfile>();
			});
			config.AssertConfigurationIsValid();
			serviceCollection.AddSingleton<IReadSCFile, ReadSCFile>();
		}

		#endregion Private Methods
	}
}