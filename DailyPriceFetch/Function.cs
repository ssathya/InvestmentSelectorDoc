using Amazon.Lambda.Core;
using AutoMapper;
using CommonUtils.Process;
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

			try
			{
				//Pricing
				Console.WriteLine("Working on obtaining price");
				bool result = true;
				result = GetPricingData(serviceProvider);
				Console.WriteLine($"Pricing information returned {result}");

				//Evaluate Securities
				Console.WriteLine("Working on evaluating securities");
				var evalSec = serviceProvider.GetService<IEvaluateSecurity>();
				var result1 = evalSec.ComputeSecurityAnalysis().Result;
				Console.WriteLine($"Security evaluation returned {result1}");
				result = result && result1;
				return (result ? "Everything ran well" : "Something is wrong today");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unhanded exception\n\t{ex.Message}");
				return "Processing failed";
			}
		}

		private static bool GetPricingData(ServiceProvider serviceProvider)
		{
			//Populate SQS if needed.
			var popSQS = serviceProvider.GetService<IPopulateSQS>();
			popSQS.ProcessName = "Pricing Data";
			popSQS.QueueName = "PricingList";

			var result = popSQS.PopulateSQSQueue(90).Result;


			//Update prices
			var sps = serviceProvider.GetService<ISecurityPriceSave>();


			var result1 = sps.GetPricingData().Result;
			return result && result1;
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
			serviceCollection.AddScoped<IEvaluateSecurity, EvaluateSecurity>();
			serviceCollection.AddHttpClient();
		}
	}
}
