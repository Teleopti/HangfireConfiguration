using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.SqlServer;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class QueryAllWorkerServersTest
    {
        [Fact]
        public void ShouldQueryWorkerServers()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            var workerServers = system.WorkerServerQueries.QueryAllWorkerServers(null, null);

            Assert.NotNull(workerServers.Single());
        }
        
        [Fact]
        public void ShouldReturnWorkerServer()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            var workerServer = system.WorkerServerQueries.QueryAllWorkerServers(null, null).Single();

            Assert.Same(system.Hangfire.CreatedStorages.Single(), workerServer.JobStorage);
        }
        
        [Fact]
        public void ShouldAutoUpdate()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            var workerServers = system.WorkerServerQueries
                .QueryAllWorkerServers(
                new ConfigurationOptions
                {
                    AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "Hangfire"}.ToString()
                }, null);

            Assert.Contains("Hangfire", system.Repository.Data.Single().ConnectionString);
        }
        
        [Fact]
        public void ShouldQueryWorkerServersWithDefaultSqlStorageOptions()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            var workerServers = system.WorkerServerQueries.QueryAllWorkerServers(null, new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});

            Assert.False(system.Hangfire.CreatedStorages.Single().Options.PrepareSchemaIfNecessary);
        }
    }
}