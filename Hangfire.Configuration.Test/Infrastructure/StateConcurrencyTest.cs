using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
    public class StateConcurrencyTest : DatabaseTestBase
    {
        [Test]
        public void ShouldReturnCorrectNumberOfWorkServersConcurrently()
        {
            const int storageCount = 500;
            var system = new SystemUnderInfraTest();
            system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
            Enumerable.Range(1, storageCount)
                .ForEach(_ => { system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {ConnectionString = ConnectionString}); });

            var hangfireConfiguration = system.StartWorkerServers();
            var run = new ConcurrencyRunner();
            run.InParallel(() =>
                {
                    var actual = hangfireConfiguration.QueryAllWorkerServers().Count();
                    Assert.AreEqual(storageCount, actual);
                })
                .Times(100)
                .Wait();
        }

        public StateConcurrencyTest(string connectionString) : base(connectionString)
        {
        }
    }
}