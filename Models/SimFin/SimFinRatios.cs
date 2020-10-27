using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Models.SimFin
{
    public class SimFinRatios
    {
		[JsonProperty(PropertyName = "indicatorId")]
		public string IndicatorId { get; set; }

		[JsonProperty(PropertyName = "indicatorName")]
		public string IndicatorName { get; set; }

		[JsonProperty(PropertyName = "value")]
		public decimal? Value { get; set; }

		[JsonProperty(PropertyName = "period")]
		public string Period { get; set; }

		[JsonProperty(PropertyName = "fyear")]
		public int? Fyear { get; set; }

		[JsonProperty(PropertyName = "currency")]
		public string Currency { get; set; }

		[JsonProperty(PropertyName = "period-end-date")]
		public string PeriodEndDate { get; set; }
	}
}
