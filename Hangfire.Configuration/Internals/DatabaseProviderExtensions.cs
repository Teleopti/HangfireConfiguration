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

		public static string ChangeDatabase(this string connectionString, string newDatabase)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) {InitialCatalog = newDatabase}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Database = newDatabase}.ToString());
		}

		public static DbConnection CreateConnection(this string connectionString)
		{
			var connection = new ConnectionStringDialectSelector(connectionString).SelectDialect<DbConnection>(
				() => new SqlConnection(), () => new NpgsqlConnection());
			connection.ConnectionString = connectionString;
			return connection;
		}


		public static string PointToMasterDatabase(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) {InitialCatalog = "master"}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Database = "postgres"}.ToString());
		}
	}
}