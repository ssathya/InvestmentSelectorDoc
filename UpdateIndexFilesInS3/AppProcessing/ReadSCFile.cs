using AutoMapper;
using Models.SlickChartModels;
using MongoRepository.Model;
using MongoRepository.Model.AppModel;
using MongoRepository.Repository;
using MongoRepository.Setup;
using SheetToObjects.Adapters.MicrosoftExcel;
using SheetToObjects.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UpdateIndexFilesInS3.Mapper;

namespace UpdateIndexFilesInS3.AppProcessing
{
	//read Slick Chart file.

	public class ReadSCFile : IReadSCFile
	{

		#region Private Fields

		private const string excelFileUrl = @"https://1drv.ms/x/s!ApMGIczTfCKAts9wQoWTXRE4-uz2hA?e=e4h8xg";

		private const string NASDAQ100 = "Nasdaq 100";

		private const string SnP500 = "S&P 500";

		private readonly IAppRepository<string> appRepository;
		private readonly SheetProvider excelSheetAdapter;

		private readonly SheetMapper exSheetMapper;
		private readonly IMapper mapper;
		private string fileFullPath;

		private List<SlickChartFirmNames> slickChartFirmNames;

		#endregion Private Fields


		#region Public Constructors

		public ReadSCFile(IMapper mapper, IAppRepository<string> appRepository)
		{
			exSheetMapper = new SheetMapper()
				.AddSheetToObjectConfig(new SlickConfig());
			excelSheetAdapter = new SheetProvider();
			slickChartFirmNames = new List<SlickChartFirmNames>();
			this.mapper = mapper;
			this.appRepository = appRepository;
		}

		#endregion Public Constructors


		#region Private Destructors

		~ReadSCFile()
		{
			DoCleanUp();
		}

		#endregion Private Destructors


		#region Public Methods

		public async Task<bool> ExtractValuesFromMasterSheet()
		{
			fileFullPath = await CopyFileFromOneDrive();
			if (string.IsNullOrEmpty(fileFullPath))
			{
				return false;
			}
			Task.WaitAll();
			if (ExtractSsData(SnP500) == false)
			{
				return false;
			}
			if (ExtractSsData(NASDAQ100) == false)
			{
				return false;
			}
			return true;
		}

		public async Task<bool> StoreValuesToDb()
		{
			if (slickChartFirmNames == null || slickChartFirmNames.Count == 0)
			{
				return false;
			}
			var scfnDB = mapper.Map<List<SlickChartFirmNamesDB>>(slickChartFirmNames);
			for (int i = 0; i < scfnDB.Count; i++)
			{
				IAPPDoc aPPDoc = scfnDB[i];
				scfnDB[i] = (SlickChartFirmNamesDB)DBValuesSetup.SetAppDocValues(aPPDoc);
			}
			await appRepository.DeleteManyAsync<SlickChartFirmNamesDB>(r => r.ComputeDate < scfnDB[0].ComputeDate);
			await appRepository.AddManyAsync(scfnDB);
			return true;
		}

		#endregion Public Methods


		#region Private Methods

		private static IndexNames ObtainIndexName(string tabName)
		{
			IndexNames indexNames = IndexNames.None;
			switch (tabName)
			{
				case SnP500:
					indexNames |= IndexNames.SnP;
					break;

				case NASDAQ100:
					indexNames |= IndexNames.Nasdaq;
					break;

				default:
					indexNames = IndexNames.None;
					break;
			}

			return indexNames;
		}

		private async Task<string> CopyFileFromOneDrive()
		{
			var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xlsx");
			using var client = new WebClient();
			string base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(excelFileUrl));
			string encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');
			var resultUrl = string.Format("https://api.onedrive.com/v1.0/shares/{0}/root/content", encodedUrl);

			var uri = new Uri(resultUrl);
			var data = await Task.Run(() => client.DownloadData(uri));
			using var fs = new FileStream(filePath, FileMode.CreateNew);
			await fs.WriteAsync(data);
			return filePath;
		}

		private void DoCleanUp()
		{
			if (!string.IsNullOrWhiteSpace(fileFullPath) && File.Exists(fileFullPath))
			{
				File.Delete(fileFullPath);
				fileFullPath = "";
			}
		}

		private bool ExtractSsData(string tabName)
		{
			var excelRange = new ExcelRange(new ExcelCell("A", 1), new ExcelCell("G", 600));
			var exSheet = excelSheetAdapter.GetFromPath(fileFullPath, tabName, excelRange, true);
			var exMapResult = exSheetMapper.Map<SlickChartExtract>(exSheet);
			var a1 = exMapResult.ParsedModels;
			if (exMapResult.IsFailure)
			{
				return false;
			}
			if (exMapResult.IsSuccess)
			{
				var tmpslickChartFirmNames = a1.Select(a => a.Value).OrderBy(r => r.Company).ToDictionary(x => x.Symbol, x => x.Company);

				//slickChartExtracts.AddRange(a1.Select(a => a.Value));
				IndexNames indexNames = ObtainIndexName(tabName);
				foreach (var keyValuePair in tmpslickChartFirmNames)
				{
					var key = keyValuePair.Key;
					var result = slickChartFirmNames.Where(r => r.Symbol.Equals(key)).FirstOrDefault();
					if (result == null)
					{
						result = new SlickChartFirmNames
						{
							Company = keyValuePair.Value,
							Symbol = key,
							Index = IndexNames.None
						};
						slickChartFirmNames.Add(result);
					}
					result.Index |= indexNames;
				}
			}
			return true;
		}

		#endregion Private Methods
	}
}