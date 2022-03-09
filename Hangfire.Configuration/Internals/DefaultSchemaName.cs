using System.Reflection;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Internals;

internal static class DefaultSchemaName
{
	public static string SqlServer() =>
		typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
			.GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public).GetValue(null) as string;

	public static string Postgres() => new PostgreSqlStorageOptions().SchemaName;
}