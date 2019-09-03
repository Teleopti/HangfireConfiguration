using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("Infrastructure")]
    public class ConfigurationRepositoryTest
    {
        [Fact, CleanDatabase]
        public void ShouldReadEmptyConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());

            Assert.Empty(repository.ReadConfigurations());
        }

        [Fact, CleanDatabase]
        public void ShouldWrite()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());

            repository.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = "connection string",
                SchemaName = "schema name",
                Active = false
            });

            var configuration = repository.ReadConfigurations().Single();
            Assert.Equal("connection string", configuration.ConnectionString);
            Assert.Equal("schema name", configuration.SchemaName);
            Assert.Equal(false, configuration.Active);
        }

        [Fact, CleanDatabase]
        public void ShouldRead()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = "connectionString",
                SchemaName = "schemaName",
                GoalWorkerCount = 3,
                Active = true
            });

            var result = repository.ReadConfigurations().Single();

            Assert.Equal(1, result.Id);
            Assert.Equal("connectionString", result.ConnectionString);
            Assert.Equal("schemaName", result.SchemaName);
            Assert.Equal(3, result.GoalWorkerCount);
            Assert.Equal(true, result.Active);
        }

        [Fact, CleanDatabase]
        public void ShouldNotInsertMultiple()
        {
            int[] arr = Enumerable.Range(1, 10).ToArray();

            Parallel.ForEach(arr, (item) =>
            {
                var connection = ConnectionUtils.GetConnectionString();
                var repository = new ConfigurationRepository(connection);
                var configurator = new DefaultServerConfigurator(repository, new DistributedLock("lockid", connection));
                configurator.Configure( new ConfigurationOptions
                {
                    AutoUpdatedHangfireConnectionString = connection,
                    AutoUpdatedHangfireSchemaName = "SchemaName"
                });
            });

            Assert.Single(new ConfigurationRepository(ConnectionUtils.GetConnectionString()).ReadConfigurations());
        }
    }
}