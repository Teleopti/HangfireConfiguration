using System.Data.SqlClient;
using Npgsql;

namespace Hangfire.Configuration.Internals
{
	internal static class DatabaseProviderExtensions
	{
		public static string DatabaseName(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).InitialCatalog,
				() => new NpgsqlConnectionStringBuilder(connectionString).Database);
		}

		public static string ServerName(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).DataSource,
				() => new NpgsqlConnectionStringBuilder(connectionString).Host);
		}

		public static string PointToMasterDatabase(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) {InitialCatalog = "master"}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Database = "postgres"}.ToString());
		}
	}
}