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
            Parallel.ForEach(Enumerable.Range(1, 10), (item) =>
            {
	            using (new HangfireServerUnderTest())
	            {
	            }
            });

            Assert.Single(new ConfigurationStorage(ConnectionUtils.GetConnectionString()).ReadConfigurations());
        }
    }
}