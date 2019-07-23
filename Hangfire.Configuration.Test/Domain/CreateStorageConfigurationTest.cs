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
        public void ShouldNotBeOverridenByDefaultConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            configuration.CreateStorageConfiguration(new NewStorageConfiguration
            {
                Server = "newStorageServer",
                SchemaName = "newSchemaName"
            });
            
            configuration.ConfigureDefaultStorage("defaultConnectionString", "defaultSchemaName");

            Assert.Contains("newStorageServer", repository.ReadConfigurations().First().ConnectionString);
        }

        [Fact]
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            configuration.CreateStorageConfiguration(new NewStorageConfiguration
            {
                Server = "newStorageServer",
                SchemaName = "newSchemaName"
            });
            configuration.ConfigureDefaultStorage("defaultConnectionString", "defaultSchemaName");
            
            configuration.WriteGoalWorkerCount(10);

            var config = repository.ReadConfigurations();
            Assert.Null(config.First().GoalWorkerCount);
        }
        
        [Fact]
        public void ShouldReadAllConfigurations()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            configuration.ConfigureDefaultStorage("defaultConnectionString", "defaultSchemaName");
            configuration.CreateStorageConfiguration(new NewStorageConfiguration
            {
                Server = "newStorageServer",
                SchemaName = "newSchemaName"
            });
            
            var configurations = repository.ReadConfigurations();

            Assert.Equal(2, configurations.Count());
        }
    }
}