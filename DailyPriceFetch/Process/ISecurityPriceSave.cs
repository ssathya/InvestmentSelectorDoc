using System.Threading.Tasks;

namespace DailyPriceFetch.Process
{
	public interface ISecurityPriceSave
	{
		Task<bool> GetPricingData();		
	}
}