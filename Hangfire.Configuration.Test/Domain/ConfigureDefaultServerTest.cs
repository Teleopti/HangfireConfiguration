using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureDefaultServerTest
    {
        [Fact]
        public void ShouldConfigureDefaultServer()
        {
            var system = new SystemUnderTest();

            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString"
            }, null, null);

            Assert.Equal("connectionString", system.Repository.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldNotConfigureDefaultServerIfNoneGiven()
        {
            var system = new SystemUnderTest();
            
            system.ServerStarter.StartServers(new ConfigurationOptions(), null, null);

            Assert.Empty(system.Repository.Data);
        }

        [Fact]
        public void ShouldUpdateLegacyConfigurationWithDefaultConnectionString()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {GoalWorkerCount = 54});

            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString"
            }, null, null);

            Assert.Equal("connectionString", system.Repository.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldActivateDefault()
        {
            var system = new SystemUnderTest();

            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString"
            }, null, null);

            Assert.True(system.Repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldActivateLegacyConfigurationWhenConfiguredAsDefault()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {GoalWorkerCount = 4});

            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString"
            }, null, null);

            Assert.True(system.Repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldSaveSchemaName()
        {
            var system = new SystemUnderTest();
            
            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString",
                DefaultSchemaName = "schemaName"
            }, null, null);

            Assert.Equal("schemaName", system.Repository.Data.Single().SchemaName);
        }

        [Fact]
        public void ShouldSaveSchemaNameOnLegacyConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {GoalWorkerCount = 4});

            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "connectionString",
                DefaultSchemaName = "schemaName"
            }, null, null);

            Assert.Equal("schemaName", system.Repository.Data.Single().SchemaName);
        }

        [Fact]
        public void ShouldUpdateConfigurationIfAlreadyExists()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = "existingDefault"});

            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "newDefault"
            }, null, null);

            Assert.Equal("newDefault", system.Repository.Data.Single().ConnectionString);
        }
        
        [Fact]
        public void ShouldNotActivateOnUpdatingDefault()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = "existingDefault", Active = false});
            system.Repository.Has(new StoredConfiguration {ConnectionString = "newStorageConnection", Active = true});

            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "newDefault"
            }, null, null);

            Assert.False(system.Repository.Data.First().Active);
            Assert.True(system.Repository.Data.Last().Active);
        }
    }
}