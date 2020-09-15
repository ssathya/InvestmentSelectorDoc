using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.AWSS3;
using Utilities.Models;
using Utilities.StringHelpers;

namespace Utilities.AppEnv
{
	public static class SetApplicationEnvVar
	{
		private static bool _envVariablesRead = false;

		public static void SetEnvVariablesFromS3()
		{
			if (_envVariablesRead)
			{
				return;
			}
			var readS3Objs = new ReadWriteS3Objects();
			var keysToServices = JsonConvert
				.DeserializeObject<List<EntityKeys>>(readS3Objs
					.GetEncryptedDataFromS3("Random.txt")
				.Result);
			AdjustForDevEnv(keysToServices);
			foreach (var entityKeys in keysToServices)
			{
				if (!string.IsNullOrEmpty(entityKeys.Entity)
					&& !string.IsNullOrEmpty(entityKeys.Key))
					Environment.SetEnvironmentVariable(entityKeys.Entity, entityKeys.Key);
			}
			_envVariablesRead = true;
		}

		private static void AdjustForDevEnv(List<EntityKeys> keysToServices)
		{
			var enKeyValue = Environment.GetEnvironmentVariable("RunEnv");
			if (!enKeyValue.IsNullOrWhiteSpace() && enKeyValue.Equals("Development"))
			{
				var valueToRemove = keysToServices.FirstOrDefault(r => r.Entity.Equals("InvestDb"));
				var alternateEntity = keysToServices.FirstOrDefault(r => r.Entity.Equals("InvestDb1"));
				if (valueToRemove != null && alternateEntity != null)
				{
					keysToServices.Remove(valueToRemove);
					alternateEntity.Entity = "InvestDb";
					keysToServices.Add(alternateEntity);
				}
			}
			keysToServices = keysToServices.Distinct().ToList();
		}
	}
}
