using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    public class NewStorageConfigurationTest
    {
        [Fact, CleanDatabase]
        public void ShouldWrite()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			
            repository.WriteNewStorageConfiguration("connection string", "schema name", false);

            var configuration = repository.ReadConfiguration();
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

            var configuration = repository.ReadConfiguration();
            Assert.Equal("newConnectionString", configuration.First().ConnectionString);
        }

        [Fact, CleanDatabase]
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteNewStorageConfiguration("newConnectionString", "newSchemaName", false);
            repository.WriteDefaultConfiguration("defaultConnectionString", "defaultSchemaName");
            
            repository.WriteGoalWorkerCount(10);

            var configuration = repository.ReadConfiguration();
            Assert.Null(configuration.First().GoalWorkerCount);
        }
        
        [Fact, CleanDatabase]
        public void ShouldReadAllConfigurations()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteDefaultConfiguration("defaultConnectionString", "defaultSchemaName");
            repository.WriteNewStorageConfiguration("newConnectionString", "newSchemaName", false);
            
            var configurations = repository.ReadConfiguration();

            Assert.Equal(2, configurations.Count());
        }
    }
}