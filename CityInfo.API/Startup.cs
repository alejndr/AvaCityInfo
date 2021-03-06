﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Entities;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CityInfo.API
{
	public class Startup
	{
		// Preparamos nuestra app para poder utilizar el archivo de configuración
		// que hemos creado (appSettings.json)
		public static IConfigurationRoot Configuration;

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appSettings.json", optional:false, reloadOnChange:true)
				.AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			// "XmlDataContractSerializerOutputFormatter" habilita la serielización de los datos a xml
			services.AddMvc()
				.AddMvcOptions(o => o.OutputFormatters.Add(
					new XmlDataContractSerializerOutputFormatter()));

// Compiler directives, mientras la build este en debug se nos dara una instancia de
// LocalMailService, que utilizaremos en desarrollo, si cambiamos a release
// se nos dara la instancia de CloudMailService que seria la que utilizariamos 
// una vez terminada la app
#if DEBUG
			services.AddTransient<IMailService, LocalMailService>();
#else
			services.AddTransient<IMailService, CloudMailService>();
#endif
			var connectionString = Startup.Configuration["connectionStrings:cityInfoDBConnectionString"];
			services.AddDbContext<CityInfoContext>(o => o.UseSqlServer(connectionString));

			// When we learned about dependency injection, we learned that
			// there were three lifetimes we could register to service with.
			// Transient, for services that must be created, each time they are requested
			// Scoped, for services that are created once per request and
			// Singleton, for services that are created first time they are requested.

			services.AddScoped<ICityInfoRepository, CityInfoRepository>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
			CityInfoContext cityInfoContext)
		{
			loggerFactory.AddConsole();

			loggerFactory.AddDebug();

			loggerFactory.AddNLog();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			} else
			{
				app.UseExceptionHandler();
			}

			cityInfoContext.EnsureSeedDataForContext();

			app.UseStatusCodePages();

			AutoMapper.Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<Entities.City, Models.CityWithoutPointsOfInterestDto>();
				cfg.CreateMap<Entities.City, Models.CityDto>();
				cfg.CreateMap<Models.PointOfInterestForCreationDto, Entities.PointOfInterest>();
				cfg.CreateMap<Models.PointOfInterestForUpdateDto, Entities.PointOfInterest>();
				cfg.CreateMap<Entities.PointOfInterest, Models.PointOfInterestForUpdateDto>();
			});

			app.UseMvc();

			//app.Run(async (context) =>
			//{
			//	await context.Response.WriteAsync("Hello World!");
			//});
		}
	}
}
