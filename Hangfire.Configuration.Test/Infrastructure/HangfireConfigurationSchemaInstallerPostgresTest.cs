using System.Linq;
using Dapper;
using Npgsql;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
public class HangfireConfigurationSchemaInstallerPostgresTest
{
	[Test]
	public void ShouldInstallSchemaVersion5()
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion: 5);

		Assert.AreEqual(5, version());
	}

	[Test]
	public void ShouldInstallSchemaVersion6()
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion: 6);

		Assert.AreEqual(6, version());
	}

	[Test]
	public void ShouldUpgradeFrom5To6()
	{
		DatabaseTestSetup.SetupPostgres(ConnectionStrings.Postgres, schemaVersion: 5);
		Assert.AreEqual(5, version());

		install(6);

		Assert.AreEqual(6, version());
	}

	private void install(int? schemaVersion = null)
	{
		using var c = new NpgsqlConnection(ConnectionStrings.Postgres);
		if (schemaVersion.HasValue)
			HangfireConfigurationSchemaInstaller.Install(c, schemaVersion.Value);
		else
			HangfireConfigurationSchemaInstaller.Install(c);
	}

	private static int version()
	{
		using var c = new NpgsqlConnection(ConnectionStrings.Postgres);
		return c.Query<int>("SELECT Version FROM HangfireConfiguration.Schema").Single();
	}
}