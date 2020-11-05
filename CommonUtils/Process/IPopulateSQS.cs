using System.Threading.Tasks;

namespace CommonUtils.Process
{
	public interface IPopulateSQS
	{
		string ProcessName { get; set; }
		string QueueName { get; set; }

		Task<bool> PopulateSQSQueue(int chunckSize = 75);
	}
}