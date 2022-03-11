using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class CreateServerConfigurationTest
    {
        [Test]
        public void ShouldSaveNewServerConfiguration()
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
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
            Assert.AreEqual("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword", storedConfiguration.ConnectionString);
            Assert.AreEqual("awesomeSchema", storedConfiguration.SchemaName);
        }

        [Test]
        public void ShouldSetGoalWorkerCountToDefaultConfiguration()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
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
            Assert.AreEqual(10, config.First().GoalWorkerCount);
        }

        [Test]
        public void ShouldReadAllConfigurations()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
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

            Assert.AreEqual(2, configurations.Count());
        }

        [Test]
        public void ShouldThrowWhenConnectionFails()
        {
            var system = new SystemUnderTest();
            system.SchemaCreator.TryConnectFailsWith = new Exception();

            Assert.Throws<Exception>(() => system.ConfigurationApi.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = "Server",
                    Database = "TestDatabase",
                    User = "testUser",
                    Password = "awesomePassword"
                }));
        }

        [Test]
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

            system.SchemaCreator.ConnectionTriedWith
	            .Should().Contain("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword");
        }

        [Test]
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

            system.SchemaCreator.ConnectionTriedWith
	            .Should().Contain("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=createUser;Password=createPassword");
        }

        [Test]
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
            
            system.SchemaCreator.Schemas.Last().ConnectionString
	            .Should().Contain("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=createUser;Password=createPassword");
            Assert.AreEqual("schema", system.SchemaCreator.Schemas.Last().SchemaName);
        }

        [Test]
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
            Assert.AreEqual("creator", system.SchemaCreator.Schemas.Last().ConnectionString);
            Assert.AreEqual("storage", storedConfiguration.ConnectionString);
            Assert.AreEqual("schema", storedConfiguration.SchemaName);
        }

        [Test]
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
            Assert.AreEqual("Schema already exists.", e.Message);
        }

        [Test]
        public void ShouldThrowWhenDefaultSchemaNameAlreadyExists()
        {
            var system = new SystemUnderTest();
            var connection = new SqlConnectionStringBuilder {DataSource = "_", InitialCatalog = "existingDatabase"}.ToString();
            
            system.SchemaCreator.Has(DefaultSchemaName.SqlServer(), connection);

            Assert.Throws<Exception>(() => system.ConfigurationApi.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    SchemaCreatorConnectionString = connection,
                    SchemaName = null
                }));
        }

        [Test]
        public void ShouldCreateSchemaWithSameNameInDifferentDatabase()
        {
            var system = new SystemUnderTest();
            system.SchemaCreator.Has("schemaName", "connectionOne");

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                SchemaCreatorConnectionString = "connectionTwo",
                SchemaName = "schemaName"
            });

            Assert.AreEqual(2, system.SchemaCreator.Schemas.Count());
            Assert.AreEqual("schemaName", system.SchemaCreator.Schemas.Last().SchemaName);
            Assert.AreEqual("connectionTwo", system.SchemaCreator.Schemas.Last().ConnectionString);
        }

        [Test]
        public void ShouldSaveNewServerConfigurationWithName()
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                Name = "namedConfiguration",
                StorageConnectionString = "storage",
                SchemaCreatorConnectionString = "creator",
                SchemaName = "schema"
            });

            var storedConfiguration = system.ConfigurationStorage.Data.Last();
            Assert.AreEqual("namedConfiguration", storedConfiguration.Name);
        }
        
        [Test]
        public void ShouldCreateWithDefaultSchemaName()
        {
	        var system = new SystemUnderTest();

	        system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
	        {
		        StorageConnectionString = "Data Source=.",
		        SchemaCreatorConnectionString = "Data Source=.",
		        SchemaName = null
	        });

	        var storedConfiguration = system.ConfigurationStorage.Data.Last();
	        Assert.AreEqual(DefaultSchemaName.SqlServer(), storedConfiguration.SchemaName);
        }
    }
}