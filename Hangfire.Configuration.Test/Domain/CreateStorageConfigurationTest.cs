using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class CreateStorageConfigurationTest
    {
        [Fact]
        public void ShouldSaveNewStorageConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            var connectionString = "Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword";
            var schemaName = "awesomeSchema";

            configuration.CreateStorageConfiguration(new NewStorageConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaName = schemaName
            });

            var storedConfiguration = repository.Data.Single();
            Assert.Equal(connectionString, storedConfiguration.ConnectionString);
            Assert.Equal(schemaName, storedConfiguration.SchemaName);
        }

        [Fact]
        public void ShouldBeInactiveOnSave()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);

            configuration.CreateStorageConfiguration(new NewStorageConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaName = "awesomeSchema"
            });

            var storedConfiguration = repository.Data.Single();
            Assert.Equal(false, storedConfiguration.Active);
        }

        [Fact]
        public void ShouldActivate()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            repository.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = "connectionString",
                SchemaName = "awesomeSchema",
                Active = false
            });

            configuration.ActivateStorage(1);

            var storedConfiguration = repository.Data.Single();
            Assert.Equal(true, storedConfiguration.Active);
        }

        [Fact]
        public void ShouldDeactivatePreviouslyActive()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            repository.Has(
                new StoredConfiguration {Id = 1, Active = true, ConnectionString = "connectionString", SchemaName = "awesomeSchema"},
                new StoredConfiguration {Id = 2, ConnectionString = "connectionString2", SchemaName = "awesomeSchema2"}
            );

            configuration.ActivateStorage(2);

            Assert.Equal(false, repository.Data.Single(x => x.Id == 1).Active);
        }
    }
}