using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.Configuration;
using Hangfire.Server;
using Hangfire.SqlServer;
#if !NET472
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

#else
using Owin;

#endif

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
#if !NET472
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(x => { });
        }

        public void Configure(IApplicationBuilder app)
#else
        public void Configuration(IAppBuilder app)
#endif

        {
            GlobalConfiguration.Configuration
                .UseColouredConsoleLogProvider()
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

#if !NET472
            app.UseDeveloperExceptionPage();
#else
            app.UseErrorPage(new Microsoft.Owin.Diagnostics.ErrorPageOptions {ShowExceptionDetails = true});
#endif

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
                AllowNewServerCreation = true,
                AllowMultipleActive = true,
                PrepareSchemaIfNecessary = true,
                UpdateConfigurations = new []
                {
	                new UpdateStorageConfiguration
	                {
		                ConnectionString =defaultHangfireConnectionString,
		                Name = DefaultConfigurationName.Name(),
		                SchemaName = defaultHangfireSchema
	                }
                }
            };

            Console.WriteLine();
            Console.WriteLine(Program.NodeAddress + "/HangfireConfiguration");
            app.UseHangfireConfigurationUI("/HangfireConfiguration", options);

            var hangfireConfiguration = app
                    .UseHangfireConfiguration(options)
                    .UseStorageOptions(storageOptions)
                ;

            hangfireConfiguration
                .QueryAllWorkerServers()
                .Select((configurationInfo, i) => (configurationInfo, i))
                .ForEach(s =>
                {
                    Console.WriteLine(Program.NodeAddress + $"/HangfireDashboard{s.i}");
                    app.UseHangfireDashboard($"/HangfireDashboard{s.i}", new DashboardOptions(),
                        s.configurationInfo.JobStorage);
                });

            hangfireConfiguration
                .UseStorageOptions(storageOptions) //Needed???? already set above
                .UseServerOptions(new BackgroundJobServerOptions
                {
                    Queues = new[] {"critical", "default"},
                })
                .StartPublishers()
                .StartWorkerServers(new[] {new CustomBackgroundProcess()});
        }
    }
}