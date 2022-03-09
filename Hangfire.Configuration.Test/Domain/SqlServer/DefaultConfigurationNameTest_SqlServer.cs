using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain.SqlServer
{
    public class DefaultConfigurationNameTest
    {
        [Test]
        public void ShouldUpdateLegacyWithDefaultName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 3});

            var result = system.WorkerServerQueries.QueryAllWorkerServers();

            Assert.AreEqual(DefaultConfigurationName.Name(), result.Single().Name);
        }

        [Test]
        public void ShouldNotUpdateNamed()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name"});

	        var result = system.WorkerServerQueries.QueryAllWorkerServers();

	        Assert.AreEqual("name", result.Single().Name);
        }

        [Test]
        public void ShouldUpdateFirstLegacyWithDefaultName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 2, GoalWorkerCount = 3 });
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 1, GoalWorkerCount = 1 });

            var result = system.WorkerServerQueries.QueryAllWorkerServers();

            Assert.AreEqual(DefaultConfigurationName.Name(), result.Single(x => x.ConfigurationId == 1).Name);
        }

        [Test]
        public void ShouldUpdateAutoUpdateMarkedWithDefaultName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString()});

            var result = system.WorkerServerQueries.QueryAllWorkerServers();

            Assert.AreEqual(DefaultConfigurationName.Name(), result.Single().Name);
        }

        [Test]
        public void ShouldUpdateLegacyOverAutoUpdateMarkedWithDefaultName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Id = 1, 
	            ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString()
            });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Id = 2, 
	            GoalWorkerCount = 3
            });

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

            Assert.AreEqual(DefaultConfigurationName.Name(), result.Single(x => x.ConfigurationId == 2).Name);
            Assert.Null(result.Single(x => x.ConfigurationId == 1).Name);
        }
    }
}