namespace Models.HouseKeeping
{
	public class RunDateSave
	{
		public string Symbol { get; set; }
		public string ProcessName { get => Symbol; set => Symbol = value; }
		public long LastRunTime { get; set; }
	}
}
