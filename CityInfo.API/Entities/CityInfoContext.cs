using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Entities
{
	public class CityInfoContext : DbContext
	{
		public CityInfoContext(DbContextOptions<CityInfoContext> options) : base(options)
		{
			// Execute migrations and if there is no database create it.
			Database.Migrate();
		}

		public DbSet<City> Cities { get; set; }
		
		public DbSet<PointOfInterest> PointOfInterests { get; set; }
	}
}
