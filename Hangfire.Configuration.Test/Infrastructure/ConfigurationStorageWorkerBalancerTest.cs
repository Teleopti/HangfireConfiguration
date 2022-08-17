using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Infrastructure;

public class ConfigurationStorageWorkerBalancerTest : DatabaseTest
{
	public ConfigurationStorageWorkerBalancerTest(string connectionString) : base(connectionString)
	{
	}

	[Test]
	public void ShouldWriteEnabled()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});

		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {WorkerBalancerEnabled = true});

		system.ConfigurationStorage.ReadConfigurations().Single().WorkerBalancerEnabled
			.Should().Be(true);
	}

	[Test]
	public void ShouldWriteDisabled()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});

		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {WorkerBalancerEnabled = false});

		system.ConfigurationStorage.ReadConfigurations().Single().WorkerBalancerEnabled
			.Should().Be(false);
	}

	[Test]
	public void ShouldReadNull()
	{
		Console.WriteLine(ConnectionString);
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});

		system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration());

		system.ConfigurationStorage.ReadConfigurations().Single().WorkerBalancerEnabled
			.Should().Be(null);
	}
}