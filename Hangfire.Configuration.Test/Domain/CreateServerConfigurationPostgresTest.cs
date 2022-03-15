using System;
using System.Linq;
using Hangfire.PostgreSql;
using Npgsql;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class CreateServerConfigurationPostgresTest
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
                SchemaName = "awesomeSchema",
				DatabaseProvider = "PostgreSql"
			});

            var storedConfiguration = system.ConfigurationStorage.Data.Last();
            Assert.AreEqual(@"Host=AwesomeServer;Database=TestDatabase;Username=testUser;Password=awesomePassword", storedConfiguration.ConnectionString);
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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name(),
			            SchemaName = "defaultSchemaName"
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);
            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                SchemaName = "newSchemaName",
                Database = "database",
                User = "user",
                Password = "Password",
                SchemaCreatorUser = "createUser",
                SchemaCreatorPassword = "createPassword",
                DatabaseProvider = "PostgreSql"
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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name(),
			            SchemaName = "defaultSchemaName"
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);
            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "newServer",
                Database = "database",
                User = "user",
                Password = "Password",
                SchemaCreatorUser = "createUser",
                SchemaCreatorPassword = "createPassword",
                SchemaName = "newSchemaName",
                DatabaseProvider = "PostgreSql"
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
                    Password = "awesomePassword",
                    DatabaseProvider = "PostgreSql"
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
                    Password = "awesomePassword",
                    DatabaseProvider = "PostgreSql"
				});

            system.SchemaCreator.ConnectionTriedWith
	            .Should().Contain(@"Host=AwesomeServer;Database=TestDatabase;Username=testUser;Password=awesomePassword");
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
                    SchemaCreatorPassword = "createPassword",
                    DatabaseProvider = "PostgreSql"
				});

            system.SchemaCreator.ConnectionTriedWith
	            .Should().Contain(@"Host=AwesomeServer;Database=TestDatabase;Username=createUser;Password=createPassword");
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
                    SchemaName = "schema",
                    DatabaseProvider = "PostgreSql"
				});

            system.SchemaCreator.Schemas.Last().ConnectionString
	            .Should().Contain(@"Host=AwesomeServer;Database=TestDatabase;Username=createUser;Password=createPassword");
            Assert.AreEqual("schema", system.SchemaCreator.Schemas.Last().SchemaName);
        }

        [Test]
        public void ShouldThrowWhenSchemaAlreadyExists()
        {
            var system = new SystemUnderTest();
            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                Server = "server",
                Database = "existingDatabase",
                SchemaName = "existingSchema",
                DatabaseProvider = "PostgreSql"
			});

            var e = Assert.Throws<Exception>(() => system.ConfigurationApi.CreateServerConfiguration(
                new CreateServerConfiguration
                {
                    Server = "server",
                    Database = "existingDatabase",
                    SchemaName = "existingSchema",
                    DatabaseProvider = "PostgreSql"
				}));
            Assert.AreEqual("Schema already exists.", e.Message);
        }

        [Test]
        public void ShouldThrowWhenDefaultSchemaNameAlreadyExists()
        {
            var system = new SystemUnderTest();
            system.SchemaCreator.Has(DefaultSchemaName.Postgres(), "Host=_;Database=existingDatabase");

            Assert.Throws<Exception>(() => system.ConfigurationApi.CreateServerConfiguration(
                new CreateServerConfiguration
                {
	                Server = "_",
	                Database = "existingDatabase",
                    SchemaName = null,
                    DatabaseProvider = "PostgreSql"
				}));
        }

        [Test]
        public void ShouldCreateSchemaWithSameNameInDifferentDatabase()
        {
            var system = new SystemUnderTest();
            system.SchemaCreator.Has("schemaName", "Host=_;Database=one");

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
	            Server = "_",
	            Database = "two",
                SchemaName = "schemaName",
                DatabaseProvider = "PostgreSql"
			});

            Assert.AreEqual(2, system.SchemaCreator.Schemas.Count());
            Assert.AreEqual("schemaName", system.SchemaCreator.Schemas.Last().SchemaName);
            system.SchemaCreator.Schemas.Last().ConnectionString
	            .Should().StartWith("Host=_;Database=two");
        }

        [Test]
        public void ShouldSaveNewServerConfigurationWithName()
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration
            {
                Name = "namedConfiguration",
                SchemaName = "schema",
                DatabaseProvider = "PostgreSql"
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
		        Server = "_",
		        Database = "db",
		        SchemaName = null,
		        DatabaseProvider = "PostgreSql"
	        });

	        var storedConfiguration = system.ConfigurationStorage.Data.Last();
	        Assert.AreEqual(DefaultSchemaName.Postgres(), storedConfiguration.SchemaName);
        }
    }
}