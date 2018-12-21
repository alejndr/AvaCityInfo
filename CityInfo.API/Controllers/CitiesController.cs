using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
	// Vinculamos la clase a la ruta
	[Route("api/cities")]
	public class CitiesController : Controller
	{
		[HttpGet()]
		public IActionResult GetCities()
		{
			return Ok(CitiesDataStore.Current.Cities);
		}

		// La ruta entera seria "api/cities/{id}" pero al tener el route no hace falta
		[HttpGet("{id}")]
		public IActionResult GetCity(int id)
		{
			var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id);
			if (cityToReturn == null)
			{
				return NotFound();
			}

			return Ok(cityToReturn);
		}
	}
}
