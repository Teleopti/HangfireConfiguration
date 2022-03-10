using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace Hangfire.Configuration.Internals
{
	internal static class DatabaseProviderExtensions
	{
		public const string RedisStart = "redis$$";
	
		public static string TrimRedisPrefix(this string connString)
		{
			return connString != null && connString.StartsWith(RedisStart) ? 
				connString.Substring(RedisStart.Length) : 
				connString;
		}
		
		public static string DatabaseName(this string connectionString)
		{
			return connectionString.ToDbVendorSelector().SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).InitialCatalog,
				() => new NpgsqlConnectionStringBuilder(connectionString).Database);
		}

		public static string ApplicationName(this string connectionString)
		{
			return connectionString.ToDbVendorSelector().SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).ApplicationName,
				() => new NpgsqlConnectionStringBuilder(connectionString).ApplicationName);
		}

		public static string PointToMasterDatabase(this string connectionString)
		{
			return connectionString.ToDbVendorSelector().SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) {InitialCatalog = "master"}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Database = "postgres"}.ToString());
		}

		public static string ChangeDatabase(this string connectionString, string newDatabase)
		{
			return connectionString.ToDbVendorSelector().SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) {InitialCatalog = newDatabase}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Database = newDatabase}.ToString());
		}

		public static string ChangeApplicationName(this string connectionString, string applicationName) =>
			connectionString.ToDbVendorSelector().SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) {ApplicationName = applicationName}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {ApplicationName = applicationName}.ToString(),
				() => connectionString);

		public static DbConnection CreateConnection(this string connectionString)
		{
			var connection = connectionString.ToDbVendorSelector().SelectDialect<DbConnection>(
				() => new SqlConnection(), () => new NpgsqlConnection());
			connection.ConnectionString = connectionString;
			return connection;
		}
	}
}