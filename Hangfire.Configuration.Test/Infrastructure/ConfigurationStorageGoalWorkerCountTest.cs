using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

public class ConfigurationStorageGoalWorkerCountTest(string connectionString) : 
	DatabaseTest(connectionString)
{
	[Test]
	public void ShouldReadEmptyGoalWorkerCount()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
		var storage = system.ConfigurationStorage;
		
		Assert.IsEmpty(storage.ReadConfigurations());
	}

	[Test]
	public void ShouldWriteGoalWorkerCount()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
		var storage = system.ConfigurationStorage;

		storage.WriteConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 1 } }});

		Assert.AreEqual(1, storage.ReadConfigurations().Single().Containers.Single().GoalWorkerCount);
	}

	[Test]
	public void ShouldReadGoalWorkerCount()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
		var storage = system.ConfigurationStorage;
		storage.WriteConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 1 } }});

		var actual = storage.ReadConfigurations();

		Assert.AreEqual(1, actual.Single().Containers.Single().GoalWorkerCount);
	}

	[Test]
	public void ShouldWriteNullGoalWorkerCount()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
		var storage = system.ConfigurationStorage;
		storage.WriteConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 1 } }});

		var configuration = storage.ReadConfigurations().Single();
		configuration.Containers ??= new[] { new ContainerConfiguration() }; configuration.Containers[0].GoalWorkerCount = null;
		storage.WriteConfiguration(configuration);

		Assert.Null(storage.ReadConfigurations().Single().Containers.Single().GoalWorkerCount);
	}
}