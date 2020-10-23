using Amazon.Runtime.Internal.Util;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.AppModel;
using Models.SlickChartModels;
using MongoRepository.Model.HouseKeeping;
using MongoRepository.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities.AppEnv;

namespace DailyPriceFetch.Process
{
    public class EvaluateSecurity
    {
		private readonly ILogger<EvaluateSecurity> logger;
		private readonly IMapper mapper;
		private readonly IAppRepository<string> appRepository;
		private readonly HttpClient client;
		private readonly string IsrKey;
		private List<string> securityList;
		private const string SecAnalKey = "Security Analysis";
		private const int HoursToWait = 23;
		private readonly string indexListAPI = @"https://bq0hiv4olf.execute-api.us-east-1.amazonaws.com/Prod/api/SecurityList/DONTCARE/{IndexId}";

		public EvaluateSecurity(
			ILogger<EvaluateSecurity> logger,
			IMapper mapper,
			IAppRepository<string> appRepository,
			IHttpClientFactory clientFactory)
		{
			this.logger = logger;
			this.mapper = mapper;
			this.appRepository = appRepository;
			
			//HttpClient
			client = clientFactory.CreateClient();

			//API Gateway key
			IsrKey = EnvHandler.GetApiKey(@"ISRApiHandler");
		}
		public async Task<bool> ComputeSecurityAnalysis()
		{
			if (await CheckRunDateAsync() == false)
			{
				return true;
			}
			await PopulateSecurityListAsync();
		}

		private async Task PopulateSecurityListAsync()
		{
			var queryIndexList = indexListAPI.Replace("@{IndexId}", Convert.ToInt32(IndexNames.SnP).ToString());
			
			var indexComponents = await PullSecureityListAsync(queryIndexList);
			securityList.AddRange(indexComponents);
			queryIndexList = indexListAPI.Replace("@{IndexId}", Convert.ToInt32(IndexNames.Nasdaq).ToString());
			indexComponents = await PullSecureityListAsync(queryIndexList);
			securityList.AddRange(indexComponents);
			securityList = securityList.Distinct().ToList();
		}

		private async Task<List<string>> PullSecureityListAsync(string queryIndexList)
		{
			var drh = client.DefaultRequestHeaders;
			if (!drh.Contains(@"x-api-key"))
			{
				client.DefaultRequestHeaders.Add(@"x-api-key", IsrKey);
			}
			var response = await client.GetAsync(queryIndexList);
			if (!response.IsSuccessStatusCode)
			{
				logger.LogError($"Error while getting security list.");
				return new List<string>();
			}
			string apiResponse = await response.Content.ReadAsStringAsync();
			List<string> indexComponents = JsonConvert.DeserializeObject<List<SlickChartFirmNames>>(apiResponse).Select(r => r.Symbol).ToList();
			return indexComponents;
		}

		private async Task<bool> CheckRunDateAsync()
		{
			var lastUpdateTime = (await appRepository.GetAllAsync<RunDateSaveDB>(r => r.Symbol.Equals(SecAnalKey))).FirstOrDefault();
			if (lastUpdateTime != null)
			{
				logger.LogDebug("Found last run record");
				var lastRunTime = DateTimeOffset.FromUnixTimeSeconds(lastUpdateTime.LastRunTime).DateTime;
				//If we ran this application within the past 23 - 24 hours don't run it again.
				if ((DateTime.Now.ToUniversalTime() - lastRunTime).TotalHours <= HoursToWait)
				{
					return false;
				}
			}
			return true;
		}
	}
}
