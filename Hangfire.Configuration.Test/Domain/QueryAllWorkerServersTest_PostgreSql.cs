using System.Linq;
using Hangfire.PostgreSql;
using Npgsql;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class QueryAllWorkerServersTest
    {
        [Fact]
        public void ShouldQueryWorkerServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServers = system.WorkerServerQueries.QueryAllWorkerServers(null, (PostgreSqlStorageOptions)null);

            Assert.NotNull(workerServers.Single());
        }

        [Fact]
        public void ShouldReturnWorkerServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (PostgreSqlStorageOptions)null).Single();

            Assert.Same(system.Hangfire.CreatedStorages.Single(), workerServer.JobStorage);
        }

        [Fact]
        public void ShouldAutoUpdate()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServers = system.WorkerServerQueries
                .QueryAllWorkerServers(
                    new ConfigurationOptions
                    {
	                    UpdateConfigurations = new []
	                    {
		                    new UpdateConfiguration
		                    {
			                    ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "Hangfire" }.ToString(),
			                    Name = DefaultConfigurationName.Name()
		                    }
	                    }
                    }, (PostgreSqlStorageOptions)null);

            Assert.Contains("Hangfire", system.ConfigurationStorage.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldQueryWorkerServersWithDefaultSqlStorageOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration{ConnectionString = ConnectionUtils.GetFakeConnectionString()});

            var workerServers = system.WorkerServerQueries.QueryAllWorkerServers(null, new PostgreSqlStorageOptions { PrepareSchemaIfNecessary = false});

            Assert.False(system.Hangfire.CreatedStorages.Single().Options.PrepareSchemaIfNecessary);
        }

        [Fact]
        public void ShouldReturnStorageConfigurationId()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 3});

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (PostgreSqlStorageOptions)null).Single();

            Assert.Equal(3, workerServer.ConfigurationId);
        }

        [Fact]
        public void ShouldReturnConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name"});

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, (PostgreSqlStorageOptions)null).Single();

            Assert.Equal("name", workerServer.Name);
        }
    }
}