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

            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaCreatorUser = "createuser",
                SchemaCreatorPassword = "createPassword",
                SchemaName = "awesomeSchema"
            });

            var storedConfiguration = system.Repository.Data.Last();
            Assert.Equal("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword", storedConfiguration.ConnectionString);
            Assert.Equal("awesomeSchema", storedConfiguration.SchemaName);
        }

        [Fact]
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = "defaultConnectionString",
                AutoUpdatedHangfireSchemaName = "defaultSchemaName"
            }, null, null);
            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                SchemaName = "newSchemaName",
                Database = "database",
                User = "user",
                Password = "Password",
                SchemaCreatorUser = "createUser",
                SchemaCreatorPassword = "createPassword",
            });

            system.Configuration.WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = 10});

            var config = system.Repository.ReadConfigurations();
            Assert.Equal(10, config.First().GoalWorkerCount);
        }

        [Fact]
        public void ShouldReadAllConfigurations()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = "defaultConnectionString",
                AutoUpdatedHangfireSchemaName = "defaultSchemaName"
            }, null, null);
            system.Configuration.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                Database = "database",
                User = "user",
                Password = "Password",
                SchemaCreatorUser = "createUser",
                SchemaCreatorPassword = "createPassword",
                SchemaName = "newSchemaName"
            });

            var configurations = system.Repository.ReadConfigurations();

            Assert.Equal(2, configurations.Count());
        }

        [Fact]
        public void ShouldThrowWhenConnectionFails()
        {
            var system = new SystemUnderTest();
            system.SchemaCreator.TryConnectFailsWith = new Exception();

            Assert.ThrowsAny<Exception>(() => system.Configuration.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = "Server",
                    Database = "TestDatabase",
                    User = "testUser",
                    Password = "awesomePassword"
                }));
        }

        [Fact]
        public void ShouldTryConnectWithStorageConnectionString()
        {
            var system = new SystemUnderTest();

            system.Configuration.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = "AwesomeServer",
                    Database = "TestDatabase",
                    User = "testUser",
                    Password = "awesomePassword"
                });

            Assert.Contains("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword", system.SchemaCreator.ConnectionTriedWith);
        }

        [Fact]
        public void ShouldTryConnectWithCreatorConnectionString()
        {
            var system = new SystemUnderTest();

            system.Configuration.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = "AwesomeServer",
                    Database = "TestDatabase",
                    SchemaCreatorUser = "createUser",
                    SchemaCreatorPassword = "createPassword"
                });

            Assert.Contains("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=createUser;Password=createPassword", system.SchemaCreator.ConnectionTriedWith);
        }

        [Fact]
        public void ShouldCreateSchemaInDatabaseWithGivenConnectionsString()
        {
            var system = new SystemUnderTest();

            system.Configuration.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = "AwesomeServer",
                    Database = "TestDatabase",
                    SchemaCreatorUser = "createUser",
                    SchemaCreatorPassword = "createPassword",
                    SchemaName = "schema"
                });

            Assert.Contains("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=createUser;Password=createPassword", system.SchemaCreator.SchemaCreatedWith.Last().ConnectionString);
            Assert.Equal("schema", system.SchemaCreator.SchemaCreatedWith.Last().SchemaName);
        }
    }
}