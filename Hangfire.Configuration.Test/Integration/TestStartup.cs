using Hangfire.Server;
#if !NET472
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#else
using Owin;
#endif

namespace Hangfire.Configuration.Test.Integration
{
    public class TestStartup
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
            app
                .UseHangfireConfiguration(new ConfigurationOptions
                {
                    ConnectionString = ConnectionUtils.GetConnectionString(),
                    AutoUpdatedHangfireConnectionString = ConnectionUtils.GetConnectionString(),
                    UseWorkerDeterminer = true
                })
                .StartWorkerServers( new IBackgroundProcess[] { })
                ;
        }
    }
}