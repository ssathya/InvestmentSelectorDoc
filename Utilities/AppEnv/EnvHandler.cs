using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilities.AWS_S3;
using Utilities.StringHelpers;

namespace Utilities.AppEnv
{
	public static class EnvHandler
	{

		#region Private Fields

		private static Dictionary<string, string> resourcesDict;

		#endregion Private Fields


		#region Public Methods

		public static string GetApiKey(string provider)
		{
			var apiKey = Environment.GetEnvironmentVariable(provider);
			if (!apiKey.IsNullOrWhiteSpace())
			{
				return apiKey;
			}
			SetApplicationEnvVar.SetEnvVariablesFromS3();
			apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.Process);
			if (apiKey.IsNullOrWhiteSpace())
			{
				apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.Machine);
			}
			if (apiKey.IsNullOrWhiteSpace())
			{
				apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.User);
			}
			if (apiKey.IsNullOrWhiteSpace())
			{
				apiKey = Environment.GetEnvironmentVariable("NewsAPI");
			}

			return apiKey;
		}

		public static async Task<Dictionary<string, string>> ResourceFileContent()
		{
			if (resourcesDict != null && resourcesDict.Any())
			{
				return resourcesDict;
			}

			var resourceFileName = Environment.GetEnvironmentVariable("ResourceFilePath");
			if (resourceFileName.IsNullOrWhiteSpace())
			{
				return null;
			}

			try
			{
				string txt;
				if (!File.Exists(resourceFileName))
				{
					var readS3Objs = new ReadWriteS3Objects();
					var fileName = Path.GetFileName(resourceFileName);
					txt = await readS3Objs.GetDataFromS3(fileName);
				}
				else
				{
					txt = await File.ReadAllTextAsync(resourceFileName);
				}
				ConvertTxtToResourceDict(txt);
				return resourcesDict;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception while reading environment file\n\t{0}", ex.Message);
				return null;
			}
		}

		#endregion Public Methods


		#region Private Methods

		private static void ConvertTxtToResourceDict(string txt)
		{
			var rfc = JObject.Parse(txt);

			resourcesDict = rfc.ToObject<Dictionary<string, string>>();
			//var resourceFileContent = JObject.Parse(await File.ReadAllTextAsync(resourceFileName));
			//resourcesDict = resourceFileContent.ToObject<Dictionary<string, string>>();
			resourcesDict = rfc.ToObject<Dictionary<string, string>>();
		}

		#endregion Private Methods
	}
}