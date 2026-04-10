using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration;

public static class HangfireConfigurationSchemaInstaller
{
	internal const string SchemaName = "HangfireConfiguration";

	public static int SchemaVersion(string connectionString) =>
		connectionString.ToDbSelector().PickFunc(
			() => sqlServerMigrations.Keys.Max(),
			() => postgreSqlMigrations.Keys.Max());

	private static readonly Assembly assembly = typeof(HangfireConfigurationSchemaInstaller).GetTypeInfo().Assembly;

	private static readonly string sqlServerSetup = readResource("Hangfire.Configuration.SqlServer.Setup.sql");
	private static readonly Dictionary<int, string> sqlServerMigrations = loadMigrations("Hangfire.Configuration.SqlServer.");

	private static readonly string postgreSqlSetup = readResource("Hangfire.Configuration.PostgreSql.Setup.sql");
	private static readonly Dictionary<int, string> postgreSqlMigrations = loadMigrations("Hangfire.Configuration.PostgreSql.");

	public static void Install(DbConnection connection)
	{
		if (connection == null)
			throw new ArgumentNullException(nameof(connection));
		connection.ToDbSelector().PickAction(
			() => installSqlServer(connection, sqlServerMigrations.Keys.Max()),
			() => installPostgreSql(connection, postgreSqlMigrations.Keys.Max()));
	}

	public static void Install(DbConnection connection, int schemaVersion)
	{
		if (connection == null)
			throw new ArgumentNullException(nameof(connection));
		connection.ToDbSelector().PickAction(
			() => installSqlServer(connection, schemaVersion),
			() => installPostgreSql(connection, schemaVersion));
	}

	private static void installSqlServer(DbConnection connection, int targetVersion) =>
		install(connection, targetVersion, sqlServerSetup, sqlServerMigrations,
			$"SELECT [Version] FROM [{SchemaName}].[Schema]",
			$@"UPDATE [{SchemaName}].[Schema] SET [Version] = @Version
IF @@ROWCOUNT = 0 
	INSERT INTO [{SchemaName}].[Schema] ([Version]) VALUES (@Version)");

	private static void installPostgreSql(DbConnection connection, int targetVersion) =>
		install(connection, targetVersion, postgreSqlSetup, postgreSqlMigrations,
			$"SELECT version FROM {SchemaName}.schema",
			$@"UPDATE {SchemaName}.schema SET version = @Version;
INSERT INTO {SchemaName}.schema (version) SELECT @Version WHERE NOT EXISTS (SELECT 1 FROM {SchemaName}.schema)");

	private static void install(
		DbConnection connection, int targetVersion,
		string setupScript, Dictionary<int, string> migrations,
		string readVersionSql, string upsertVersionSql)
	{
		if (connection.State == ConnectionState.Closed)
			connection.Open();

		using var tx = connection.BeginTransaction();

		connection.Execute(
			setupScript.Replace("$(HangfireConfigurationSchema)", SchemaName),
			transaction: tx, commandTimeout: 0);

		var currentVersion = connection.QuerySingleOrDefault<int?>(
			readVersionSql, transaction: tx);

		var newVersion = currentVersion ?? -1;
		var pendingMigrations = migrations
			.Where(m => m.Key > (currentVersion ?? 0) && m.Key <= targetVersion)
			.OrderBy(m => m.Key);
		foreach (var migration in pendingMigrations)
		{
			connection.Execute(
				migration.Value.Replace("$(HangfireConfigurationSchema)", SchemaName),
				transaction: tx, commandTimeout: 0);
			newVersion = migration.Key;
		}

		connection.Execute(upsertVersionSql,
			new {Version = newVersion}, transaction: tx);

		tx.Commit();
	}

	private static Dictionary<int, string> loadMigrations(string prefix)
	{
		var migrations = new Dictionary<int, string>();
		var resourceNames = assembly.GetManifestResourceNames()
			.Where(name => name.StartsWith(prefix) && name.EndsWith(".sql") && !name.EndsWith("Setup.sql"));

		foreach (var resourceName in resourceNames)
		{
			var suffix = resourceName.Substring(prefix.Length);
			var versionStr = suffix.Substring(0, suffix.Length - 4);
			if (int.TryParse(versionStr, out var version))
				migrations[version] = readResource(resourceName);
		}

		return migrations;
	}

	private static string readResource(string name)
	{
		using var stream = assembly.GetManifestResourceStream(name);
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
}
