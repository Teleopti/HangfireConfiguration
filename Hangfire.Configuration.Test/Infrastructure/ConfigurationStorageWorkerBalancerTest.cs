using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Infrastructure;

public class ConfigurationStorageWorkerBalancerTest(string connectionString) : 
	DatabaseTest(connectionString)
{
	[Test]
	public void ShouldWriteEnabled()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});

		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { WorkerBalancerEnabled = true } }});

		system.ConfigurationStorage.ReadConfigurations().Single().Containers.Single().WorkerBalancerEnabled
			.Should().Be(true);
	}

	[Test]
	public void ShouldWriteDisabled()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});

		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { WorkerBalancerEnabled = false } }});

		system.ConfigurationStorage.ReadConfigurations().Single().Containers.Single().WorkerBalancerEnabled
			.Should().Be(false);
	}

	[Test]
	public void ShouldReadNull()
	{
		Console.WriteLine(ConnectionString);
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});

		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration());

		var result = system.ConfigurationStorage.ReadConfigurations().Single();
		(result.Containers?.SingleOrDefault()?.WorkerBalancerEnabled).Should().Be(null);
	}
}