using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.SlickChartModels;
using MongoRepository.Model.AppModel;
using MongoRepository.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISRApiHandler.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SecurityListController : BaseController<SlickChartFirmNamesDB, SlickChartFirmNames>
	{
		public SecurityListController(
			ILogger<SecurityListController> logger,
			IAppRepository<string> appRepository,
			IMapper mapper)
			: base(logger, appRepository, mapper)
		{
		}
		[HttpGet("{strIdxNm}/{indexSelector}")]
		public async Task<IActionResult> Get(string strIdxNm, IndexNames indexSelector)
		{
			_logger.LogDebug($"Getting data only for {strIdxNm}");
			if (indexSelector == IndexNames.None)
			{
				return StatusCode(StatusCodes.Status400BadRequest, "Cannot process request");
			}
			try
			{
				var records = await _appRepository.GetAllAsync<SlickChartFirmNamesDB>(r => r.Index.HasFlag(indexSelector));
				if (records.Count == 0)
				{
					return StatusCode(StatusCodes.Status204NoContent, "No records found");
				}
				var retRcds = _mapper.Map<List<SlickChartFirmNames>>(records);
				return Ok(retRcds);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error occurred while getting securities for {indexSelector}\n\t {ex.Message}");

			}
			return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong while performing Get operation");
		}
	}
}
