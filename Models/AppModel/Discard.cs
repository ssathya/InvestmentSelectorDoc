using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.AppModel
{
	public class Discard
	{

	}

	public class Rootobject
	{
		public Class1[] Property1 { get; set; }
	}

	public class Class1
	{
		public Candlef[] candles { get; set; }
		public string symbol { get; set; }
		public bool empty { get; set; }
	}

	public class Candlef
	{
		public float open { get; set; }
		public float high { get; set; }
		public float low { get; set; }
		public float close { get; set; }
		public int volume { get; set; }
		public long datetime { get; set; }
	}

}
