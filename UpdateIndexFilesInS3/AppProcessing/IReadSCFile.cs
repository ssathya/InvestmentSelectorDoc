using System.Threading.Tasks;

namespace UpdateIndexFilesInS3.AppProcessing
{
	public interface IReadSCFile
	{
		Task<bool> ExtractValuesFromMasterSheet();

		Task<bool> StoreValuesToDb();
	}
}