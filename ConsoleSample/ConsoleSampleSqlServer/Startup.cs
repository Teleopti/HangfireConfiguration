using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.Configuration;
using Hangfire.Server;
using Hangfire.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ConsoleSample
{
	public class CustomBackgroundProcess : IBackgroundProcess
	{
		public void Execute(BackgroundProcessContext context)
		{
			Console.WriteLine("20 second tick!");
			context.StoppingToken.Wait(TimeSpan.FromSeconds(20));
		}
	}

	public class Startup
	{
		public static HangfireConfiguration HangfireConfiguration;

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddHangfire(x => { });
		}

		public void Configure(IApplicationBuilder app)

		{
			GlobalConfiguration.Configuration
				.UseColouredConsoleLogProvider()
				.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings();

			app.UseDeveloperExceptionPage();

			var configurationConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;";
			var defaultHangfireConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;";
			var defaultHangfireSchema = "hangfirecustomschemaname";

			app.Use((context, next) =>
			{
				// simulate a hosting site with content security policy
				context.Response.Headers.Append("Content-Security-Policy",
					"script-src 'self'; frame-ancestors 'self';");

				// simulate a hosting site with a static file handler
				if (context.Request.Path.Value.Split('/').Last().Contains("."))
				{
					context.Response.StatusCode = (int) HttpStatusCode.NotFound;
					return Task.CompletedTask;
				}

				return next.Invoke();
			});

			var storageOptions = new SqlServerStorageOptions
			{
				CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
				QueuePollInterval = TimeSpan.Zero,
				SlidingInvisibilityTimeout = TimeSpan.FromMinutes(1),
				UseRecommendedIsolationLevel = true,
				UsePageLocksOnDequeue = true,
				DisableGlobalLocks = true,
				EnableHeavyMigrations = true,
				PrepareSchemaIfNecessary = true,
				SchemaName = "NotUsedSchemaName"
			};

			var options = new ConfigurationOptions
			{
				ConnectionString = configurationConnectionString,
				PrepareSchemaIfNecessary = true,
				UpdateConfigurations = new[]
				{
					new UpdateStorageConfiguration
					{
						ConnectionString = defaultHangfireConnectionString,
						Name = DefaultConfigurationName.Name(),
						SchemaName = defaultHangfireSchema
					}
				}
			};

			Console.WriteLine();
			Console.WriteLine(Program.NodeAddress + "/HangfireConfiguration");
			app.UseHangfireConfigurationUI("/HangfireConfiguration", options);

			HangfireConfiguration = app
					.UseHangfireConfiguration(options)
					.UseStorageOptions(storageOptions)
				;

			HangfireConfiguration
				.UseStorageOptions(storageOptions) //Needed???? already set above
				.UseServerOptions(new BackgroundJobServerOptions
				{
					Queues = new[] {"critical", "default"},
				})
				.StartPublishers()
				.StartWorkerServers(new[] {new CustomBackgroundProcess()});

			HangfireConfiguration
				.QueryAllWorkerServers()
				.ForEach(x => { Console.WriteLine(Program.NodeAddress + $"/HangfireDashboard/{x.ConfigurationId}"); });

			app.UseDynamicHangfireDashboards("/HangfireDashboard", options, new DashboardOptions());
		}
	}
}