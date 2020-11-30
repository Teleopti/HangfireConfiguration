using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("NotParallel")]
    public class ConfigurationStorageTest
    {
        [Fact, CleanDatabase]
        public void ShouldReadEmptyConfiguration()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());

            Assert.Empty(storage.ReadConfigurations());
        }

        [Fact, CleanDatabase]
        public void ShouldWrite()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());

            storage.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = "connection string",
                SchemaName = "schema name",
                Active = false
            });

            var configuration = storage.ReadConfigurations().Single();
            Assert.Equal("connection string", configuration.ConnectionString);
            Assert.Equal("schema name", configuration.SchemaName);
            Assert.Equal(false, configuration.Active);
        }

        [Fact, CleanDatabase]
        public void ShouldRead()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());
            storage.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = "connectionString",
                SchemaName = "schemaName",
                GoalWorkerCount = 3,
                Active = true
            });

            var result = storage.ReadConfigurations().Single();

            Assert.Equal(1, result.Id);
            Assert.Equal("connectionString", result.ConnectionString);
            Assert.Equal("schemaName", result.SchemaName);
            Assert.Equal(3, result.GoalWorkerCount);
            Assert.Equal(true, result.Active);
        }

        [Fact, CleanDatabase]
        public void ShouldUpdate()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());
            storage.WriteConfiguration(new StoredConfiguration());

            var existing = storage.ReadConfigurations().Single();
            existing.ConnectionString = "connection";
            existing.SchemaName = "schema";
            existing.GoalWorkerCount = 23;
            existing.Active = true;
            storage.WriteConfiguration(existing);

            var configuration = storage.ReadConfigurations().Single();
            Assert.Equal("connection", configuration.ConnectionString);
            Assert.Equal("schema", configuration.SchemaName);
            Assert.Equal(23, configuration.GoalWorkerCount);
            Assert.Equal(true, configuration.Active);
        }

        [Fact, CleanDatabase]
        public void ShouldWriteName()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());

            storage.WriteConfiguration(new StoredConfiguration
            {
                Name = "name",
            });

            var configuration = storage.ReadConfigurations().Single();
            Assert.Equal("name", configuration.Name);
        }

        [Fact, CleanDatabase]
        public void ShouldUpdateName()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());
            storage.WriteConfiguration(new StoredConfiguration());

            var existing = storage.ReadConfigurations().Single();
            existing.Name = "name";
            storage.WriteConfiguration(existing);

            var configuration = storage.ReadConfigurations().Single();
            Assert.Equal("name", configuration.Name);
        }
    }
}