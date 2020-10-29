using System.Threading.Tasks;

namespace UpdateIndexFilesInS3.AppProcessing
{
	public interface IReadSCFile
	{

		#region Public Methods

		void AddIndexETFToPricingList();
		Task<bool> ExtractValuesFromMasterSheet();

		Task<bool> StoreValuesToDb();

		#endregion Public Methods
	}
}