using Amazon.Lambda.Core;
using AutoMapper;
using DailyPriceFetch.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoRepository.Repository;
using MongoRepository.Setup;
using NLog.Extensions.Logging;
using System;
using System.Linq;
using Utilities.AppEnv;
using Utilities.TradingCal;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DailyPriceFetch
{
	public class Function
	{

		/// <summary>
		/// A simple function that takes a string and does a ToUpper
		/// </summary>
		/// <param name="input"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		//public string FunctionHandler(string input, ILambdaContext context)
		public string FunctionHandler()
		{
			var nonWorkingDatys = ExchangeCalendar.GetAllNonWorkingDays(DateTime.Now.Year);
			if (nonWorkingDatys.FirstOrDefault(x => x.Date == DateTime.Now.Date) != null)
			{
				return $"Today is not a working day {DateTime.Now.Date}";
			}

			//House keeping
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			var serviceProvider = serviceCollection.BuildServiceProvider();

			//Populate SQS if needed.
			var popSQS = serviceProvider.GetService<IPopulateSQS>();
			//var result = popSQS.PopulateSQSQueue(90).Result;
			bool result = true;

			//Update prices
			var sps = serviceProvider.GetService<ISecurityPriceSave>();
			//var result1 = sps.GetPricingData().Result;
			var result1 = sps.ComputeSecurityAnalysis().Result;

			return (result ? "Everything ran well" : "Something is wrong today");
		}

		private void ConfigureServices(IServiceCollection serviceCollection)
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
			serviceCollection.AddLogging(c => c.AddNLog())
				.Configure<LoggerFilterOptions>(o => o.MinLevel = LogLevel.Debug)
				.AddTransient(typeof(ILogger<>), typeof(Logger<>));
			serviceCollection.AddScoped<IPopulateSQS, PopulateSQS>();
			serviceCollection.AddScoped<ISecurityPriceSave, SecurityPriceSave>();
			serviceCollection.AddHttpClient();
		}
	}
}
