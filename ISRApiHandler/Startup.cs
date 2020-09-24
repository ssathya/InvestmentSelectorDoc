using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ISRApiHandler
{
	public class Startup
	{
		public const string AppS3BucketKey = "AppS3Bucket";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public static IConfiguration Configuration { get; private set; }

		// This method gets called by the runtime. Use this method to add services to the container
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			// Register the Swagger generator, defining 1 or more Swagger documents
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "Investment Selector API Handler",
					Description = "API wrapper for database calls",
					Contact = new OpenApiContact
					{
						Name = "Sridhar Sathya",
						Email = "ss_auto_delete@hotmail.com",
					}
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// Enable middle-ware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger();

			// Enable middle-ware to serve swagger-ui (HTML, JS, CSS, etc.),
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISR API Handler");
			});
			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}