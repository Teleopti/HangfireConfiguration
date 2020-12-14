using System.Data.SqlClient;
using System.Threading;
using Xunit;
#if !NET472
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
#else
using Microsoft.Owin.Testing;
#endif

namespace Hangfire.Configuration.Test.Integration
{

    [Collection("NotParallel")]
    public class IntegrationTest
    {
	    [Fact(Skip = "Sus"), CleanDatabase]
        public void ShouldStartServerWithWorkers()
        {
            new HangfireSchemaCreator().CreateHangfireSchema(null, ConnectionUtils.GetConnectionString());
            SqlConnection.ClearAllPools(); // make the test stable for some reason
            
#if !NET472
            new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
#else
            TestServer.Create<TestStartup>();
#endif
        }
    }
    
}