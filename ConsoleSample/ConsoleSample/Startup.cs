using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.Configuration;
using Hangfire.Server;
using Hangfire.SqlServer;
using Owin;

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
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseColouredConsoleLogProvider()
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            app.UseErrorPage(new Microsoft.Owin.Diagnostics.ErrorPageOptions {ShowExceptionDetails = true});

            var configurationConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;";
            var defaultHangfireConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;";
            var defaultHangfireSchema = "HangFireCustomSchemaName";

            app.Use((context, next) =>
            {
                // simulate a hosting site with content security policy
                context.Response.Headers.Append("Content-Security-Policy", "script-src 'self'; frame-ancestors 'self';");

                // simulate a hosting site with a static file handler
                if (context.Request.Uri.AbsolutePath.Split('/').Last().Contains("."))
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    return Task.CompletedTask;
                }

                return next.Invoke();
            });

            app.UseHangfireConfigurationInterface("/HangfireConfiguration", new HangfireConfigurationInterfaceOptions
            {
                ConnectionString = configurationConnectionString,
                AllowNewServerCreation = true,
                PrepareSchemaIfNecessary = true
            });

            app.UseHangfireConfiguration(new ConfigurationOptions
                {
                    ConnectionString = configurationConnectionString,
                    DefaultHangfireConnectionString = defaultHangfireConnectionString,
                    DefaultSchemaName = defaultHangfireSchema,
                })
                .Start(new SqlServerStorageOptions
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
                })
                .WithServers(
                    new BackgroundJobServerOptions
                    {
                        Queues = new[] {"critical", "default"},
                    },
                    new[] {new CustomBackgroundProcess()}
                )
                .EnabledStorages()
                .Select((es, i) =>
                {
                    var number = i + 1; 
                    app.UseHangfireDashboard($"/HangfireDashboard{number}", new DashboardOptions(), es.JobStorage);
                    Console.WriteLine($@"Starting dashboard for storage {number}: 
                                                Schema: {es.Configuration.SchemaName} 
                                                GoalWorkerCount: {es.Configuration.GoalWorkerCount} 
                                                Connection: {es.Configuration.ConnectionString} 
                                                Active: { es.Configuration.Active },");
                    return es;
                }).ToArray();

        }
    }
}