using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class ConfigureUpdateConfigurationsTest
{
	[Test]
	public void ShouldConfigureUpdatedConfiguration()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					Name = "name",
					ConnectionString = "connectionString",
					SchemaName = "schema"
				}
			}
		});
		system.WorkerServerStarter.Start();

		var configuration = system.Configurations().Single();
		Assert.AreEqual("name", configuration.Name);
		Assert.AreEqual("connectionString", configuration.ConnectionString);
		Assert.AreEqual("schema", configuration.SchemaName);
	}

	[Test]
	public void ShouldActivateOnFirstUpdate()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					Name = "name",
					ConnectionString = "connectionString"
				}
			}
		});
		system.WorkerServerStarter.Start();

		Assert.True(system.Configurations().Single().Active);
	}

	[Test]
	public void ShouldUpdateConfiguration()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Name = "name", ConnectionString = "previous"});

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					Name = "name",
					ConnectionString = "newConnectionString"
				}
			}
		});
		system.WorkerServerStarter.Start();

		var configuration = system.Configurations().Single();
		Assert.AreEqual("newConnectionString", configuration.ConnectionString);
	}

	[Test]
	public void ShouldConfigureUpdatedConfigurations()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					Name = "name1",
					ConnectionString = "connectionString1",
					SchemaName = "schema1"
				},
				new ExternalConfiguration
				{
					Name = "name2",
					ConnectionString = "connectionString2",
					SchemaName = "schema2"
				}
			}
		});
		system.WorkerServerStarter.Start();

		var configuration = system.Configurations().OrderBy(x => x.Id);
		Assert.AreEqual("name1", configuration.ElementAt(0).Name);
		Assert.AreEqual("connectionString1", configuration.ElementAt(0).ConnectionString);
		Assert.AreEqual("schema1", configuration.ElementAt(0).SchemaName);
		Assert.AreEqual("name2", configuration.ElementAt(1).Name);
		Assert.AreEqual("connectionString2", configuration.ElementAt(1).ConnectionString);
		Assert.AreEqual("schema2", configuration.ElementAt(1).SchemaName);
	}
}