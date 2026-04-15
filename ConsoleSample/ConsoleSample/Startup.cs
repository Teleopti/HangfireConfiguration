using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ConsoleSample;

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

		var db = Program.DatabaseSelection;
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

		var options = new ConfigurationOptions
		{
			ConnectionString = db.ConfigurationConnectionString,
			PrepareSchemaIfNecessary = true,
			ExternalConfigurations =
			[
				new ExternalConfiguration
				{
					ConnectionString = db.DefaultHangfireConnectionString,
					Name = DefaultConfigurationName.Name(),
					SchemaName = defaultHangfireSchema
				}
			]
		};

		Console.WriteLine();
		Console.WriteLine(Program.NodeAddress + "/HangfireConfiguration");
		app.UseHangfireConfigurationUI("/HangfireConfiguration", options);

		HangfireConfiguration = app
				.UseHangfireConfiguration(options)
				.UseStorageOptions(db.StorageOptions)
			;

		HangfireConfiguration
			.UseApplicationBuilder(app)
			.UseServerOptions(new BackgroundJobServerOptions
			{
				Queues = new[] {"critical", "invoices", "default"},
			})
			.StartPublishers()
			.StartBackgroundJobServers([new SampleBackgroundProcess()]);

		HangfireConfiguration
			.QueryAllBackgroundJobServers()
			.ForEach(x =>
			{
				//
				Console.WriteLine(Program.NodeAddress + $"/HangfireDashboard/{x.ConfigurationId}");
			});

		app.UseDynamicHangfireDashboards("/HangfireDashboard", options, new DashboardOptions());
	}
}
