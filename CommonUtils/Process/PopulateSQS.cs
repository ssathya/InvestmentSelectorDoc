using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.HouseKeeping;
using Models.SlickChartModels;
using MongoRepository.Model.AppModel;
using MongoRepository.Model.HouseKeeping;
using MongoRepository.Repository;
using MongoRepository.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Utilities.ListHelpers;
using Utilities.StringHelpers;

namespace CommonUtils.Process
{
	public class PopulateSQS : IPopulateSQS
	{
		#region Private Fields

		private readonly IAppRepository<string> appRepository;
		private readonly ILogger<PopulateSQS> logger;
		private readonly IMapper mapper;
		private readonly IAmazonSQS sqs;
		private GetQueueUrlResponse queueUrlResponse;
		private List<SlickChartFirmNames> slickChartFirmNames;

		#endregion Private Fields

		#region Public Properties

		public string ProcessName { get; set; }
		public string QueueName { get; set; }

		#endregion Public Properties

		#region Public Constructors

		public PopulateSQS(ILogger<PopulateSQS> logger,
					 IMapper mapper,
					 IAppRepository<string> appRepository)
		{
			this.logger = logger;
			this.mapper = mapper;
			this.appRepository = appRepository;

			//SQS
			sqs = new AmazonSQSClient(RegionEndpoint.USEast1);
		}

		#endregion Public Constructors

		#region Public Methods

		public async Task<bool> PopulateSQSQueue(int chunckSize = 75)
		{
			if (ProcessName.IsNullOrWhiteSpace() || QueueName.IsNullOrWhiteSpace())
			{
				logger.LogCritical("Required values Process Name and/or Queue Name need to be set before calling PopulateSQS Queue");
				throw new Exception("Either Process name or Queue Name is not set");
			}
			queueUrlResponse = await sqs.GetQueueUrlAsync(QueueName);
			if (!await CheckIfQueyNeedsRefresh())
			{
				return true;
			}

			//The ComputeDate >= 0 has no meaning.
			//API needs a filter so giving a bogus filter.
			var symbolCollection = await appRepository.GetAllAsync<SlickChartFirmNamesDB>(r => r.ComputeDate >= 0);
			if (symbolCollection == null || symbolCollection.Count == 0)
			{
				return false;
			}
			slickChartFirmNames = mapper.Map<List<SlickChartFirmNames>>(symbolCollection);
			var symbols = slickChartFirmNames.OrderBy(r => r.Symbol).Select(r => r.Symbol).ToList();
			if (!await StoreValuesInQueue(chunckSize, symbols))
			{
				return false;
			}
			await SaveCurrentRunDate();
			return true;
		}

		#endregion Public Methods

		#region Private Methods

		private async Task<bool> CheckIfQueyNeedsRefresh()
		{
			var lastUpdateTime = (await appRepository.GetAllAsync<RunDateSaveDB>(r => r.Symbol.Equals(ProcessName))).FirstOrDefault();
			if (lastUpdateTime != null)
			{
				logger.LogDebug("Found last run record");
				var lastRunTime = DateTimeOffset.FromUnixTimeSeconds(lastUpdateTime.LastRunTime).DateTime;
				if ((DateTime.Now.ToUniversalTime() - lastRunTime).TotalHours <= 3)
				{
					return false;
				}
			}
			return true;
		}

		private async Task SaveCurrentRunDate()
		{
			try
			{
				await appRepository.DeleteOneAsync<RunDateSaveDB>(r => r.Symbol.Equals(ProcessName));
				var currentRunDateSave = new RunDateSave
				{
					ProcessName = ProcessName,
					LastRunTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
				};
				var currentRunDateSaveDb = mapper.Map<RunDateSaveDB>(currentRunDateSave);
				currentRunDateSaveDb = (RunDateSaveDB)DBValuesSetup.SetAppDocValues(currentRunDateSaveDb);
				await appRepository.AddOneAsync<RunDateSaveDB>(currentRunDateSaveDb);
				await DBValuesSetup.CreateIndex<RunDateSaveDB>(appRepository);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while updating Run Date for {ProcessName}");
				logger.LogError($"Error details \n\t{ex.Message}");
			}
		}

		private async Task<bool> StoreValuesInQueue(int chunckSize, List<string> symbols)
		{
			var symbolChunks = symbols.ChunkBy(chunckSize);
			foreach (var symbolChunk in symbolChunks)
			{
				var msg = JsonSerializer.Serialize(symbolChunk);
				try
				{
					if (queueUrlResponse.HttpStatusCode != HttpStatusCode.OK)
					{
						logger.LogError("Could not open Queue");
						return false;
					}
					var smr = await sqs.SendMessageAsync(new SendMessageRequest
					{
						QueueUrl = queueUrlResponse.QueueUrl,
						MessageBody = msg
					});
					if (smr.HttpStatusCode != HttpStatusCode.OK)
					{
						logger.LogError("Could not save tickers in queue");
						return false;
					}
					else
					{
						logger.LogInformation("Tickers saved to queue");
					}
				}
				catch (Exception ex)
				{
					logger.LogCritical($"Error wile saving values to SQS\n\t{ex.Message}");
					return false;
				}
			}
			return true;
		}

		#endregion Private Methods
	}
}