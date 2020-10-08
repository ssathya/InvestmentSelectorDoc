using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MongoRepository.Repository;
using MongoRepository.Setup;
using Utilities.AppEnv;

namespace ISRApiHandler
{
	public static class StartupService
	{
		public static IServiceCollection AddSwagger(this IServiceCollection services)
		{
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "DB Rest Actions", Version = "v1" });
			});
			return services;
		}
		public static IServiceCollection AddMongoRepository(this IServiceCollection services)
		{
			var mongoURL = EnvHandler.GetApiKey("InvestDb");

			services.AddSingleton<IAppRepository<string>>(r => new Repository<string>(mongoURL));
			return services;
		}
		internal static void SetupAutoMapper(this IServiceCollection services)
		{
			services.AddAutoMapper(typeof(Startup));
			MapperConfiguration config = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<AMProfile>();
			});
			config.AssertConfigurationIsValid();
		}
	}
}
