using System.Linq;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class QueryAllWorkerServersTest
    {
        [Test]
        public void ShouldQueryWorkerServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServers = system.WorkerServerQueries.QueryAllWorkerServers();

            Assert.NotNull(workerServers.Single());
        }

        [Test]
        public void ShouldReturnWorkerServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers().Single();

            workerServer.JobStorage
	            .Should().Be.SameInstanceAs(system.Hangfire.CreatedStorages.Single());
        }

        [Test]
        public void ShouldAutoUpdate()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.UseOptions(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = @"Data Source=.;Initial Catalog=Hangfire;",
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            });
            system.WorkerServerQueries.QueryAllWorkerServers();

            system.ConfigurationStorage.Data.Single().ConnectionString
	            .Should().Contain("Hangfire");
        }

        [Test]
        public void ShouldQueryWorkerServersWithDefaultSqlStorageOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = @"Data Source=.;Initial Catalog=fakedb;" });

            system.UseStorageOptions(new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});
            system.WorkerServerQueries.QueryAllWorkerServers();

            Assert.False(system.Hangfire.CreatedStorages.Single().SqlServerOptions.PrepareSchemaIfNecessary);
        }

        [Test]
        public void ShouldQueryWorkerServersWithDefaultPostgresStorageOptions()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString =  @"Host=localhost;Database=fakedb;" });

	        system.UseStorageOptions(new PostgreSqlStorageOptions {PrepareSchemaIfNecessary = false});
	        system.WorkerServerQueries.QueryAllWorkerServers();

	        Assert.False(system.Hangfire.CreatedStorages.Single().PostgresOptions.PrepareSchemaIfNecessary);
        }
        
        [Test]
        public void ShouldReturnStorageConfigurationId()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 3});

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers().Single();

            Assert.AreEqual(3, workerServer.ConfigurationId);
        }

        [Test]
        public void ShouldReturnConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name"});

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers().Single();

            Assert.AreEqual("name", workerServer.Name);
        }
    }
}