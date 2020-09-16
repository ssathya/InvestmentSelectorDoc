namespace MongoRepository.Model.AppModel
{
	public class SelectedFirmDB : Models.Index.SelectedFirm, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
