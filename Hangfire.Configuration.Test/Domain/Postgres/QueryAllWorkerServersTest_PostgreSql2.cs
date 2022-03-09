using System.Linq;
using Hangfire.PostgreSql;
using Npgsql;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain.Postgres
{
    public class QueryAllWorkerServersTest
    {
        [Test]
        public void ShouldQueryWorkerServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServers = system.WorkerServerQueries.QueryAllWorkerServers(null, (PostgreSqlStorageOptions)null);

            Assert.NotNull(workerServers.Single());
        }

        [Test]
        public void ShouldReturnWorkerServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (PostgreSqlStorageOptions)null).Single();

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
			                    ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "Hangfire" }.ToString(),
			                    Name = DefaultConfigurationName.Name()
		                    }
	                    }
                    }, (PostgreSqlStorageOptions)null);

            system.ConfigurationStorage.Data.Single().ConnectionString
	            .Should().Contain("Hangfire");
        }

        [Test]
        public void ShouldQueryWorkerServersWithDefaultSqlStorageOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration{ConnectionString = @"Host=localhost;Database=fakedb;"});

            system.WorkerServerQueries.QueryAllWorkerServers(null, new PostgreSqlStorageOptions { PrepareSchemaIfNecessary = false});

            Assert.False(system.Hangfire.CreatedStorages.Single().PostgresOptions.PrepareSchemaIfNecessary);
        }

        [Test]
        public void ShouldReturnStorageConfigurationId()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 3});

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (PostgreSqlStorageOptions)null).Single();

            Assert.AreEqual(3, workerServer.ConfigurationId);
        }

        [Test]
        public void ShouldReturnConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name"});

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (PostgreSqlStorageOptions)null).Single();

            Assert.AreEqual("name", workerServer.Name);
        }
    }
}