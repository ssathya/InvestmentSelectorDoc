using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.AppModel;
using MongoRepository.Model.AppModel;
using MongoRepository.Repository;
using MongoRepository.Setup;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Utilities.AppEnv;
using Utilities.StringHelpers;

namespace DailyPriceFetch.Process
{
	public class SecurityPriceSave : ISecurityPriceSave
	{

		private const string QueueName = "PricingList";
		private readonly string apiKey;
		private readonly string IsrKey;
		private readonly IAppRepository<string> appRepository;
		private readonly HttpClient client;
		private readonly ILogger<SecurityPriceSave> logger;
		private readonly IMapper mapper;
		private readonly GetQueueUrlResponse queueUrlResponse;
		private readonly string quotesAPI = @"https://api.tdameritrade.com/v1/marketdata/quotes?apikey={apiKey}&symbol={tickersToUse}";
		private readonly string historicPriceAPI = @" https://bq0hiv4olf.execute-api.us-east-1.amazonaws.com/Prod/api/HistoricPrice/{tickersToUse}";
		private  List<string> securityList;
		private readonly IAmazonSQS sqs;

		public SecurityPriceSave(ILogger<SecurityPriceSave> logger,
					IMapper mapper,
					IAppRepository<string> appRepository,
					IHttpClientFactory clientFactory)
		{
			this.logger = logger;
			this.mapper = mapper;
			this.appRepository = appRepository;

			//SQS
			sqs = new AmazonSQSClient(RegionEndpoint.USEast1);
			queueUrlResponse = sqs.GetQueueUrlAsync(QueueName).Result;

			//HttpClient
			client = clientFactory.CreateClient();

			//TDA
			apiKey = EnvHandler.GetApiKey(@"tdameritrade");

			//API Gateway key
			IsrKey = EnvHandler.GetApiKey(@"ISRApiHandler");

			//Security list
			securityList = GetTickerList().Result;
			if (securityList == null || securityList.Count == 0)
			{
				securityList = new List<string>
				{
					"MSFT",
					"AAPL",
					"FDX",
					"UPS",
					"CRM"
				};				
			}
		}

		public async Task<bool> ComputeSecurityAnalysis()
		{
			if (securityList == null || securityList.Count == 0)
			{				
				return false;
			}
			//var queryTickers = string.Join(',', securityList);

			var historicPrices = await appRepository.GetAllAsync<HistoricPriceDB>(r => securityList.Contains(r.Symbol));
			foreach (var historicPrice in historicPrices)
			{
				historicPrice.ComputeDate = historicPrice.ComputeDate >= 999999999999 ?
					historicPrice.ComputeDate /= 1000 : historicPrice.ComputeDate;
			}
			if (historicPrices.Count != securityList.Count)
			{
				var missingTickers = string.Join(',',securityList.Except(historicPrices.Select(r => r.Symbol)));
				logger.LogInformation("Historic price and security list are not in sync");
				logger.LogInformation(missingTickers);
			}
			foreach (var security in securityList)
			{
				HistoricPrice historicPrice = await ObtainHistoricPrice(security);
				SecurityAnalysis securityAnalysis = ComputeValues(historicPrice);
			}
			return true;
			throw new NotImplementedException();
		}

		private SecurityAnalysis ComputeValues(HistoricPrice historicPrice)
		{

			throw new NotImplementedException();
		}

		private async Task<HistoricPrice> ObtainHistoricPrice(string security)
		{
			var queryHistoricPrice = historicPriceAPI.Replace(@"{tickersToUse}", security);
			var drh = client.DefaultRequestHeaders;
			if (!drh.Contains(@"x-api-key"))
			{
				client.DefaultRequestHeaders.Add(@"x-api-key", IsrKey);
			}			
			try
			{
				var response = await client.GetAsync(queryHistoricPrice);
				if (!response.IsSuccessStatusCode)
				{
					logger.LogError($"Error while getting historic price; \n\tError code: {response.StatusCode}");
					logger.LogError($"{response.ReasonPhrase}");
					return null;
				}
				string apiResponse = await response.Content.ReadAsStringAsync();
				var historicPrices = JsonConvert.DeserializeObject<List<HistoricPrice>>(apiResponse);
				return historicPrices.FirstOrDefault();
			}
			catch (Exception ex)
			{
				logger.LogError($"Excepting getting/processing historic price\n\t{ex.Message}");
				return null;
			}		
		}

		public async Task<bool> GetPricingData()
		{
			try
			{
				
				if (securityList == null || securityList.Count == 0)
				{
					return false;
				}
				var queryTickers = string.Join(',', securityList);
				string urlStr = quotesAPI.Replace(@"{tickersToUse}", queryTickers)
											.Replace(@"{apiKey}", apiKey);
				string tdaResponse = await client.GetStringAsync(urlStr);
				if (tdaResponse.IsNullOrWhiteSpace())
				{
					return false;
				}
				//logger.LogDebug(tdaResponse);
				List<DailyPriceDB> currentPricesDB = ConvertPriceJsonToObjects(tdaResponse);
				var listOfSymbols = from a in currentPricesDB
									select a.Symbol;				

				var recordsDeleted = await appRepository.DeleteManyAsync<DailyPriceDB>(r => listOfSymbols.Contains(r.Symbol));
				await appRepository.AddManyAsync(currentPricesDB);
				await DBValuesSetup.CreateIndex<DailyPriceDB>(appRepository);
				return true;
			}
			catch (Exception ex)
			{
				logger.LogError("Issues getting data from TDA");
				logger.LogError($"Error details \n\t{ex.Message}");
				return false;
			}
		}

		private List<DailyPriceDB> ConvertPriceJsonToObjects(string tdaResponse)
		{
			var jobjs = JObject.Parse(tdaResponse).SelectTokens(@"$.*");
			var currentPricesDB = new List<DailyPriceDB>();
			foreach (var dpDB in from jobj in jobjs
								 let dpDB = mapper.Map<DailyPriceDB>(jobj.ToObject<DailyPrice>())
								 select dpDB)
			{
				var saveObj = (DailyPriceDB)DBValuesSetup.SetAppDocValues(dpDB);
				currentPricesDB.Add(saveObj);
			}

			return currentPricesDB;
		}

		private async Task DeleteExtractedQueue(ReceiveMessageResponse receiveMessageResponse)
		{
			var receiptHandler = receiveMessageResponse.Messages.First().ReceiptHandle;
			_ = await sqs.DeleteMessageAsync(new DeleteMessageRequest
			{
				QueueUrl = queueUrlResponse.QueueUrl,
				ReceiptHandle = receiptHandler
			});
		}

		private string ExtractTickersFromQueue(ReceiveMessageResponse receiveMessageResponse)
		{
			var tickerJson = receiveMessageResponse.Messages.FirstOrDefault()?.Body;
			return tickerJson;
		}

		private async Task<List<string>> GetTickerList()
		{
			List<string> returnValues = null;

			try
			{
				if (queueUrlResponse.HttpStatusCode != HttpStatusCode.OK)
				{
					logger.LogError("Could not open Queue");
					return returnValues;
				}
				var receiveMessageRequest = new ReceiveMessageRequest
				{
					QueueUrl = queueUrlResponse.QueueUrl
				};
				var receiveMessageResponse = await sqs.ReceiveMessageAsync(receiveMessageRequest);
				if (receiveMessageResponse.Messages.Count == 0)
				{
					logger.LogInformation("No message to process");
					return returnValues;
				}
				string tickerJson = ExtractTickersFromQueue(receiveMessageResponse);
				if (tickerJson.IsNullOrWhiteSpace())
				{
					logger.LogInformation("Message body happens to be empty");
					return returnValues;
				}
				await DeleteExtractedQueue(receiveMessageResponse);
				returnValues = System.Text.Json.JsonSerializer.Deserialize<List<string>>(tickerJson);
			}
			catch (Exception ex)
			{
				logger.LogError("Error while extracting values from SQS");
				logger.LogError($"Error details\n\t{ex.Message}");
			}
			return returnValues;
		}

	}
}