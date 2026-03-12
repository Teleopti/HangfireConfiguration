using System.Collections.Generic;
using System.Linq;
using Dapper;
using DbAgnostic;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
public class HangfireConfigurationSchemaInstallerPostgresTest
{
	[Test]
	[TestCase(5)]
	[TestCase(6)]
	[TestCase(7)]
	public void ShouldInstallSchemaVersion(int schemaVersion)
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion);
		Assert.AreEqual(schemaVersion, version());
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
					SchemaName = "s",
					Active = true,
					WorkerBalancerEnabled = true,
					GoalWorkerCount = 10,
					MaxWorkersPerServer = 2,
				});

		install();

		Assert.AreEqual(10, read().Single().GoalWorkerCount);
		Assert.AreEqual(HangfireConfigurationSchemaInstaller.SchemaVersion, version());
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