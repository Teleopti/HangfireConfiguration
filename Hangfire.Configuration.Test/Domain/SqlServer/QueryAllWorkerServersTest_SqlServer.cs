using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain.SqlServer
{
    public class QueryAllWorkerServersTest
    {
        [Test]
        public void ShouldQueryWorkerServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServers = system.WorkerServerQueries.QueryAllWorkerServers(null, (SqlServerStorageOptions)null);

            Assert.NotNull(workerServers.Single());
        }

        [Test]
        public void ShouldReturnWorkerServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (SqlServerStorageOptions)null).Single();

            workerServer.JobStorage
	            .Should().Be.SameInstanceAs(system.Hangfire.CreatedStorages.Single());
        }

        [Test]
        public void ShouldAutoUpdate()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerQueries
                .QueryAllWorkerServers(
                    new ConfigurationOptions
                    {
	                    UpdateConfigurations = new []
	                    {
		                    new UpdateStorageConfiguration
		                    {
			                    ConnectionString = new SqlConnectionStringBuilder{ DataSource = "Hangfire" }.ToString(),
			                    Name = DefaultConfigurationName.Name()
		                    }
	                    }
                    }, (SqlServerStorageOptions)null);

            system.ConfigurationStorage.Data.Single().ConnectionString
	            .Should().Contain("Hangfire");
        }

        [Test]
        public void ShouldQueryWorkerServersWithDefaultSqlStorageOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration{ ConnectionString =  @"Data Source=.;Initial Catalog=fakedb;" });

            system.WorkerServerQueries.QueryAllWorkerServers(null, new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});

            Assert.False(system.Hangfire.CreatedStorages.Single().SqlServerOptions.PrepareSchemaIfNecessary);
        }

        [Test]
        public void ShouldReturnStorageConfigurationId()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 3});

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (SqlServerStorageOptions)null).Single();

            Assert.AreEqual(3, workerServer.ConfigurationId);
        }

        [Test]
        public void ShouldReturnConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name"});

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (SqlServerStorageOptions)null).Single();

            Assert.AreEqual("name", workerServer.Name);
        }
    }
}