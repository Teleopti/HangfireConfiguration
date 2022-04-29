using System.Data.Common;
using System.Data.SqlClient;
using Dapper;
using Npgsql;
using NUnit.Framework;

namespace Hangfire.Configuration.Test;

public static class DatabaseTestSetup
{
	private static int _postgresTestCount = 0;

	public static void SetupPostgres(string connectionString, int? schemaVersion = null)
	{
		if (_postgresTestCount >= int.MaxValue)
			Assert.Ignore("Skipping the rest of postgres");
		_postgresTestCount++;

		var database = new NpgsqlConnectionStringBuilder(connectionString).Database;
		var master = new NpgsqlConnectionStringBuilder(connectionString) {Database = "postgres"}.ToString();
		using (var c = new NpgsqlConnection(master))
		{
			c.Execute($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE);");
			c.Execute($@"CREATE DATABASE ""{database}""");
		}

		using (var conn = new NpgsqlConnection(connectionString))
			installSchema(conn, schemaVersion);
	}

	private static int _sqlServerTestCount = 0;

	public static void SetupSqlServer(string connectionString, int? schemaVersion = null)
	{
		if (_sqlServerTestCount >= int.MaxValue)
			Assert.Ignore("Skipping the rest of sql server");
		_sqlServerTestCount++;

		var database = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
		var master = new SqlConnectionStringBuilder(connectionString) {InitialCatalog = "master"}.ToString();
		using (var c = new SqlConnection(master))
		{
			c.Execute($"if db_id('{database}') is not null alter database [{database}] set single_user with rollback immediate");
			c.Execute($"DROP DATABASE IF EXISTS [{database}]");
			c.Execute($"CREATE DATABASE [{database}]");
		}

		using (var conn = new SqlConnection(connectionString))
			installSchema(conn, schemaVersion);
	}

	private static void installSchema(DbConnection connection, int? schemaVersion)
	{
		if (schemaVersion is > 0)
			HangfireConfigurationSchemaInstaller.Install(connection, schemaVersion.Value);
		else
			HangfireConfigurationSchemaInstaller.Install(connection);
	}
}