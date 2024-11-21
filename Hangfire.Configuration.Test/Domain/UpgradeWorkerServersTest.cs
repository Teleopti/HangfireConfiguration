using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class UpgradeWorkerServersTest
{
	[Test]
	public void ShouldUpgradeConfigurationSchema()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = "Host=local;Database=d"});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledHangfireConfigurationSchema
			.Should().Contain("Host=local;Database=d");
	}

	[Test]
	public void ShouldUpgradeStorageSchema()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Data Source=.;Initial Catalog=db"
		});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledSchemas
			.Single().ConnectionString.Should().Contain("Data Source=.;Initial Catalog=db");
	}

	[Test]
	public void ShouldUpgradeStorageSchemaWithSchemaName()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Data Source=.;Initial Catalog=db",
			SchemaName = "myschema"
		});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledSchemas
			.Single().SchemaName.Should().Be("myschema");
	}

	[Test]
	public void ShouldUpgradeTwoConfigurations()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(
			new StoredConfiguration
			{
				ConnectionString = "Data Source=.;Initial Catalog=db",
				SchemaName = "schema1"
			},
			new StoredConfiguration
			{
				ConnectionString = "Data Source=.;Initial Catalog=db",
				SchemaName = "schema2"
			});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledSchemas.ElementAt(0).SchemaName.Should().Be("schema1");
		system.SchemaInstaller.InstalledSchemas.ElementAt(1).SchemaName.Should().Be("schema2");
	}

	[Test]
	public void ShouldSkipNullOrEmptyConnectionString([Values("", null)] string c)
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(
			new StoredConfiguration
			{
				ConnectionString = c
			});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledSchemas.Should().Be.Empty();
	}

	[Test]
	public void ShouldUpgradeStorageSchemaWithDefaultSchemaName()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Data Source=.;Initial Catalog=db",
		});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledSchemas
			.Single().SchemaName.Should().Be("HangFire");
	}

	[Test]
	public void ShouldUpgradeStorageSchemaWithDefaultSchemaNamePostgres()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Host=local;Database=db",
		});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledSchemas
			.Single().SchemaName.Should().Be("hangfire");
	}

	[Test]
	public void ShouldUpgradeUsingCredentials()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Data Source=.;Initial Catalog=db"
		});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers
		{
			SchemaUpgraderUser = "upgrader",
			SchemaUpgraderPassword = "pass",
		});

		var upgraded = system.SchemaInstaller.InstalledSchemas.Single();
		upgraded.ConnectionString.Should().Contain("User ID=upgrader");
		upgraded.ConnectionString.Should().Contain("Password=pass");
	}

	[Test]
	public void ShouldUpgradeUsingCredentialsPostgres()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Host=localhost;Database=datta"
		});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers
		{
			SchemaUpgraderUser = "upgrader",
			SchemaUpgraderPassword = "pass",
		});

		var upgraded = system.SchemaInstaller.InstalledSchemas.Single();
		upgraded.ConnectionString.Should().Contain("Username=upgrader");
		upgraded.ConnectionString.Should().Contain("Password=pass");
	}

	[Test]
	public void ShouldNotUpgradeRedis()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "redisserver"
		});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledSchemas.Should().Be.Empty();
	}

	[Test]
	public void ShouldThrowOnFailingUpgrade()
	{
		var system = new SystemUnderTest();
		system.SchemaInstaller.InstallHangfireConfigurationSchemaFailsWith = new Exception("boom!");

		var exception = Assert.Catch(() => system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers()));

		exception.Message.Should().Be("boom!");
	}

	[Test]
	public void ShouldInstallStorageSchemaEvenIfConfigurationSchemaFails()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Host=host;Database=datta"
		});
		system.SchemaInstaller.InstallHangfireConfigurationSchemaFailsWith = new Exception("boom!");

		Assert.Catch(() => system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers()));

		system.SchemaInstaller.InstalledSchemas.Single().ConnectionString
			.Should().Contain("Host=host;Database=datta");
	}

	[Test]
	public void ShouldInstallSecondStorageSchemaEvenIfFirstStorageSchemaFails()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(
			new StoredConfiguration
			{
				ConnectionString = "Host=host;Database=data1",
				SchemaName = "schema1"
			},
			new StoredConfiguration
			{
				ConnectionString = "Host=host;Database=data2",
				SchemaName = "schema2"
			});
		system.SchemaInstaller.InstallHangfireStorageSchemaFailsWith = (new Exception("boom!"), "schema1");

		var exception = Assert.Catch(() => system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers()));

		system.SchemaInstaller.InstalledSchemas.Single().SchemaName
			.Should().Be("schema2");
		exception.Message.Should().Be("boom!");
	}
	
	[Test]
	public void ShouldUpgradeUsingIntegratedSecurityWithoutCredentials()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Data Source=.;Initial Catalog=db"
		});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers());

		var upgraded = system.SchemaInstaller.InstalledSchemas.Single();
		upgraded.ConnectionString.Should().Contain("Integrated Security=True");
	}
	
	[Test]
	public void ShouldUpgradeConfigurationSchemaWithCredentials()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = "Data Source=."});

		system.ConfigurationApi().UpgradeWorkerServers(new UpgradeWorkerServers
		{
			SchemaUpgraderUser = "user",
			SchemaUpgraderPassword = "pass"
		});

		var upgraded = system.SchemaInstaller.InstalledHangfireConfigurationSchema; 
		upgraded.Should().Contain("User ID=user");
		upgraded.Should().Contain("Password=pass");
	}
}