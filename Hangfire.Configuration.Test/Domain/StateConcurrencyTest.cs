using System.Linq;
using Hangfire.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Hangfire.Configuration.Test.Domain
{
    public class StateConcurrencyTest : XunitContextBase
    {
        public StateConcurrencyTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ShouldReturnCorrectNumberOfWorkServersConcurrently()
        {
            const int storageCount = 500;
            var system = new SystemUnderTest();
            Enumerable.Range(1, storageCount)
                .ForEach(id => { system.ConfigurationStorage.Has(new StoredConfiguration {Id = id}); });

            var run = new ConcurrencyRunner();
            run.InParallel(() =>
                {
                    var actual = system.WorkerServerQueries.QueryAllWorkerServers(null, (SqlServerStorageOptions)null).Count();
                    Assert.Equal(storageCount, actual);
                })
                .Times(100)
                .Wait();
        }
    }
}