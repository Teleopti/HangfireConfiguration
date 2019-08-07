using System;
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
                UserForCreate = "createuser",
                PasswordForCreate = "createPassword",
                SchemaName = schemaName
            });

            var storedConfiguration = system.Repository.Data.Last();
            Assert.Equal(connectionString, storedConfiguration.ConnectionString);
            Assert.Equal(schemaName, storedConfiguration.SchemaName);
        }

        [Fact]
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var system = new SystemUnderTest();
            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "defaultConnectionString",
                DefaultSchemaName = "defaultSchemaName"
            }, null, null);
            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                SchemaName = "newSchemaName",
                Database = "database",
                User = "user",
                Password = "Password",
                UserForCreate = "createUser",
                PasswordForCreate = "createPassword",                
            });

            system.Configuration.WriteGoalWorkerCount(10);

            var config = system.Repository.ReadConfigurations();
            Assert.Equal(10, config.First().GoalWorkerCount);
        }

        [Fact]
        public void ShouldReadAllConfigurations()
        {
            var system = new SystemUnderTest();
            system.ServerStarter.StartServers(new ConfigurationOptions
            {
                DefaultHangfireConnectionString = "defaultConnectionString",
                DefaultSchemaName = "defaultSchemaName"
            }, null, null);
            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                Database = "database",
                User = "user",
                Password = "Password",
                UserForCreate = "createUser",
                PasswordForCreate = "createPassword",
                SchemaName = "newSchemaName"
            });

            var configurations = system.Repository.ReadConfigurations();

            Assert.Equal(2, configurations.Count());
        }

        [Fact]
        public void ShouldSaveEmptyDefault()
        {
            var system = new SystemUnderTest();
            
            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                UserForCreate = "createUser",
                PasswordForCreate = "createPassword",
                SchemaName = "awesomeSchema"
            });

            var storedConfigurations = system.Repository.Data.ToArray();
            Assert.Equal(2, storedConfigurations.Count());
            Assert.Null(storedConfigurations.First().ConnectionString);
            Assert.Null(storedConfigurations.First().SchemaName);
            Assert.Null(storedConfigurations.First().GoalWorkerCount);
            Assert.Null(storedConfigurations.First().Active);
        }

        [Fact]
        public void ShouldThrowIfAnyNull()
        {
            var system = new SystemUnderTest();
            
            Assert.ThrowsAny<Exception>(() => system.Configuration.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = null,
                    Database = "TestDatabase",
                    User = "testUser",
                    Password = "awesomePassword",
                    UserForCreate = "createUser",
                    PasswordForCreate = "createPassword",
                }));
        }
    }
}