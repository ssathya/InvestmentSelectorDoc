﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoRepository.Model;
using MongoRepository.Repository;
using MongoRepository.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.AppEnv;

namespace ISRApiHandler.Controllers
{
	//[Route("api/[controller]")]
	[ApiController]
	public abstract class BaseController<T, T1> : ControllerBase where T : IAPPDoc
	{

		#region Protected Fields

		protected readonly IAppRepository<string> _appRepository;
		protected readonly ILogger _logger;
		protected readonly IMapper _mapper;

		#endregion Protected Fields


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
			symbol = symbol.ToUpper();
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

		[HttpDelete("{year}/{month}/{date}")]
		public virtual async Task<ActionResult> Delete(int year, int month, int date)
		{
			var tmpDate = DateTime.SpecifyKind(new DateTime(year, month, date), DateTimeKind.Utc);
			var parameter = new DateTimeOffset(new DateTime(year, month, date));
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
				long curtoffTime = DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds();
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
			symbol = symbol.ToUpper();
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
			
			try
			{
				var recordsToSave = _mapper.Map<List<T>>(newRecords);
				for (int i = 0; i < recordsToSave.Count; i++)
				{
					IAPPDoc curRcd = recordsToSave[i];
					recordsToSave[i] = (T)DBValuesSetup.SetAppDocValues(curRcd);
				}
				await _appRepository.AddManyAsync(recordsToSave);
				await DBValuesSetup.CreateIndex<T>(_appRepository);
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