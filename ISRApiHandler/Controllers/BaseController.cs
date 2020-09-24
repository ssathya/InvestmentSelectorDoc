using AutoMapper;
using ISRApiHandler.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoRepository.Model;
using MongoRepository.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.AppEnv;

namespace ISRApiHandler.Controllers
{
	//[Route("api/[controller]")]
	[ApiController]
	public abstract class BaseController<T, T1> : ControllerBase where T : IAPPDoc
	{
		#region Private Fields

		private readonly IAppRepository<string> _appRepository;
		private readonly ILogger _logger;
		private readonly IMapper _mapper;

		#endregion Private Fields

		#region Public Constructors

		public BaseController(ILogger logger, IAppRepository<string> appRepository, IMapper mapper)
		{
			_logger = logger;
			_appRepository = appRepository;
			_mapper = mapper;
		}

		#endregion Public Constructors

		#region Public Methods

		[HttpDelete("{symbol}")]
		public virtual async Task<ActionResult> Delete(string symbol)
		{
			try
			{
				var result = await _appRepository.DeleteManyAsync<T>(r => r.Symbol.Equals(symbol));
				return Ok(result >= 0);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error occurred while performing Delete operation; error:\n {ex.Message} ");
				return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong while performing Delete operation");
			}
		}

		[HttpDelete("{parameter}")]
		public virtual async Task<ActionResult> Delete(DateTimeOffset parameter)
		{
			try
			{
				var cutOffTime = parameter.ToUniversalTime().ToUnixTimeSeconds();
				var result = await _appRepository.DeleteManyAsync<T>(r => r.ComputeDate <= cutOffTime);
				return Ok(result >= 0);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error occurred while performing Delete operation; error:\n {ex.Message} ");
				return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong while performing Delete operation");
			}
		}

		[HttpGet]
		public virtual async Task<IActionResult> Get()
		{
			try
			{
				long curtoffTime = DateTimeOffset.Now.AddDays(-1).UtcTicks;
				var records = await _appRepository.GetAllAsync<T>(r => r.ComputeDate >= curtoffTime);
				var retRcds = _mapper.Map<List<T1>>(records);
				return Ok(retRcds);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error occurred while performing Get operation; error:\n {ex.Message} ");
				return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong while performing Get operation");
			}
		}

		[HttpGet("{symbol}")]
		public virtual async Task<IActionResult> Get(string symbol)
		{
			try
			{
				var records = await _appRepository.GetAllAsync<T>(r => r.Symbol.Equals(symbol));
				var retRcds = _mapper.Map<List<T1>>(records);
				return Ok(retRcds);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error occurred while performing Get operation; error:\n {ex.Message} ");
				return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong while performing Get operation");
			}
		}

		[HttpPost]
		public virtual async Task<ActionResult> Post([FromBody] IList<T1> newRecords)
		{
			if (!ModelState.IsValid)
			{
				return StatusCode(StatusCodes.Status422UnprocessableEntity, "Can't processable Entities");
			}
			var recordsToSave = _mapper.Map<List<T>>(newRecords);
			for (int i = 0; i < recordsToSave.Count; i++)
			{
				IAPPDoc curRcd = recordsToSave[i];
				recordsToSave[i] = (T)DBValuesSetup.SetAppDocValues(curRcd);
			}
			try
			{
				await _appRepository.AddManyAsync(recordsToSave);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error occurred while performing Post operation; error:\n {ex.Message} ");
				return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong while performing Post operation");
			}
			return Ok(true);
		}

		#endregion Public Methods
	}
}