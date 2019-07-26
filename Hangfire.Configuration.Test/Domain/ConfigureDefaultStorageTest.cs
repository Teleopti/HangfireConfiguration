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
            var target = new ServerStarter(null, new Configuration(repository), new FakeHangfire());

            target.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString"
            }, null, null);

            Assert.Equal("connectionString", repository.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldNotConfigureDefaultStorageIfNoneGiven()
        {
            var repository = new FakeConfigurationRepository();
            var target = new ServerStarter(null, new Configuration(repository), new FakeHangfire());

            target.StartServers(new ConfigurationOptions(), null, null);

            Assert.Empty(repository.Data);
        }

        [Fact]
        public void ShouldUpdateLegacyConfigurationWithDefaultConnectionString()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {GoalWorkerCount = 54});
            var target = new ServerStarter(null, new Configuration(repository), new FakeHangfire());

            target.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString"
            }, null, null);

            Assert.Equal("connectionString", repository.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldActivateDefault()
        {
            var repository = new FakeConfigurationRepository();
            var target = new ServerStarter(null, new Configuration(repository), new FakeHangfire());

            target.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString"
            }, null, null);

            Assert.True(repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldActivateLegacyConfigurationWhenConfiguredAsDefault()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {GoalWorkerCount = 4});
            var target = new ServerStarter(null, new Configuration(repository), new FakeHangfire());

            target.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString"
            }, null, null);

            Assert.True(repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldSaveSchemaName()
        {
            var repository = new FakeConfigurationRepository();
            var target = new ServerStarter(null, new Configuration(repository), new FakeHangfire());

            target.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString",
                DefaultSchemaName = "schemaName"
            }, null, null);

            Assert.Equal("schemaName", repository.Data.Single().SchemaName);
        }

        [Fact]
        public void ShouldSaveSchemaNameOnLegacyConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {GoalWorkerCount = 4});
            var target = new ServerStarter(null, new Configuration(repository), new FakeHangfire());

            target.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString",
                DefaultSchemaName = "schemaName"
            }, null, null);

            Assert.Equal("schemaName", repository.Data.Single().SchemaName);
        }

        [Fact]
        public void ShouldNotConfigureDefaultStorageIfAlreadyExists()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {ConnectionString = "existingDefault"});
            var target = new ServerStarter(null, new Configuration(repository), new FakeHangfire());

            target.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "newDefault"
            }, null, null);

            Assert.Equal("existingDefault", repository.Data.Single().ConnectionString);
        }
    }
}