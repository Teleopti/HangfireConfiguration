using Hangfire.Server;
using Xunit;
#if !NET472
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder.Internal;

#else
using Owin;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Testing;
using Microsoft.Owin.Builder;
#endif

namespace Hangfire.Configuration.Test.Integration
{

    [Collection("NotParallel")]
    public class IntegrationTest
    {
        
        private class Startup
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
                    .StartWorkerServers(null, null, new IBackgroundProcess[] { })
                    ;
            }
        }

        [Fact, CleanDatabase]
        public void ShouldStartServerWithWorkers()
        {
            new HangfireSchemaCreator().CreateHangfireSchema(null, ConnectionUtils.GetConnectionString());
            
#if !NET472
            new TestServer(new WebHostBuilder().UseStartup<Startup>());
#else
            TestServer.Create<Startup>();
#endif
        }
    }
    
}