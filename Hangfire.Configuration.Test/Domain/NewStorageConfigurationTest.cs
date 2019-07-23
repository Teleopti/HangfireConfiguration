using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class NewStorageConfigurationTest
    {
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