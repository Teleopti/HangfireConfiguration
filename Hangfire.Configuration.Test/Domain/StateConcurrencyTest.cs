using System.Linq;
using Hangfire.SqlServer;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class StateConcurrencyTest
    {
        [Test]
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
                    Assert.AreEqual(storageCount, actual);
                })
                .Times(100)
                .Wait();
        }
    }
}