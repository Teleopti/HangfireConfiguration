using Hangfire;
using Hangfire.Configuration;
using Owin;

namespace ConsoleSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseHangfireDashboard("/HangfireDashboard");
            
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "critical", "default" },
                TaskScheduler = null
            });

            app.UseHangfireConfiguration("/HangfireConfiguration", new HangfireConfigurationOptions
            {
                ConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;",
                AllowNewStorageCreation = true,
                PrepareSchemaIfNecessary = true
            });
        }
    }
}