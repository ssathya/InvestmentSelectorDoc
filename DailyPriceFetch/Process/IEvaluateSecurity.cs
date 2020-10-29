using Models.AppModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyPriceFetch.Process
{
	public interface IEvaluateSecurity
	{
		List<SecurityAnalysis> SecurityAnalyses { get; }

		Task<bool> ComputeSecurityAnalysis();
	}
}