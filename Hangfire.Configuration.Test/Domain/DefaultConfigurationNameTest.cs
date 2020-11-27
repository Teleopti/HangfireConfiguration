using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Hangfire.Configuration.Test.Domain
{
    public class DefaultConfigurationNameTest : XunitContextBase
    {
        public DefaultConfigurationNameTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ShouldUpdateLegacyWithDefaultName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.Has(new StoredConfiguration {GoalWorkerCount = 3});

            var result = system.WorkerServerQueries.QueryAllWorkerServers(null, null);

            Assert.Equal(DefaultConfigurationName.Name(), result.Single().Name);
        }

        [Fact]
        public void ShouldNotUpdateNamed()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.Has(new StoredConfiguration {Name = "name"});

            var result = system.WorkerServerQueries.QueryAllWorkerServers(null, null);

            Assert.Equal("name", result.Single().Name);
        }

        [Fact]
        public void ShouldUpdateFirstLegacyWithDefaultName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.Has(new StoredConfiguration {Id = 2, GoalWorkerCount = 3});
            system.ConfigurationRepository.Has(new StoredConfiguration {Id = 1, GoalWorkerCount = 1});

            var result = system.WorkerServerQueries.QueryAllWorkerServers(null, null);

            Assert.Equal(DefaultConfigurationName.Name(), result.Single(x => x.ConfigurationId == 1).Name);
        }

        [Fact]
        public void ShouldUpdateAutoUpdateMarkedWithDefaultName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString()});

            var result = system.WorkerServerQueries.QueryAllWorkerServers(null, null);

            Assert.Equal(DefaultConfigurationName.Name(), result.Single().Name);
        }

        [Fact]
        public void ShouldUpdateLegacyOverAutoUpdateMarkedWithDefaultName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.Has(new StoredConfiguration {Id = 1, ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString()});
            system.ConfigurationRepository.Has(new StoredConfiguration {Id = 2, GoalWorkerCount = 3});

            var result = system.WorkerServerQueries.QueryAllWorkerServers(null, null);

            Assert.Equal(DefaultConfigurationName.Name(), result.Single(x => x.ConfigurationId == 2).Name);
            Assert.Null(result.Single(x => x.ConfigurationId == 1).Name);
        }
    }
}