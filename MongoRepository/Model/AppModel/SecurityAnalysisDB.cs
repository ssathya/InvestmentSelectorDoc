namespace MongoRepository.Model.AppModel
{
	public class SecurityAnalysisDB : Models.Index.SecurityAnalysis, IAPPDoc
	{
		public long ComputeDate { get; set; }
		public string Id { get; set; }
		public int Version { get; set; }
	}
}
