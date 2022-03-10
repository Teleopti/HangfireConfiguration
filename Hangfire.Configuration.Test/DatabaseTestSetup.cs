using Dapper;

namespace Hangfire.Configuration.Test;

public static class DatabaseTestSetup
{
	public static void SetupPostgres(string connectionString, int? schemaVersion = null)
	{
		using var c = connectionString.PointToMasterDatabase().CreateConnection();
		c.Execute($"DROP DATABASE IF EXISTS \"{connectionString.DatabaseName()}\" WITH (FORCE);");
		c.Execute($@"CREATE DATABASE ""{connectionString.DatabaseName()}""");
		
		installSchema(connectionString, schemaVersion);
	}
	
	public static void SetupSqlServer(string connectionString, int? schemaVersion = null)
	{
		using var c = connectionString.PointToMasterDatabase().CreateConnection();
		c.Execute($"if db_id('{connectionString.DatabaseName()}') is not null alter database [{connectionString.DatabaseName()}] set single_user with rollback immediate");
		c.Execute($"DROP DATABASE IF EXISTS [{connectionString.DatabaseName()}]");
		c.Execute($"CREATE DATABASE [{connectionString.DatabaseName()}]");
		
		installSchema(connectionString, schemaVersion);
	}
	
	private static void installSchema(string connectionString, int? schemaVersion)
	{
		using var connection = connectionString.CreateConnection();

		if (schemaVersion is > 0)
		{
			SqlServerObjectsInstaller.Install(connection, schemaVersion.Value);
		}
		else
		{
			SqlServerObjectsInstaller.Install(connection);
		}
	}
}