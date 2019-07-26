using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class CreateStorageConfigurationTest
    {
        [Fact]
        public void ShouldSaveNewStorageConfiguration()
        {
            
            var system = new SystemUnderTest(); 
            
            var connectionString = "Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword";
            var schemaName = "awesomeSchema";

            system.Configuration.CreateStorageConfiguration(new NewStorageConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaName = schemaName
            });

            var storedConfiguration = system.Repository.Data.Single();
            Assert.Equal(connectionString, storedConfiguration.ConnectionString);
            Assert.Equal(schemaName, storedConfiguration.SchemaName);
        }

        [Fact]
        public void ShouldNotBeOverridenByDefaultConfiguration()
        {
            var system = new SystemUnderTest();
            system.Configuration.CreateStorageConfiguration(new NewStorageConfiguration
            {
                Server = "newStorageServer",
                SchemaName = "newSchemaName"
            });
            
            system.Configuration.ConfigureDefaultStorage("defaultConnectionString", "defaultSchemaName");

            Assert.Contains("newStorageServer", system.Repository.ReadConfigurations().First().ConnectionString);
        }

        [Fact]
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var system = new SystemUnderTest();
            system.Configuration.ConfigureDefaultStorage("defaultConnectionString", "defaultSchemaName");
            system.Configuration.CreateStorageConfiguration(new NewStorageConfiguration
            {
                Server = "newStorageServer",
                SchemaName = "newSchemaName"
            });
            
            system.Configuration.WriteGoalWorkerCount(10);

            var config = system.Repository.ReadConfigurations();
            Assert.Null(config.First().GoalWorkerCount);
        }
        
        [Fact]
        public void ShouldReadAllConfigurations()
        {
            var system = new SystemUnderTest();
            system.Configuration.ConfigureDefaultStorage("defaultConnectionString", "defaultSchemaName");
            system.Configuration.CreateStorageConfiguration(new NewStorageConfiguration
            {
                Server = "newStorageServer",
                SchemaName = "newSchemaName"
            });
            
            var configurations = system.Repository.ReadConfigurations();

            Assert.Equal(2, configurations.Count());
        }
    }
}