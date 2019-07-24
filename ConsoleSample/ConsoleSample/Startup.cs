using System.Drawing.Text;
using Hangfire;
using Hangfire.Configuration;
using Owin;

namespace ConsoleSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var connectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;";

            var storedConnectionString = HangfireConfiguration.ReadActiveConfigurationConnectionString(connectionString);
            if (storedConnectionString != null)
                connectionString = storedConnectionString;
            else
                HangfireConfiguration.ConfigureDefaultStorage(connectionString, connectionString, "HangFire");
          
            app.UseErrorPage();
            app.UseHangfireDashboard("/HangfireDashboard");
            
            
            var determiner = HangfireConfiguration.GetWorkerDeterminer(connectionString);
            var defaultWorkerCount = determiner.DetermineStartingServerWorkerCount();
            
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "critical", "default" },
                TaskScheduler = null,
                WorkerCount = defaultWorkerCount
            });
            
            app.UseHangfireConfiguration("/HangfireConfiguration", new HangfireConfigurationOptions
            {
                ConnectionString = connectionString,
                AllowNewStorageCreation = true,
                PrepareSchemaIfNecessary = true
            });
        }
    }
}