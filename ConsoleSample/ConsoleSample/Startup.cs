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
            var defaultHangfireSchema = "HangFireCustomSchemaName";

            app.Use((context, next) =>
            {
                // simulate a hosting site with content security policy
                context.Response.Headers.Append("Content-Security-Policy", "script-src 'self'; frame-ancestors 'self';");

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

            app.UseHangfireConfigurationUI("/HangfireConfiguration", new HangfireConfigurationUIOptions
            {
                ConnectionString = configurationConnectionString,
                AllowNewServerCreation = true,
                PrepareSchemaIfNecessary = true
            });

            app.UseHangfireConfiguration(new ConfigurationOptions
                {
                    ConnectionString = configurationConnectionString,
                    AutoUpdatedHangfireConnectionString = defaultHangfireConnectionString,
                    AutoUpdatedHangfireSchemaName = defaultHangfireSchema,
                })
//                .StartPublishers(storageOptions)
                .StartWorkerServers(
                    storageOptions,
                    new BackgroundJobServerOptions
                    {
                        Queues = new[] {"critical", "default"},
                    },
                    new[] {new CustomBackgroundProcess()}
                );
            //dashboard here
        }
    }
}