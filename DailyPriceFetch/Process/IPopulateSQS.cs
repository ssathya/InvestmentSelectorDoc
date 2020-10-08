using System.Threading.Tasks;

namespace DailyPriceFetch.Process
{
	public interface IPopulateSQS
	{
		Task<bool> PopulateSQSQueue(int chunckSize = 75);
	}
}