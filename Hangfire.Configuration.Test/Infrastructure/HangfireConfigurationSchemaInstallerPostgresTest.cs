using System.Collections.Generic;
using System.Linq;
using Dapper;
using DbAgnostic;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
public class HangfireConfigurationSchemaInstallerPostgresTest
{
	[Test]
	[TestCase(5)]
	[TestCase(6)]
	[TestCase(7)]
	[TestCase(8)]
	public void ShouldInstallSchemaVersion(int schemaVersion)
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion);
		Assert.AreEqual(schemaVersion, version());
	}

	[Test]
	[TestCase(5)]
	[TestCase(6)]
	[TestCase(7)]
	[TestCase(8)]
	public void ShouldUpgradeFromSchemaVersion(int schemaVersion)
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion);

		install();

		version().Should().Be(HangfireConfigurationSchemaInstaller.SchemaVersion);
	}

	[Test]
	public void ShouldUpgradeFrom5To6()
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion: 5);
		Assert.AreEqual(5, version());

		install(6);

		Assert.AreEqual(6, version());
	}

	[Test]
	public void ShouldUpgradeFrom6ToLatest()
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion: 6);
		using (var c = ConnectionStrings.Postgres.CreateConnection())
			c.Execute(@"INSERT INTO HangfireConfiguration.Configuration 
(ConnectionString, SchemaName, GoalWorkerCount, Active, MaxWorkersPerServer, WorkerBalancerEnabled) 
VALUES 
(@ConnectionString, @SchemaName, @GoalWorkerCount, @Active, @MaxWorkersPerServer, @WorkerBalancerEnabled) ",
				new
				{
					ConnectionString = ConnectionStrings.Postgres,
					SchemaName = "hangfire",
					Active = true,
					WorkerBalancerEnabled = true,
					GoalWorkerCount = 10,
					MaxWorkersPerServer = 2,
				});

		install();

		var result = read().Single();
		result.ConnectionString.Should().Be(ConnectionStrings.Postgres);
		result.SchemaName.Should().Be("hangfire");
		result.Active.Should().Be(true);
		result.Containers.Single().WorkerBalancerEnabled.Should().Be(true);
		result.Containers.Single().GoalWorkerCount.Should().Be(10);
		result.Containers.Single().MaxWorkersPerServer.Should().Be(2);
		version().Should().Be(HangfireConfigurationSchemaInstaller.SchemaVersion);
	}

	[Test]
	public void ShouldUpgradeFrom7ToLatest()
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion: 7);
		using (var c = ConnectionStrings.Postgres.CreateConnection())
			c.Execute(@"INSERT INTO HangfireConfiguration.KeyValueStore (Key, Value) VALUES (@Key, @Value)",
				new
				{
					Key = "Configuration:1",
					Value = @"{""Id"":1,""ConnectionString"":""Host=localhost"",""GoalWorkerCount"":10,""MaxWorkersPerServer"":2,""WorkerBalancerEnabled"":true,""Active"":true}"
				});

		install();

		var result = read().Single();
		result.ConnectionString.Should().Be("Host=localhost");
		result.Active.Should().Be(true);
		result.Containers.Should().Not.Be.Null();
		result.Containers.Single().Tag.Should().Be(DefaultContainerTag.Tag());
		result.Containers.Single().GoalWorkerCount.Should().Be(10);
		result.Containers.Single().MaxWorkersPerServer.Should().Be(2);
		result.Containers.Single().WorkerBalancerEnabled.Should().Be(true);
		version().Should().Be(HangfireConfigurationSchemaInstaller.SchemaVersion);
	}

	private void install(int? schemaVersion = null)
	{
		using var c = ConnectionStrings.Postgres.CreateConnection();
		if (schemaVersion.HasValue)
			HangfireConfigurationSchemaInstaller.Install(c, schemaVersion.Value);
		else
			HangfireConfigurationSchemaInstaller.Install(c);
	}

	private static int version()
	{
		using var c = ConnectionStrings.Postgres.CreateConnection();
		return c.Query<int>("SELECT Version FROM HangfireConfiguration.Schema").Single();
	}

	private IEnumerable<StoredConfiguration> read() =>
		new HangfireConfiguration()
			.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionStrings.Postgres})
			.ConfigurationApi()
			.ReadConfigurations();
}
