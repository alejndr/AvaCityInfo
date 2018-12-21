using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CityInfo.API
{
	public class Startup
	{
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
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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

			app.UseStatusCodePages();

			app.UseMvc();

			app.Run(async (context) =>
			{
				await context.Response.WriteAsync("Hello World!");
			});
		}
	}
}
