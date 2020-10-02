using Models.SlickChartModels;
using SheetToObjects.Lib.FluentConfiguration;

namespace UpdateIndexFilesInS3.Mapper
{
	public class SlickConfig : SheetToObjectConfig
	{
		public SlickConfig()
		{
			CreateMap<SlickChartExtract>(x => x
			.HasHeaders()
			.MapColumn(c => c.WithHeader("Symbol").IsRequired().MapTo(m => m.Symbol))
			.MapColumn(c => c.WithHeader("Company").IsRequired().MapTo(m => m.Company))
			.MapColumn(c => c.WithHeader("Weight").MapTo(m => m.Weight))
			.MapColumn(c => c.WithHeader("Price").MapTo(m => m.Price))
			.MapColumn(c => c.WithHeader("Chg").MapTo(m => m.Change))
			.MapColumn(c => c.WithColumnLetter("G").MapTo(m => m.PercentChg))
			.MapColumn(c => c.WithHeader("#").MapTo(m => m.Number))
			);

		}
	}
}
