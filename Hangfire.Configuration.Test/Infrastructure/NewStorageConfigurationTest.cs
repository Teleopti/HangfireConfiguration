using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("Infrastructure")]
    public class NewStorageConfigurationTest
    {
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

            var configuration = repository.ReadConfigurations();
            Assert.Equal("connection string", configuration.Single().ConnectionString);
            Assert.Equal("schema name", configuration.Single().SchemaName);
            Assert.Equal(false, configuration.Single().Active);
        }
        
        [Fact, CleanDatabase]
        public void ShouldNotBeOverridenByDefaultConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteNewStorageConfiguration("newConnectionString", "newSchemaName", false);
            
            repository.WriteDefaultConfiguration("defaultConnectionString", "defaultSchemaName");

            var configuration = repository.ReadConfigurations();
            Assert.Equal("newConnectionString", configuration.First().ConnectionString);
        }

        [Fact, CleanDatabase]
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteNewStorageConfiguration("newConnectionString", "newSchemaName", false);
            repository.WriteDefaultConfiguration("defaultConnectionString", "defaultSchemaName");
            var configuration = new Configuration(repository);
            
            configuration.WriteGoalWorkerCount(10);

            var config = repository.ReadConfigurations();
            Assert.Null(config.First().GoalWorkerCount);
        }
        
        [Fact, CleanDatabase]
        public void ShouldReadAllConfigurations()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteDefaultConfiguration("defaultConnectionString", "defaultSchemaName");
            repository.WriteNewStorageConfiguration("newConnectionString", "newSchemaName", false);
            
            var configurations = repository.ReadConfigurations();

            Assert.Equal(2, configurations.Count());
        }
    }
}