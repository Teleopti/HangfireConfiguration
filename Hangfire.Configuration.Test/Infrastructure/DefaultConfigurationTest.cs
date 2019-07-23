using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("Infrastructure")]
    public class DefaultConfigurationTest
    {
        [Fact, CleanDatabase]
        public void ShouldReadEmptyConnectionString()
        {
            var connectionString = HangfireConfiguration.ReadActiveConfigurationConnectionString(ConnectionUtils.GetConnectionString());

            Assert.Null(connectionString);
        }

        [Fact, CleanDatabase]
        public void ShouldSaveConnectionString()
        {
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);

            var connectionString = HangfireConfiguration.ReadActiveConfigurationConnectionString(ConnectionUtils.GetConnectionString());
            Assert.Equal("connectionString", connectionString);
        }

        [Fact, CleanDatabase]
        public void ShouldReadConnectionString()
        {
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);

            var connectionString = HangfireConfiguration.ReadActiveConfigurationConnectionString(ConnectionUtils.GetConnectionString());

            Assert.Equal("connectionString", connectionString);
        }

        [Fact, CleanDatabase]
        public void ShouldUpdateWithConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteConfiguration(new StoredConfiguration());

            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);

            var connectionString = HangfireConfiguration.ReadActiveConfigurationConnectionString(ConnectionUtils.GetConnectionString());
            Assert.Equal("connectionString", connectionString);
        }

        [Fact, CleanDatabase]
        public void ShouldReadEmptyDefaultConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());

            Assert.Equal(0, repository.ReadConfigurations().Count());
        }

        [Fact, CleanDatabase]
        public void ShouldReadDefaultConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});
            repository.WriteDefaultConfiguration("connectionString", "schemaName");

            var result = repository.ReadConfigurations();

            Assert.Equal(1, result.Single().Id);
            Assert.Equal("connectionString", result.Single().ConnectionString);
            Assert.Equal("schemaName", result.Single().SchemaName);
            Assert.Equal(1, result.Single().GoalWorkerCount);
            Assert.Equal(true, result.Single().Active);
        }

        [Fact, CleanDatabase]
        public void ShouldActivateOnSave()
        {
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);

            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            Assert.True(repository.ReadConfigurations().Single().Active);
        }

        [Fact, CleanDatabase]
        public void ShouldActivateOnUpdateWithConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteConfiguration(new StoredConfiguration());

            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);

            Assert.True(repository.ReadConfigurations().Single().Active);
        }

        [Fact, CleanDatabase]
        public void ShouldSaveSchemaName()
        {
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", "schemaName");

            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            var schemaName = repository.ReadConfigurations().Single().SchemaName;
            Assert.Equal("schemaName", schemaName);
        }

        [Fact, CleanDatabase]
        public void ShouldSaveSchemaNameOnUpdate()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteConfiguration(new StoredConfiguration());

            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", "schemaName");

            var schemaName = repository.ReadConfigurations().Single().SchemaName;
            Assert.Equal("schemaName", schemaName);
        }

        [Fact, CleanDatabase]
        public void ShouldReadActiveConfigurationConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteNewStorageConfiguration("newConfigurationConnectionString", "newSchemaName", false);
            repository.WriteDefaultConfiguration("connectionString", "schemaName");

            var result = HangfireConfiguration.ReadActiveConfigurationConnectionString(ConnectionUtils.GetConnectionString());

            Assert.Equal("connectionString", result);
        }
    }
}