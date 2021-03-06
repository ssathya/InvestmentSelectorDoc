﻿using AutoMapper;
using Models.AppModel;
using Models.HouseKeeping;
using Models.SlickChartModels;
using MongoRepository.Model.AppModel;
using MongoRepository.Model.HouseKeeping;

namespace MongoRepository.Setup
{
	public class AMProfile : Profile
	{
		public AMProfile()
		{
			CreateMap<IndexComponent, IndexComponentsDB>()
				.ForMember(d => d.Id, s => s.Ignore())
				.ForMember(d => d.Version, s => s.Ignore())
				.ForMember(d => d.ComputeDate, s => s.Ignore())
				.ReverseMap();
			CreateMap<SelectedFirm, SelectedFirmDB>()
				.ForMember(d => d.Id, s => s.Ignore())
				.ForMember(d => d.Version, s => s.Ignore())
				.ForMember(d => d.ComputeDate, s => s.Ignore())
				.ReverseMap();
			CreateMap<DailyPrice, DailyPriceDB>()
				.ForMember(d => d.Id, s => s.Ignore())
				.ForMember(d => d.Version, s => s.Ignore())
				.ForMember(d => d.ComputeDate, s => s.Ignore())
				.ReverseMap();
			CreateMap<HistoricPrice, HistoricPriceDB>()
				.ForMember(d => d.Id, s => s.Ignore())
				.ForMember(d => d.Version, s => s.Ignore())
				.ForMember(d => d.ComputeDate, s => s.Ignore())
				.ReverseMap();
			CreateMap<SecurityAnalysis, SecurityAnalysisDB>()
				.ForMember(d => d.Id, s => s.Ignore())
				.ForMember(d => d.Version, s => s.Ignore())
				.ForMember(d => d.ComputeDate, s => s.Ignore())
				.ReverseMap();
			CreateMap<IndexValues, IndexValuesDB>()
				.ForMember(d => d.Id, s => s.Ignore())
				.ForMember(d => d.Version, s => s.Ignore())
				.ForMember(d => d.ComputeDate, s => s.Ignore())
				.ReverseMap();
			CreateMap<SlickChartFirmNames, SlickChartFirmNamesDB>()
				.ForMember(d => d.Id, s => s.Ignore())
				.ForMember(d => d.Version, s => s.Ignore())
				.ForMember(d => d.ComputeDate, s => s.Ignore())
				.ReverseMap();
			CreateMap<RunDateSave, RunDateSaveDB>()
				.ForMember(d => d.Id, s => s.Ignore())
				.ForMember(d => d.Version, s => s.Ignore())
				.ForMember(d => d.ComputeDate, s => s.Ignore())
				.ReverseMap();
		}
	}
}
