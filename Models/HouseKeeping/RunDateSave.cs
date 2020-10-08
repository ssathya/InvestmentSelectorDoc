using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.HouseKeeping
{
    public class RunDateSave
    {
		public string Symbol { get; set; }
		public string ProcessName { get => Symbol; set => Symbol = value; }
		public long LastRunTime { get; set; }		
	}
}
