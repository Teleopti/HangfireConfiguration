using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
#if !NET472
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
#else
using Microsoft.Owin.Testing;
#endif

namespace Hangfire.Configuration.Test.Integration
{
    [Collection("NotParallel")]
    public class AutoUpdateConcurrencyTest
    {
        [Fact(Skip = "Sus"), CleanDatabase]
        public void ShouldNotInsertMultiple()
        {
            new HangfireSchemaCreator().CreateHangfireSchema(null, ConnectionUtils.GetConnectionString());
            Parallel.ForEach(Enumerable.Range(1, 10), (item) =>
            {
#if !NET472
                new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
#else
                TestServer.Create<TestStartup>();
#endif
            });

            Assert.Single(new ConfigurationStorage(ConnectionUtils.GetConnectionString()).ReadConfigurations());
        }
    }
}