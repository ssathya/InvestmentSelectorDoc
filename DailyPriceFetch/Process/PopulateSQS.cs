using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.HouseKeeping;
using Models.SlickChartModels;
using MongoRepository.Model.AppModel;
using MongoRepository.Model.HouseKeeping;
using MongoRepository.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DailyPriceFetch.Process
{
	public class PopulateSQS : IPopulateSQS
	{
		private const string PricingDataKey = "Pricing Data";
		private readonly ILogger<PopulateSQS> logger;
		private readonly IMapper mapper;
		private readonly IAppRepository<string> appRepository;
		private List<SlickChartFirmNames> slickChartFirmNames;

		public PopulateSQS(ILogger<PopulateSQS> logger,
					 IMapper mapper,
					 IAppRepository<string> appRepository)
		{
			this.logger = logger;
			this.mapper = mapper;
			this.appRepository = appRepository;
		}
		public async Task<bool> PopulateSQSQueue(int chunckSize = 75)
		{
			var lastUpdateTime = (await appRepository.GetAllAsync<RunDateSaveDB>(r => r.Symbol.Equals(PricingDataKey))).FirstOrDefault();
			if (lastUpdateTime != null)
			{
				logger.LogDebug("Found last run record");
				var lastRunTime = DateTimeOffset.FromUnixTimeSeconds(lastUpdateTime.LastRunTime).DateTime;
				if ((DateTime.Now.ToUniversalTime() - lastRunTime).TotalHours <= 3)
				{
					return true;
				}
			}
			await SaveCurrentRunDate();

			//The ComputeDate >= 0 has no meaning. 
			//API needs a filter so giving a bogus filter.
			var symbolCollection = await appRepository.GetAllAsync<SlickChartFirmNamesDB>(r => r.ComputeDate >= 0);
			if (symbolCollection == null || symbolCollection.Count == 0)
			{
				return false;
			}
			slickChartFirmNames = mapper.Map<List<SlickChartFirmNames>>(symbolCollection);
			var symbols = slickChartFirmNames.Select(r => r.Symbol).ToList();
			return true;
		}

		private async Task SaveCurrentRunDate()
		{
			await appRepository.DeleteOneAsync<RunDateSaveDB>(r => r.Symbol.Equals(PricingDataKey));
			var currentRunDateSave = new RunDateSave
			{
				ProcessName = PricingDataKey,
				LastRunTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};
			var currentRunDateSaveDb = mapper.Map<RunDateSaveDB>(currentRunDateSave);
			await appRepository.AddOneAsync<RunDateSaveDB>(currentRunDateSaveDb);
		}
	}
}
