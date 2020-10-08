﻿using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.AppModel;
using MongoRepository.Model.AppModel;
using MongoRepository.Repository;
using MongoRepository.Setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
		private readonly IAppRepository<string> appRepository;
		private readonly HttpClient client;
		private readonly ILogger<SecurityPriceSave> logger;
		private readonly IMapper mapper;
		private readonly GetQueueUrlResponse queueUrlResponse;
		private readonly string quotesAPI = @"https://api.tdameritrade.com/v1/marketdata/quotes?apikey={apiKey}&symbol={tickersToUse}";
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
		}

		public async Task<bool> GetPricingData()
		{
			List<string> securityList = await GetTickerList();
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

			await appRepository.DeleteManyAsync<DailyPriceDB>(r => r.ComputeDate != currentPricesDB[0].ComputeDate);
			await appRepository.AddManyAsync(currentPricesDB);
			await DBValuesSetup.CreateIndex<DailyPriceDB>(appRepository);
			return true;
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
			returnValues = JsonSerializer.Deserialize<List<string>>(tickerJson);
			return returnValues;
		}

	}
}