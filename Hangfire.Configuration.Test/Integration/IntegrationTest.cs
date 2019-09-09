using Xunit;
#if !NET472
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder.Internal;
#else
using Microsoft.Owin.Testing;
#endif

namespace Hangfire.Configuration.Test.Integration
{

    [Collection("NotParallel")]
    public class IntegrationTest
    {
        [Fact, CleanDatabase]
        public void ShouldStartServerWithWorkers()
        {
            new HangfireSchemaCreator().CreateHangfireSchema(null, ConnectionUtils.GetConnectionString());
            
#if !NET472
            new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
#else
            TestServer.Create<TestStartup>();
#endif
        }
    }
    
}