using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{

	// Vinculamos la clase a la ruta
	[Route("api/cities")]
	public class PointsOfInterestController : Controller
	{

		private ILogger<PointsOfInterestController> _logger;
		private IMailService _mailService;
		private ICityInfoRepository _cityInfoRepository;

		public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
			IMailService mailService,
			ICityInfoRepository cityInfoRepository)
		{
			_logger = logger;
			_mailService = mailService;
			_cityInfoRepository = cityInfoRepository;
		}

		/// <summary>
		/// Muestra todos los puntos de interes de la ciudad introducida.
		/// </summary>
		/// <param name="cityId"></param>
		/// <returns></returns>
		[HttpGet("{cityId}/pointsofinterest")]
		public IActionResult GetPointsOfInterest(int cityId)
		{
			try
			{
				
				if (!_cityInfoRepository.CityExists(cityId))
				{
					_logger.LogInformation($"City with id {cityId} wasn't found when accesing points of interest");
					return NotFound();
				}

				var pointsOfInterestForCity = _cityInfoRepository.GetPointsOfInterestForCity(cityId);

				var pointsOfInterestForCityResults = Mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);

				return Ok(pointsOfInterestForCityResults);
				
			}
			catch (Exception ex)
			{
				_logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
				return StatusCode(500, "A problem happened while handling your request.");
				throw;
			}
			
		}

		/// <summary>
		/// Muestra un punto de interes especifico.
		/// </summary>
		/// <param name="cityId"></param>
		/// <param name="PointOfInterestId"></param>
		/// <returns></returns>
		[HttpGet("{cityId}/pointsofinterest/{PointOfInterestId}", Name = "GetPointOfInterest")]
		public IActionResult GetPointOfInterest(int cityId, int PointOfInterestId)
		{
			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}

			var pointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, PointOfInterestId);

			if (pointOfInterest == null)
			{
				return NotFound();
			}

			var pointOfInterestResult = Mapper.Map<PointOfInterestDto>(pointOfInterest);

			return Ok(pointOfInterestResult);

		}

		/// <summary>
		/// Crea un punto de interes.
		/// </summary>
		/// <param name="cityId"></param>
		/// <param name="pointOfInterest"></param>
		/// <returns></returns>
		[HttpPost("{cityId}/pointsofinterest")]
		public IActionResult CreatePointOfInterest(int cityId,
			[FromBody] PointOfInterestForCreationDto pointOfInterest )
		{
			if (pointOfInterest == null)
			{
				return BadRequest();
			}

			if (pointOfInterest.Description == pointOfInterest.Name)
			{
				ModelState.AddModelError("Description", "The provided description should be different from the name");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}


			var finalPointOfInterest = Mapper.Map<Entities.PointOfInterest>(pointOfInterest);

			_cityInfoRepository.AddPointOfInterestForCity(cityId, finalPointOfInterest);

			if (!_cityInfoRepository.Save())
			{
				return StatusCode(500, "A problem happened while handling your request");
			}

			var createdPointOfInterestToReturn = Mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);

			return CreatedAtRoute("GetPointOfInterest", new
			{ cityId = cityId, id = createdPointOfInterestToReturn.Id }, createdPointOfInterestToReturn);
		}	

		/// <summary>
		/// Edita un punto de interes totalmente.
		/// </summary>
		/// <param name="cityId"></param>
		/// <param name="PointOfInterestId"></param>
		/// <param name="pointOfInterest"></param>
		/// <returns></returns>
		[HttpPut("{cityId}/pointsofinterest/{PointOfInterestId}")]
		public IActionResult UpdatePointOfInterest(int cityId, int PointOfInterestId,
			[FromBody] PointOfInterestForUpdateDto pointOfInterest)
		{
			if (pointOfInterest == null)
			{
				return BadRequest();
			}

			if (pointOfInterest.Description == pointOfInterest.Name)
			{
				ModelState.AddModelError("Description", "The provided description should be different from the name");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}

			var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, PointOfInterestId);
			if (pointOfInterestEntity == null)
			{
				return NotFound();
			}

			Mapper.Map(pointOfInterest, pointOfInterestEntity);

			if (!_cityInfoRepository.Save())
			{
				return StatusCode(500, "A problem happened while handling your request");
			}

			return NoContent();

		}

		/// <summary>
		/// Edita parcialmente un punto de interes.
		/// </summary>
		/// <param name="cityId"></param>
		/// <param name="PointOfInterestId"></param>
		/// <param name="patchDoc"></param>
		/// <returns></returns>
		[HttpPatch("{cityId}/pointsofinterest/{PointOfInterestId}")]
		public IActionResult PartiallyUpdatePointOfInterest(int cityId, int PointOfInterestId,
			[FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
		{

			// Comprueba si la sintaxis de la entrada de datos es correcta con el estandar que usa patchdoc
			// En body:
			//[
			//	{
			//		"op": "replace",
			//		"path": "/name",
			//		"value": "Updated Updated - Central Park"
			//	},
			//	{
			//		"op": "replace",
			//		"path": "/description",
			// 		"value": "Updated - Description"
			// 	},
			//
			//]
			if (patchDoc == null)
			{
				return BadRequest();
			}



			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}

			var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, PointOfInterestId);
			if (pointOfInterestEntity == null)
			{
				return NotFound();
			}


			var pointOfInterestToPatch = Mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

			patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

			// Comprueba si el modelo cumple con las condiciones.
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}


			if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
			{
				ModelState.AddModelError("Description", "The provided description must be diferent from the name.");
			}

			TryValidateModel(pointOfInterestToPatch);

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			Mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

			return NoContent();
		}

		/// <summary>
		/// Borra un punto de interes.
		/// </summary>
		/// <param name="cityId"></param>
		/// <param name="PointOfInterestId"></param>
		/// <returns></returns>
		[HttpDelete("{cityId}/pointsofinterest/{PointOfInterestId}")]
		public IActionResult DeletePointOfInterest(int cityId, int PointOfInterestId)
		{
			// Comprueba si la ciudad existe.
			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}

			// Comprueba si el punto de interes existe.
			var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, PointOfInterestId);
			if (pointOfInterestEntity == null)
			{
				return NotFound();
			}

			_cityInfoRepository.DetelePointOfInterest(pointOfInterestEntity);

			if (!_cityInfoRepository.Save())
			{
				return StatusCode(500, "A problem happened while handling your request");
			}

			_mailService.Send("Point of interest deleted.",
				$"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

			return NoContent();
		}



	}
}
