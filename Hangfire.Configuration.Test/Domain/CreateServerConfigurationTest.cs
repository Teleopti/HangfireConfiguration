using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class CreateServerConfigurationTest
    {
        [Fact]
        public void ShouldSaveNewServerConfiguration()
        {
            
            var system = new SystemUnderTest(); 
            
            var connectionString = "Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword";
            var schemaName = "awesomeSchema";

            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration()
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
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var system = new SystemUnderTest();
            system.ServerStarter.StartServers(new ConfigurationOptions {
                DefaultHangfireConnectionString = "defaultConnectionString",
                DefaultSchemaName = "defaultSchemaName"
            }, null, null);
            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
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
            system.ServerStarter.StartServers(new ConfigurationOptions {
                DefaultHangfireConnectionString = "defaultConnectionString",
                DefaultSchemaName = "defaultSchemaName"
            }, null, null);
            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                SchemaName = "newSchemaName"
            });
            
            var configurations = system.Repository.ReadConfigurations();

            Assert.Equal(2, configurations.Count());
        }
    }
}