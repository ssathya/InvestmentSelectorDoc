using AutoMapper;
using MathNet.Numerics;
using Microsoft.Extensions.Logging;
using Models.AppModel;
using Models.HouseKeeping;
using Models.SimFin;
using Models.SlickChartModels;
using MongoRepository.Model.HouseKeeping;
using MongoRepository.Repository;
using MongoRepository.Setup;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trady.Analysis.Extension;
using Utilities.AppEnv;
using Utilities.StringHelpers;

namespace DailyPriceFetch.Process
{
	public class EvaluateSecurity : IEvaluateSecurity
	{


		#region Private Fields

		private const int HoursToWait = 12;
		private const string SecAnalKey = "Security Analysis";
		private const string simfinEntries = @"https://simfin.com/api/v1/info/all-entities?api-key={apiKey}";
		private const string simfinRatios = @"https://simfin.com/api/v1/companies/id/{companyId}/ratios?api-key={apiKey}";
		private const int WindowPeriod = 120;
		private readonly IAppRepository<string> appRepository;
		private readonly HttpClient client, client1;
		private readonly string historicPriceAPI = @" https://bq0hiv4olf.execute-api.us-east-1.amazonaws.com/Prod/api/HistoricPrice/{tickersToUse}";
		private readonly string indexListAPI = @"https://bq0hiv4olf.execute-api.us-east-1.amazonaws.com/Prod/api/SecurityList/DONTCARE/{IndexId}";
		private readonly string IsrKey;
		private readonly ILogger<EvaluateSecurity> logger;
		private readonly IMapper mapper;
		private readonly string securityAnalysisAPI = @"https://bq0hiv4olf.execute-api.us-east-1.amazonaws.com/Prod/api/SecurityAnalysis";
		private readonly string simFinAPIKey;
		private List<string> securityList;
		private List<SimFinTickerToId> simFinTickerToIdsLst;

		#endregion Private Fields


		#region Public Properties

		public List<SecurityAnalysis> SecurityAnalyses { get; private set; }

		#endregion Public Properties


		#region Public Constructors

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
			client1 = clientFactory.CreateClient();

			//API Gateway key
			IsrKey = EnvHandler.GetApiKey(@"ISRApiHandler");
			simFinAPIKey = EnvHandler.GetApiKey(@"SimFinKey");
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<bool> ComputeSecurityAnalysis()
		{
			if (await CheckRunDateAsync() == false)
			{
				//already computed. Read from DB
				await PopulateSecurityAnalysesViaAPI();
				return true;
			}
			if (SecurityAnalyses == null || SecurityAnalyses.Count == 0)
			{
				SecurityAnalyses = new List<SecurityAnalysis>();
			}
			await PopulateSecurityListAsync();
			if (securityList == null || securityList.Count == 0)
			{
				return false;
			}
			foreach (var security in securityList)
			{
				var hp = await ObtainHistoricPrice(security);
				if (hp == null || hp.Candles == null || hp.Candles.Length <= WindowPeriod)
				{
					logger.LogInformation($"{security} has too little historic information");
					continue;
				}
				var noOfCandles = hp.Candles.Count();
				for (int i = 0; i < noOfCandles; i++)
				{
					hp.Candles[i].Datetime = hp.Candles[i].Datetime >= 999999999999 ?
						hp.Candles[i].Datetime /= 1000 : hp.Candles[i].Datetime;
				}
				var sa = await ComputeValuesAsync(hp);
				if (sa != null)
				{
					SecurityAnalyses.Add(sa);
				}
			}
			try
			{
				//1. Get PietroskiFScore only for top 110 companies by $$Volume.
				//2. Selecting top 110 as some of the top 100 could be Banks which
				//	do not have Pietroski Score.
				var selectedFirms = SecurityAnalyses.OrderByDescending(r => r.DollarVolumeAverage)
					.Take(110)
					.Select(r => r.Symbol);

				foreach (var symbol in selectedFirms)
				{
					var record = SecurityAnalyses.Find(r => r.Symbol == symbol);
					record.PietroskiFScore = await ComputePietroskiScore(symbol);
				}
				await SaveSecurityAnalyses();
				await UpdateRunDateAsync();
			}
			catch (Exception ex)
			{
				logger.LogError($"Issue while saving compute values\n\t{ex.Message}");
				return false;
			}
			return true;
		}

		#endregion Public Methods


		#region Private Methods

		private static JsonSerializerSettings SerializerSettings()
		{
			return new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};
		}

		private async Task<bool> CheckRunDateAsync()
		{
			var lastUpdateTime = (await appRepository.GetAllAsync<RunDateSaveDB>(r => r.Symbol.Equals(SecAnalKey))).FirstOrDefault();
			if (lastUpdateTime != null)
			{
				logger.LogDebug("Found last run record");
				var lastRunTime = DateTimeOffset.FromUnixTimeSeconds(lastUpdateTime.LastRunTime).DateTime;
				//If we ran this application within the past 12 hours don't run it again.
				if ((DateTime.Now.ToUniversalTime() - lastRunTime).TotalHours <= HoursToWait)
				{
					return false;
				}
			}
			return true;
		}

		private double ComputeDollarVolume(HistoricPrice hp)
		{
			try
			{
				var computeCandles = (from candle in hp.Candles
									  select new Trady.Core.Candle(
				  dateTime: DateTimeOffset.FromUnixTimeSeconds(candle.Datetime),
				  open: candle.Open * candle.Volume,
				  high: candle.High * candle.Volume,
				  low: candle.Low * candle.Volume,
				  close: candle.Close * candle.Volume,
				  candle.Volume)).OrderBy(r => r.DateTime);
				var a = computeCandles.FirstOrDefault();
				return Convert.ToDouble(computeCandles.Ema(30).Last().Tick ?? 0.0M);
			}
			catch (Exception ex)
			{
				logger.LogError($"Computing Dollar Volume for {hp.Symbol}");
				logger.LogError($"Error while importing historic data for momentum compute; reason\n\t{ex.Message}");
				return 0;
			}
		}

		private double ComputeEfficiencyRatio(HistoricPrice hp)
		{
			// We are computing efficiency ratio based on prices for the last one month.
			// Taking trouble to use last one month calendar days!
			var candles = hp.Candles.OrderBy(r => r.Datetime);
			var oneMonthAgo = DateTime.Now.AddMonths(-1).AddDays(-1);
			var oneMonthAgoSecs = new DateTimeOffset(oneMonthAgo).ToUnixTimeSeconds();
			var recordsToCount = candles.Where(r => r.Datetime >= oneMonthAgoSecs).Count();
			var recordsToUse =
			(from candle in candles
			 select new Trady.Core.Candle(
				 dateTime: DateTimeOffset.FromUnixTimeSeconds(candle.Datetime),
				 open: candle.Open,
				 high: candle.High,
				 low: candle.Low,
				 close: candle.Close,
				 candle.Volume)).OrderBy(r => r.DateTime).ToList();
			var efficiencyRatio = Convert.ToDouble(recordsToUse.Er(recordsToCount).Last().Tick ?? 0) * 100.0;
			return efficiencyRatio;
		}

		private void ComputeMomentum(HistoricPrice historicPrice, out double momentum, out double volatility)
		{
			var closingPrices = historicPrice.Candles.Select(r => Math.Log((double)r.Close)).ToArray();
			var seqNumbers = Enumerable.Range(0, closingPrices.Count()).Select<int, double>(i => i).ToArray();
			var leastSquaresFitting = Fit.Line(seqNumbers, closingPrices);
			var correlationCoff = GoodnessOfFit.R(seqNumbers, closingPrices);
			var annualizedSlope = (Math.Pow(Math.Exp(leastSquaresFitting.Item2), 252) - 1) * 100;
			var score = annualizedSlope * correlationCoff * correlationCoff;
			momentum = score;
			var r = Math.Exp(leastSquaresFitting.Item2) * correlationCoff * correlationCoff * 100;
			volatility = r;
		}

		private async Task<int> ComputePietroskiScore(string symbol)
		{
			if (simFinTickerToIdsLst == null || simFinTickerToIdsLst.Count == 0)
			{
				string urlForSimFinKeys = simfinEntries.Replace(@"{apiKey}", simFinAPIKey);

				try
				{
					await PopulateSimFinTickerToIds(urlForSimFinKeys);
				}
				catch (Exception ex)
				{
					logger.LogError($"Error while obtaining data from SimFin;\n\t{ex.Message}");
					return 0;
				}
			}
			decimal pfScore = await ObtainRatios(symbol);
			return Convert.ToInt32(pfScore);

		}

		private async Task<SecurityAnalysis> ComputeValuesAsync(HistoricPrice hp)
		{
			SecurityAnalysis sa = new SecurityAnalysis
			{
				Symbol = hp.Symbol
			};
			//await Task.FromResult(TestFunc(2));
			sa.DollarVolumeAverage = await Task.FromResult(ComputeDollarVolume(hp));
			sa.EfficiencyRatio = await Task.FromResult(ComputeEfficiencyRatio(hp));
			ComputeMomentum(hp, out double momentum, out double volatility);
			sa.Momentum = momentum;
			ComputeVolatility(hp, out volatility, out double inverseVolatility);
			sa.ThirtyDayVolatility = volatility;
			sa.VolatilityInverse = inverseVolatility;
			//sa.PietroskiFScore = await ComputePietroskiScore(hp.Symbol);
			return sa;
		}
		private void ComputeVolatility(HistoricPrice historicPrice, out double volatility, out double inverseVolatility)
		{
			var candles = historicPrice.Candles.OrderBy(r => r.Datetime);
			// Taking trouble to use last one month calendar days!
			var oneMonthAgo = DateTime.Now.AddMonths(-1).AddDays(-1);
			var oneMonthRecords = candles
				.Where(r => r.Datetime >= new DateTimeOffset(oneMonthAgo)
				.ToUnixTimeSeconds()).Select(r => (double)r.Close);
			volatility = Compute.ComputeMomentum.CalculateRsi(oneMonthRecords);
			volatility = Math.Sqrt(oneMonthRecords.Average(z => z * z) - Math.Pow(oneMonthRecords.Average(), 2));
			inverseVolatility = 1.0 / volatility;
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

		private async Task<decimal> ObtainRatios(string symbol)
		{
			var simFinTickerToId = simFinTickerToIdsLst.FirstOrDefault(a => a.Ticker == symbol.Trim().ToUpper());
			if (simFinTickerToId == null)
			{
				return 0;
			}
			var urlToUse = simfinRatios
				.Replace(@"{apiKey}", simFinAPIKey)
				.Replace(@"{companyId}", simFinTickerToId.SimId.ToString());
			string data = "[]";
			data = await client1.GetStringAsync(urlToUse);
			data = data.Replace(":\"N/A\"", ":null");
			var settings = SerializerSettings();
			var secRatios = JsonConvert.DeserializeObject<List<SimFinRatios>>(data, settings);
			if (secRatios == null || secRatios.Count == 0)
			{
				return 0;
			}
			var pfScore = secRatios.FirstOrDefault(pfs => pfs.IndicatorName == "Pietroski F-Score").Value ?? 0;
			return pfScore;
		}

		private async Task PopulateSecurityAnalysesViaAPI()
		{
			var drh = client.DefaultRequestHeaders;
			if (!drh.Contains(@"x-api-key"))
			{
				client.DefaultRequestHeaders.Add(@"x-api-key", IsrKey);
			}
			try
			{
				var response = await client.GetAsync(securityAnalysisAPI);
				if (!response.IsSuccessStatusCode)
				{
					logger.LogError($"Error while reading Security Analysis;\n\tError code: {response.StatusCode}");
					logger.LogError($"{response.ReasonPhrase}");
					return;
				}
				string apiResponse = await response.Content.ReadAsStringAsync();
				SecurityAnalyses = JsonConvert.DeserializeObject<List<SecurityAnalysis>>(apiResponse);
				return;
			}
			catch (Exception ex)
			{
				logger.LogError($"Excepting getting/processing PopulateSecurityAnalysesViaAPI\n\t{ex.Message}");
				return;
			}
		}

		private async Task PopulateSecurityListAsync()
		{
			var queryIndexList = indexListAPI.Replace(@"{IndexId}", Convert.ToInt32(IndexNames.SnP).ToString());

			var indexComponents = await PullSecureityListAsync(queryIndexList);
			securityList ??= new List<string>();
			securityList.AddRange(indexComponents);
			queryIndexList = indexListAPI.Replace(@"{IndexId}", Convert.ToInt32(IndexNames.Nasdaq).ToString());
			indexComponents = await PullSecureityListAsync(queryIndexList);
			securityList.AddRange(indexComponents);
			securityList = securityList.Distinct().OrderBy(a => a.Trim()).ToList();
		}

		private async Task PopulateSimFinTickerToIds(string urlForSimFinKeys)
		{
			string data = "[]";
			data = await client1.GetStringAsync(urlForSimFinKeys);
			data = data.Replace(":\"N/A\"", ":null");
			JsonSerializerSettings settings = SerializerSettings();
			simFinTickerToIdsLst = JsonConvert.DeserializeObject<List<SimFinTickerToId>>(data, settings);
			for (int i = 0; i < simFinTickerToIdsLst.Count; i++)
			{
				if (!simFinTickerToIdsLst[i].Ticker.IsNullOrWhiteSpace())
				{
					simFinTickerToIdsLst[i].Ticker = simFinTickerToIdsLst[i].Ticker.Trim().ToUpper();
				}
			}
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

		private async Task<bool> SaveSecurityAnalyses()
		{
			if (SecurityAnalyses == null || SecurityAnalyses.Count == 0)
			{
				return false;
			}
			var drh = client.DefaultRequestHeaders;
			if (!drh.Contains(@"x-api-key"))
			{
				client.DefaultRequestHeaders.Add(@"x-api-key", IsrKey);
			}
			//Don't want anything. Delete all.
			var getRunDate = DBValuesSetup.ComputeRunDate().AddMonths(1);
			var delUrlStr = $"{securityAnalysisAPI}/{getRunDate.Year}/{getRunDate.Month}/{getRunDate.Day}";
			await client.DeleteAsync(delUrlStr);
			const int stepCount = 30;
			for (int i = 0; i < SecurityAnalyses.Count; i += stepCount)
			{
				var selectedRcds = SecurityAnalyses.Skip(i).Take(stepCount);
				var json = JsonConvert.SerializeObject(selectedRcds);
				var data = new StringContent(json, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(securityAnalysisAPI, data);
				if (!response.IsSuccessStatusCode)
				{
					logger.LogError($"Error Saving security analyses: {response.ReasonPhrase}");
					return false;
				}
			}
			return true;
			//var securityAnalysesDbLst = mapper.Map<List<SecurityAnalysisDB>>(SecurityAnalyses);
			//for (int i = 0; i < securityAnalysesDbLst.Count; i++)
			//{
			//securityAnalysesDbLst[i] = (SecurityAnalysisDB)DBValuesSetup.SetAppDocValues(securityAnalysesDbLst[i]);
			//}
			//await appRepository.DeleteManyAsync<SecurityAnalysisDB>(r => r.ComputeDate != securityAnalysesDbLst[0].ComputeDate);
			//await appRepository.AddManyAsync(securityAnalysesDbLst);
			//await DBValuesSetup.CreateIndex<SecurityAnalysisDB>(appRepository);			
		}
		private async Task UpdateRunDateAsync()
		{
			await appRepository.DeleteOneAsync<RunDateSaveDB>(r => r.Symbol.Equals(SecAnalKey));
			var currentRunDateSave = new RunDateSave
			{
				ProcessName = SecAnalKey,
				LastRunTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};
			var currentRunDateSaveDb = mapper.Map<RunDateSaveDB>(currentRunDateSave);
			currentRunDateSaveDb = (RunDateSaveDB)DBValuesSetup.SetAppDocValues(currentRunDateSaveDb);
			await appRepository.AddOneAsync<RunDateSaveDB>(currentRunDateSaveDb);
			await DBValuesSetup.CreateIndex<RunDateSaveDB>(appRepository);
		}

		#endregion Private Methods

	}
}