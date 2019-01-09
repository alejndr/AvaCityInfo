using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
	public class CityInfoRepository : ICityInfoRepository
	{
		private CityInfoContext _context;

		public CityInfoRepository(CityInfoContext context)
		{
			_context = context;
		}

		public void AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest)
		{
			var city = GetCity(cityId, false);
			city.PointsOfInterest.Add(pointOfInterest);
		}

		public bool CityExists(int cityId)
		{
			return _context.Cities.Any(c => c.Id == cityId);
		}

		/// <summary>
		//  Muestra todas las ciudades haciendole una query con linq a la tabla cities.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<City> GetCities()
		{
			return _context.Cities.OrderBy(c => c.Name).ToList();
		}

		/// <summary>
		/// Muestra las ciudades con o sin sus puntos de interes.
		/// </summary>
		/// <param name="cityId"></param>
		/// <param name="includePointsOfInterest"></param>
		/// <returns></returns>
		public City GetCity(int cityId, bool includePointsOfInterest)
		{
			if (includePointsOfInterest)
			{
				return _context.Cities.Include(c => c.PointsOfInterest)
					.Where(c => c.Id == cityId).FirstOrDefault();
			}

			return _context.Cities.Where(c => c.Id == cityId).FirstOrDefault();
		}

		/// <summary>
		/// Muestra un punto de interes de una ciudad en concreto.
		/// </summary>
		/// <param name="cityId"></param>
		/// <param name="pointOfInterestId"></param>
		/// <returns></returns>
		public PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterestId)
		{
			return _context.PointOfInterests.Where(p => p.CityId == cityId && p.Id == pointOfInterestId).FirstOrDefault();
		}

		/// <summary>
		/// Muestra todos los puntos de interes de una ciudad en concreto.
		/// </summary>
		/// <param name="cityId"></param>
		/// <returns></returns>
		public IEnumerable<PointOfInterest> GetPointsOfInterestForCity(int cityId)
		{
			return _context.PointOfInterests
			.Where(p => p.CityId == cityId).ToList();
		}

		public void DetelePointOfInterest(PointOfInterest pointOfInterest)
		{
			_context.PointOfInterests.Remove(pointOfInterest);
		}

		public bool Save()
		{
			return (_context.SaveChanges() >= 0);
		}
	}
}
