using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
public class SchemaInstallSqlServerTest
{
	[Test]
	public void ShouldInstallSchemaVersion1()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 1);

		Assert.AreEqual(1, version());
	}

	[Test]
	public void ShouldInstallSchemaVersion2()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 2);

		Assert.AreEqual(2, version());
	}

	[Test]
	public void ShouldInstallSchemaVersion3()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 3);

		Assert.AreEqual(3, version());
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
    VALUES (@ConnectionString, @SchemaName, @GoalWorkerCount, @Active)
", new values {GoalWorkerCount = 99});

		install();

		Assert.AreEqual(99, read().Single().GoalWorkerCount);
		Assert.AreEqual(DatabaseSchemaInstaller.SchemaVersion, version());
	}

	[Test]
	public void ShouldUpgradeFrom2To3()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 2);
		Assert.AreEqual(2, version());
		using (var c = new SqlConnection(ConnectionStrings.SqlServer))
		{
			c.Execute(@"INSERT INTO HangfireConfiguration.Configuration (ConnectionString) VALUES (@ConnectionString)", new values());
			c.Execute(@"INSERT INTO HangfireConfiguration.Configuration (ConnectionString) VALUES (@ConnectionString)", new values());
		}

		install(3);

		Assert.AreEqual(DefaultConfigurationName.Name(), read().First().Name);
		Assert.AreEqual(3, version());
	}

	[Test]
	public void ShouldUpgradeFrom1ToLatest()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer, schemaVersion: 1);
		using (var c = new SqlConnection(ConnectionStrings.SqlServer))
			c.Execute("INSERT INTO HangfireConfiguration.Configuration ([Key], Value) VALUES ('GoalWorkerCount', 52)");

		install();

		Assert.AreEqual(52, read().Single().GoalWorkerCount);
		Assert.AreEqual(DatabaseSchemaInstaller.SchemaVersion, version());
	}

	private void install(int? schemaVersion = null)
	{
		using var c = new SqlConnection(ConnectionStrings.SqlServer);
		if (schemaVersion.HasValue)
			DatabaseSchemaInstaller.Install(c, schemaVersion.Value);
		else
			DatabaseSchemaInstaller.Install(c);
	}

	private static int version()
	{
		using var c = new SqlConnection(ConnectionStrings.SqlServer);
		return c.Query<int>("SELECT Version FROM HangfireConfiguration.[Schema]").Single();
	}

	private IEnumerable<values> read()
	{
		using var c = new SqlConnection(ConnectionStrings.SqlServer);
		return c.Query<values>("SELECT * FROM hangfireconfiguration.configuration");
	}

	private class values
	{
		public int Id { get; set; }

		public string Key { get; set; }
		public string Value { get; set; }

		public string Name { get; set; }
		public string ConnectionString { get; set; }
		public string SchemaName { get; set; }
		public int? GoalWorkerCount { get; set; }
		public int? Active { get; set; }
	}
}