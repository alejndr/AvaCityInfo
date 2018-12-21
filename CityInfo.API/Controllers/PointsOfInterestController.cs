using CityInfo.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
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
		

		/// <summary>
		/// Muestra todos los puntos de interes de la ciudad introducida.
		/// </summary>
		/// <param name="cityId"></param>
		/// <returns></returns>
		[HttpGet("{cityId}/pointsofinterest")]
		public IActionResult GetPointsOfInterest(int cityId)
		{
			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			return Ok(city.PointsOfInterest);
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
			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			var pointOfInterest = city.PointsOfInterest.FirstOrDefault(p => p.Id == PointOfInterestId);

			if (pointOfInterest == null)
			{
				return NotFound();
			}

			return Ok(pointOfInterest);
		}

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

			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			// demo purposes - to be improved
			var MaxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(
				c => c.PointsOfInterest).Max(p => p.Id);

			var finalPointOfInterest = new PointOfInterestDto()
			{
				Id = ++MaxPointOfInterestId,
				Name = pointOfInterest.Name,
				Description = pointOfInterest.Description
			};

			city.PointsOfInterest.Add(finalPointOfInterest);

			return CreatedAtRoute("GetPointOfInterest", new
			{ cityId = cityId, id = finalPointOfInterest.Id });
		}	

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

			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p =>
			p.Id == PointOfInterestId);

			if (pointOfInterestFromStore == null)
			{
				return NotFound();
			}

			pointOfInterestFromStore.Name = pointOfInterest.Name;
			pointOfInterestFromStore.Description = pointOfInterest.Description;

			return NoContent();

		}

		[HttpPatch("{cityId}/pointsofinterest/{PointOfInterestId}")]
		public IActionResult PartiallyUpdatePointOfInterest(int cityId, int PointOfInterestId,
			[FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
		{
			if (patchDoc == null)
			{
				return BadRequest();
			}

			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p =>
			p.Id == PointOfInterestId);

			if (pointOfInterestFromStore == null)
			{
				return NotFound();
			}

			var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
			{
				Name = pointOfInterestFromStore.Name,
				Description = pointOfInterestFromStore.Description
			};

			patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

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

			pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
			pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

			return NoContent();
		}

		[HttpDelete("{cityId}/pointsofinterest/{PointOfInterestId}")]
		public IActionResult DeletePointOfInterest(int cityId, int PointOfInterestId)
		{

		}



	}
}
