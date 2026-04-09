using System;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class CreateServerConfigurationTest
{
	[Test]
	public void ShouldSaveNewServerConfiguration()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServer
		{
			Server = "AwesomeServer",
			Database = "TestDatabase",
			User = "testUser",
			Password = "awesomePassword",
			SchemaCreatorUser = "createuser",
			SchemaCreatorPassword = "createPassword",
			SchemaName = "awesomeSchema"
		});

		var storedConfiguration = system.Configurations().Last();
		Assert.AreEqual("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword", storedConfiguration.ConnectionString);
		Assert.AreEqual("awesomeSchema", storedConfiguration.SchemaName);
	}

	[Test]
	public void ShouldSetGoalWorkerCountToDefaultConfiguration()
	{
		var system = new SystemUnderTest();
		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new []
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
					Name = DefaultConfigurationName.Name(),
					SchemaName = "defaultSchemaName"
				}
			}
		});
		system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServer
		{
			Server = "newServer",
			SchemaName = "newSchemaName",
			Database = "database",
			User = "user",
			Password = "Password",
			SchemaCreatorUser = "createUser",
			SchemaCreatorPassword = "createPassword",
		});

		system.ConfigurationApi().WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = 10});

		var config = system.Configurations();
		Assert.AreEqual(10, config.First().Containers.Single().GoalWorkerCount);
	}

	[Test]
	public void ShouldReadAllConfigurations()
	{
		var system = new SystemUnderTest();
		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new []
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
					Name = DefaultConfigurationName.Name(),
					SchemaName = "defaultSchemaName"
				}
			}
		});
		system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServer
		{
			Server = "newServer",
			Database = "database",
			User = "user",
			Password = "Password",
			SchemaCreatorUser = "createUser",
			SchemaCreatorPassword = "createPassword",
			SchemaName = "newSchemaName"
		});

		var configurations = system.Configurations();

		Assert.AreEqual(2, configurations.Count());
	}

	[Test]
	public void ShouldThrowWhenConnectionFails()
	{
		var system = new SystemUnderTest();
		system.SchemaInstaller.TryConnectFailsWith = new Exception();

		Assert.Throws<Exception>(() => system.ConfigurationApi().CreateServerConfiguration(
			new CreateSqlServer
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

		system.ConfigurationApi().CreateServerConfiguration(
			new CreateSqlServer
			{
				Server = "AwesomeServer",
				Database = "TestDatabase",
				User = "testUser",
				Password = "awesomePassword"
			});

		system.SchemaInstaller.ConnectionTriedWith
			.Should().Contain("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword");
	}

	[Test]
	public void ShouldTryConnectWithCreatorConnectionString()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(
			new CreateSqlServer
			{
				Server = "AwesomeServer",
				Database = "TestDatabase",
				SchemaCreatorUser = "createUser",
				SchemaCreatorPassword = "createPassword"
			});

		system.SchemaInstaller.ConnectionTriedWith
			.Should().Contain("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=createUser;Password=createPassword");
	}

	[Test]
	public void ShouldCreateSchemaInDatabaseUsingGivenConnectionsString()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(
			new CreateSqlServer
			{
				Server = "AwesomeServer",
				Database = "TestDatabase",
				SchemaCreatorUser = "createUser",
				SchemaCreatorPassword = "createPassword",
				SchemaName = "schema"
			});
            
		system.SchemaInstaller.InstalledSchemas.Last().ConnectionString
			.Should().Contain("Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=createUser;Password=createPassword");
		Assert.AreEqual("schema", system.SchemaInstaller.InstalledSchemas.Last().SchemaName);
	}

	[Test]
	public void ShouldThrowWhenSchemaAlreadyExists()
	{
		var system = new SystemUnderTest();
		system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServer
		{
			Server = "server",
			Database = "existingDatabase",
			SchemaName = "existingSchema"
		});

		var e = Assert.Throws<Exception>(() => system.ConfigurationApi().CreateServerConfiguration(
			new CreateSqlServer
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
		system.SchemaInstaller.Has(DefaultSchemaName.SqlServer(), "Data Source=_;Initial Catalog=existingDatabase");

		Assert.Throws<Exception>(() => system.ConfigurationApi().CreateServerConfiguration(
			new CreateSqlServer
			{
				Server = "_",
				Database = "existingDatabase",
				SchemaName = null
			}));
	}

	[Test]
	public void ShouldCreateSchemaWithSameNameInDifferentDatabase()
	{
		var system = new SystemUnderTest();
		system.SchemaInstaller.Has("schemaName", "Data Source=_;Initial Catalog=one");

		system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServer
		{
			Server = "_",
			Database = "two",
			SchemaName = "schemaName"
		});

		Assert.AreEqual(2, system.SchemaInstaller.InstalledSchemas.Count());
		Assert.AreEqual("schemaName", system.SchemaInstaller.InstalledSchemas.Last().SchemaName);
		system.SchemaInstaller.InstalledSchemas.Last().ConnectionString
			.Should().StartWith("Data Source=_;Initial Catalog=two");
	}

	[Test]
	public void ShouldSaveNewServerConfigurationWithName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServer
		{
			Name = "namedConfiguration",
			SchemaName = "schema"
		});

		var storedConfiguration = system.Configurations().Last();
		Assert.AreEqual("namedConfiguration", storedConfiguration.Name);
	}
        
	[Test]
	public void ShouldCreateWithDefaultSchemaName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServer
		{
			SchemaName = null
		});

		var storedConfiguration = system.Configurations().Last();
		Assert.AreEqual(DefaultSchemaName.SqlServer(), storedConfiguration.SchemaName);
	}
}