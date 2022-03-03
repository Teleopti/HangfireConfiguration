using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using Xunit;
using Xunit.Sdk;

namespace Hangfire.Configuration.Test.Domain
{
    public class CreateServerConfigurationTest
    {
        [Fact]
        public void ShouldSaveNewServerConfiguration()
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaCreatorUser = "createuser",
                SchemaCreatorPassword = "createPassword",
                SchemaName = "awesomeSchema"
            });

            var storedConfiguration = system.ConfigurationStorage.Data.Last();
            Assert.Equal("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword", storedConfiguration.ConnectionString);
            Assert.Equal("awesomeSchema", storedConfiguration.SchemaName);
        }

        [Fact]
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name(),
			            SchemaName = "defaultSchemaName"
		            }
	            }
            }, null, (SqlServerStorageOptions)null);
            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                SchemaName = "newSchemaName",
                Database = "database",
                User = "user",
                Password = "Password",
                SchemaCreatorUser = "createUser",
                SchemaCreatorPassword = "createPassword",
            });

            system.ConfigurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = 10});

            var config = system.ConfigurationStorage.ReadConfigurations();
            Assert.Equal(10, config.First().GoalWorkerCount);
        }

        [Fact]
        public void ShouldReadAllConfigurations()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name(),
			            SchemaName = "defaultSchemaName"
		            }
	            }
            }, null, (SqlServerStorageOptions)null);
            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                Database = "database",
                User = "user",
                Password = "Password",
                SchemaCreatorUser = "createUser",
                SchemaCreatorPassword = "createPassword",
                SchemaName = "newSchemaName"
            });

            var configurations = system.ConfigurationStorage.ReadConfigurations();

            Assert.Equal(2, configurations.Count());
        }

        [Fact]
        public void ShouldThrowWhenConnectionFails()
        {
            var system = new SystemUnderTest();
            system.SchemaCreator.TryConnectFailsWith = new Exception();

            Assert.ThrowsAny<Exception>(() => system.ConfigurationApi.CreateServerConfiguration(
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

            system.ConfigurationApi.CreateServerConfiguration(
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

            system.ConfigurationApi.CreateServerConfiguration(
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

            system.ConfigurationApi.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = "AwesomeServer",
                    Database = "TestDatabase",
                    SchemaCreatorUser = "createUser",
                    SchemaCreatorPassword = "createPassword",
                    SchemaName = "schema"
                });

            Assert.Contains("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=createUser;Password=createPassword", system.SchemaCreator.Schemas.Last().ConnectionString);
            Assert.Equal("schema", system.SchemaCreator.Schemas.Last().SchemaName);
        }

        [Fact]
        public void ShouldSaveNewServerConfigurationUsingConnectionStrings()
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                StorageConnectionString = "storage",
                SchemaCreatorConnectionString = "creator",
                SchemaName = "schema"
            });

            var storedConfiguration = system.ConfigurationStorage.Data.Last();
            Assert.Equal("creator", system.SchemaCreator.Schemas.Last().ConnectionString);
            Assert.Equal("storage", storedConfiguration.ConnectionString);
            Assert.Equal("schema", storedConfiguration.SchemaName);
        }

        [Fact]
        public void ShouldThrowWhenSchemaAlreadyExists()
        {
            var system = new SystemUnderTest();
            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "server",
                Database = "existingDatabase",
                SchemaName = "existingSchema"
            });

            var e = Assert.Throws<Exception>(() => system.ConfigurationApi.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = "server",
                    Database = "existingDatabase",
                    SchemaName = "existingSchema"
                }));
            Assert.Equal("Schema already exists.", e.Message);
        }

        [Fact]
        public void ShouldThrowWhenDefaultSchemaNameAlreadyExists()
        {
            var system = new SystemUnderTest();
            var connection = new SqlConnectionStringBuilder {DataSource = "_", InitialCatalog = "existingDatabase"}.ToString();
            system.SchemaCreator.Has(DefaultSchemaName.Name(connection), connection);

            Assert.Throws<Exception>(() => system.ConfigurationApi.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    SchemaCreatorConnectionString = connection,
                    SchemaName = null
                }));
        }

        [Fact]
        public void ShouldCreateSchemaWithSameNameInDifferentDatabase()
        {
            var system = new SystemUnderTest();
            system.SchemaCreator.Has("schemaName", "connectionOne");

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                SchemaCreatorConnectionString = "connectionTwo",
                SchemaName = "schemaName"
            });

            Assert.Equal(2, system.SchemaCreator.Schemas.Count());
            Assert.Equal("schemaName", system.SchemaCreator.Schemas.Last().SchemaName);
            Assert.Equal("connectionTwo", system.SchemaCreator.Schemas.Last().ConnectionString);
        }

        [Fact]
        public void ShouldSaveNewServerConfigurationWithName()
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration()
            {
                Name = "namedConfiguration",
                StorageConnectionString = "storage",
                SchemaCreatorConnectionString = "creator",
                SchemaName = "schema"
            });

            var storedConfiguration = system.ConfigurationStorage.Data.Last();
            Assert.Equal("namedConfiguration", storedConfiguration.Name);
        }
    }
}