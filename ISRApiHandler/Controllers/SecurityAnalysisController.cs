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
	public class SecurityAnalysisController : BaseController<SecurityAnalysisDB, SecurityAnalysis>
	{

		#region Public Constructors

		public SecurityAnalysisController(ILogger<SecurityAnalysisController> logger,
			IAppRepository<string> appRepository,
			IMapper mapper)
			: base(logger, appRepository, mapper)
		{
		}

		#endregion Public Constructors
	}
}