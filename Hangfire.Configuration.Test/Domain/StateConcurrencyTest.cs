using System;
using System.Linq;
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
                .ForEach(id => { system.Repository.Has(new StoredConfiguration {Id = id}); });

            var run = new ConcurrencyRunner();
            run.InParallel(() =>
                {
                    var actual = system.WorkerServerQueries.QueryAllWorkerServers(null, null).Count();
                    Assert.Equal(storageCount, actual);
                })
                .Times(100)
                .Wait();
        }
    }
}