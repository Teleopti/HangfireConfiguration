using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbAgnostic;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
public class HangfireConfigurationSchemaInstallerSqlServerTest
{
	[Test]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	[TestCase(4)]
	[TestCase(5)]
	[TestCase(6)]
	[TestCase(7)]
	[TestCase(8)]
	public void ShouldInstallSchemaVersion(int schemaVersion)
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion);
		Assert.AreEqual(schemaVersion, version());
	}

	[Test]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	[TestCase(4)]
	[TestCase(5)]
	[TestCase(6)]
	[TestCase(7)]
	[TestCase(8)]
	public void ShouldUpgradeFromSchemaVersion(int schemaVersion)
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion);
		
		install();

		version().Should().Be(HangfireConfigurationSchemaInstaller.SchemaVersion(ConnectionStrings.SqlServer));
	}

	[Test]
	public void ShouldUpgradeFrom2ToLatest()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 2);
		Assert.AreEqual(2, version());
		using (var c = new SqlConnection(ConnectionStrings.SqlServer))
			c.Execute(@"
INSERT INTO 
    HangfireConfiguration.Configuration 
    (ConnectionString, SchemaName, GoalWorkerCount, Active) 
    VALUES (@ConnectionString, @SchemaName, @GoalWorkerCount, @Active)", 
				new { GoalWorkerCount = 99, ConnectionString = (string)null, SchemaName = (string)null, Active = (bool?)null });

		install();

		Assert.AreEqual(99, read().Single().Containers.Single().GoalWorkerCount);
		Assert.AreEqual(HangfireConfigurationSchemaInstaller.SchemaVersion(ConnectionStrings.SqlServer), version());
	}

	[Test]
	public void ShouldUpgradeFrom2To3()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 2);
		Assert.AreEqual(2, version());
		using (var c = ConnectionStrings.SqlServer.CreateConnection())
		{
			c.Execute(@"INSERT INTO HangfireConfiguration.Configuration (ConnectionString) VALUES (@ConnectionString)", new { ConnectionString = (string)null });
			c.Execute(@"INSERT INTO HangfireConfiguration.Configuration (ConnectionString) VALUES (@ConnectionString)", new { ConnectionString = (string)null });
		}

		install(3);

		using var c2 = new SqlConnection(ConnectionStrings.SqlServer);
		var result = c2.Query<dynamic>("SELECT * FROM hangfireconfiguration.configuration");
		Assert.AreEqual(DefaultConfigurationName.Name(), result.First().Name);
		Assert.AreEqual(3, version());
	}

	[Test]
	public void ShouldUpgradeFrom1ToLatest()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 1);
		using (var c = ConnectionStrings.SqlServer.CreateConnection())
			c.Execute("INSERT INTO HangfireConfiguration.Configuration ([Key], Value) VALUES ('GoalWorkerCount', 52)");

		install();

		Assert.AreEqual(52, read().Single().Containers.Single().GoalWorkerCount);
		Assert.AreEqual(HangfireConfigurationSchemaInstaller.SchemaVersion(ConnectionStrings.SqlServer), version());
	}

	[Test]
	public void ShouldUpgradeFrom6ToLatest()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 6);
		using (var c = ConnectionStrings.SqlServer.CreateConnection())
			c.Execute(@"INSERT INTO HangfireConfiguration.Configuration 
(ConnectionString, SchemaName, GoalWorkerCount, Active, MaxWorkersPerServer, WorkerBalancerEnabled) 
VALUES 
(@ConnectionString, @SchemaName, @GoalWorkerCount, @Active, @MaxWorkersPerServer, @WorkerBalancerEnabled) ",
				new
				{
					ConnectionString = ConnectionStrings.SqlServer,
					SchemaName = "hangfire",
					Active = true,
					WorkerBalancerEnabled = true,
					GoalWorkerCount = 10,
					MaxWorkersPerServer = 2,
				});

		install();

		var result = read().Single();
		result.ConnectionString.Should().Be(ConnectionStrings.SqlServer);
		result.SchemaName.Should().Be("hangfire");
		result.Active.Should().Be(true);
		result.Containers.Single().WorkerBalancerEnabled.Should().Be(true);
		result.Containers.Single().GoalWorkerCount.Should().Be(10);
		result.Containers.Single().MaxWorkersPerServer.Should().Be(2);
		version().Should().Be(HangfireConfigurationSchemaInstaller.SchemaVersion(ConnectionStrings.SqlServer));
	}

	[Test]
	public void ShouldUpgradeFrom7ToLatest()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 7);
		using (var c = ConnectionStrings.SqlServer.CreateConnection())
			c.Execute(@"INSERT INTO HangfireConfiguration.KeyValueStore ([Key], [Value]) VALUES (@Key, @Value)",
				new
				{
					Key = "Configuration:1",
					Value = @"{""Id"":1,""ConnectionString"":""Data Source=."",""GoalWorkerCount"":10,""MaxWorkersPerServer"":2,""WorkerBalancerEnabled"":true,""Active"":true}"
				});

		install();

		var result = read().Single();
		result.ConnectionString.Should().Be("Data Source=.");
		result.Active.Should().Be(true);
		result.Containers.Should().Not.Be.Null();
		result.Containers.Single().Tag.Should().Be(DefaultContainerTag.Tag());
		result.Containers.Single().GoalWorkerCount.Should().Be(10);
		result.Containers.Single().MaxWorkersPerServer.Should().Be(2);
		result.Containers.Single().WorkerBalancerEnabled.Should().Be(true);
		version().Should().Be(HangfireConfigurationSchemaInstaller.SchemaVersion(ConnectionStrings.SqlServer));
	}
	
	private void install(int? schemaVersion = null)
	{
		using var c = ConnectionStrings.SqlServer.CreateConnection();
		if (schemaVersion.HasValue)
			HangfireConfigurationSchemaInstaller.Install(c, schemaVersion.Value);
		else
			HangfireConfigurationSchemaInstaller.Install(c);
	}

	private static int version()
	{
		using var c = ConnectionStrings.SqlServer.CreateConnection();
		return c.Query<int>("SELECT Version FROM HangfireConfiguration.[Schema]").Single();
	}

	private IEnumerable<StoredConfiguration> read() =>
		new HangfireConfiguration()
			.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionStrings.SqlServer})
			.ConfigurationApi()
			.ReadConfigurations();
}
