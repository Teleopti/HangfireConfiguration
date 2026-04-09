using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

public class ConfigurationStorageMaxWorkersPerServerTest(string connectionString) : 
	DatabaseTest(connectionString)
{
	[Test]
	public void ShouldWriteMaxWorkersPerServer()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});

		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { MaxWorkersPerServer = 5 } }});

		Assert.AreEqual(5, system.ConfigurationStorage.ReadConfigurations().Single().Containers.Single().MaxWorkersPerServer);
	}

	[Test]
	public void ShouldUpdateMaxWorkersPerServer()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { MaxWorkersPerServer = 5 } }});
		var existing = system.ConfigurationStorage.ReadConfigurations().Single();

		existing.Containers ??= new[] { new ContainerConfiguration() }; existing.Containers[0].MaxWorkersPerServer = 3;
		system.ConfigurationStorage.WriteConfiguration(existing);

		Assert.AreEqual(3, system.ConfigurationStorage.ReadConfigurations().Single().Containers.Single().MaxWorkersPerServer);
	}
}