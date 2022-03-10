using System;
using Dapper;

namespace Hangfire.Configuration.Test;

public static class DatabaseTestSetup
{
	public static void Setup(string connectionString, int? schemaVersion = null)
	{
		closeOpenConnections(connectionString);
		dropDb(connectionString);
		createDb(connectionString);
		installSchema(connectionString, schemaVersion);
	}

	private static void closeOpenConnections(string connectionString)
	{
		new ConnectionStringDialectSelector(connectionString).SelectDialectVoid(
			() =>
			{
				var closeExistingConnSql = String.Format(
					@"if db_id('{0}') is not null alter database [{0}] set single_user with rollback immediate",
					connectionString.DatabaseName());

				using var c = connectionString.PointToMasterDatabase().CreateConnection();
				c.Execute(closeExistingConnSql);
			}, () => { });
	}

	private static void dropDb(string connectionString)
	{
		var sqlServer = $"DROP DATABASE IF EXISTS [{connectionString.DatabaseName()}]";
		var postgres = $"DROP DATABASE IF EXISTS \"{connectionString.DatabaseName()}\" WITH (FORCE);";
		var sql = new ConnectionStringDialectSelector(connectionString)
			.SelectDialect(() => sqlServer, () => postgres);
		using var c = connectionString.PointToMasterDatabase().CreateConnection();
		c.Execute(sql);
	}

	private static void createDb(string connectionString)
	{
		var sqlServer = $"CREATE DATABASE [{connectionString.DatabaseName()}]";
		var postgres = $@"CREATE DATABASE ""{connectionString.DatabaseName()}""";
		var sql = new ConnectionStringDialectSelector(connectionString)
			.SelectDialect(() => sqlServer, () => postgres);
		using var c = connectionString.PointToMasterDatabase().CreateConnection();
		c.Execute(sql);
	}

	private static void installSchema(string connectionString, int? schemaVersion)
	{
		using var connection = connectionString.CreateConnection();

		if (schemaVersion.HasValue)
		{
			if (schemaVersion.Value > 0)
				SqlServerObjectsInstaller.Install(connection, schemaVersion.Value);
		}
		else
			SqlServerObjectsInstaller.Install(connection);
	}
}