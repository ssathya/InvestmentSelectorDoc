using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.AppModel;
using MongoRepository.Model.AppModel;
using MongoRepository.Repository;

namespace ISRApiHandler.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HistoricPriceController : BaseController<HistoricPriceDB, HistoricPrice>
	{

		#region Public Constructors

		public HistoricPriceController(ILogger<HistoricPriceController> logger,
			IAppRepository<string> appRepository,
			IMapper mapper)
			: base(logger: logger, appRepository: appRepository, mapper: mapper)
		{
		}

		#endregion Public Constructors
	}
}