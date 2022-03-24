using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

public class ConfigurationStorageMaxWorkersPerServerTest : DatabaseTest
{
	public ConfigurationStorageMaxWorkersPerServerTest(string connectionString) : base(connectionString)
	{
	}

	[Test]
	public void ShouldWriteMaxWorkersPerServer()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});

		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {MaxWorkersPerServer = 5});

		Assert.AreEqual(5, system.ConfigurationStorage.ReadConfigurations().Single().MaxWorkersPerServer);
	}

	[Test]
	public void ShouldUpdateMaxWorkersPerServer()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {MaxWorkersPerServer = 5});
		var existing = system.ConfigurationStorage.ReadConfigurations().Single();

		existing.MaxWorkersPerServer = 3;
		system.ConfigurationStorage.WriteConfiguration(existing);

		Assert.AreEqual(3, system.ConfigurationStorage.ReadConfigurations().Single().MaxWorkersPerServer);
	}
}