using System;
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
            Console.WriteLine("10 second tick!");
            context.StoppingToken.Wait(TimeSpan.FromSeconds(10));
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

            app.UseErrorPage();

            var configurationConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;";
            var defaultHangfireConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;";
            var defaultHangfireSchema = "HangFireCustomSchemaName";

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
                    DefaultSchemaName = defaultHangfireSchema
                })
                .StartServers(
                    new BackgroundJobServerOptions
                    {
                        Queues = new[] {"critical", "default"},
                    },
                    new SqlServerStorageOptions
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
                    },
                    new[] {new CustomBackgroundProcess()}
                );

            foreach (var server in HangfireConfiguration.RunningServers())
            {
                app.UseHangfireDashboard($"/HangfireDashboard{server.Number}", new DashboardOptions(), server.Storage);
            }
        }
    }
}