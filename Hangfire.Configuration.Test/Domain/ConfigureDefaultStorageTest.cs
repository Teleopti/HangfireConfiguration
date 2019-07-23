using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureDefaultStorageTest
    {
        [Fact]
        public void ShouldConfigureDefaultStorage()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);

            configuration.ConfigureDefaultStorage("connectionString", null);

            Assert.Equal("connectionString", repository.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldUpdateLegacyConfigurationWithDefaultConnectionString()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {GoalWorkerCount = 54});
            var configuration = new Configuration(repository);

            configuration.ConfigureDefaultStorage("connectionString", null);

            Assert.Equal("connectionString", repository.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldActivateDefault()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);

            configuration.ConfigureDefaultStorage("connectionString", null);

            Assert.True(repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldActivateLegacyConfigurationWhenConfiguredAsDefault()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            repository.Has(new StoredConfiguration {GoalWorkerCount = 4});

            configuration.ConfigureDefaultStorage("connectionString", null);

            Assert.True(repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldSaveSchemaName()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);

            configuration.ConfigureDefaultStorage("connectionString", "schemaName");

            Assert.Equal("schemaName", repository.Data.Single().SchemaName);
        }

        [Fact]
        public void ShouldSaveSchemaNameOnLegacyConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            repository.Has(new StoredConfiguration {GoalWorkerCount = 4});

            configuration.ConfigureDefaultStorage("connectionString", "schemaName");

            Assert.Equal("schemaName", repository.Data.Single().SchemaName);
        }
    }
}