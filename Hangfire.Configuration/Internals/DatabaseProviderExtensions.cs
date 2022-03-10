using System;
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
			return new connectionStringDialectSelector(connectionString).Select(
				() => new SqlConnectionStringBuilder(connectionString).InitialCatalog,
				() => new NpgsqlConnectionStringBuilder(connectionString).Database);
		}

		public static string ServerName(this string connectionString)
		{
			return new connectionStringDialectSelector(connectionString).Select(
				() => new SqlConnectionStringBuilder(connectionString).DataSource,
				() => new NpgsqlConnectionStringBuilder(connectionString).Host);
		}

		public static string ApplicationName(this string connectionString)
		{
			return new connectionStringDialectSelector(connectionString).Select(
				() => new SqlConnectionStringBuilder(connectionString).ApplicationName,
				() => new NpgsqlConnectionStringBuilder(connectionString).ApplicationName);
		}

		public static string PointToMasterDatabase(this string connectionString)
		{
			return new connectionStringDialectSelector(connectionString).Select(
				() => new SqlConnectionStringBuilder(connectionString) {InitialCatalog = "master"}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Database = "postgres"}.ToString());
		}

		public static string ChangeDatabase(this string connectionString, string newDatabase)
		{
			return new connectionStringDialectSelector(connectionString).Select(
				() => new SqlConnectionStringBuilder(connectionString) {InitialCatalog = newDatabase}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Database = newDatabase}.ToString());
		}

		public static string ChangeServer(this string connectionString, string server)
		{
			return new connectionStringDialectSelector(connectionString).Select(
				() => new SqlConnectionStringBuilder(connectionString) {DataSource = server}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Host = server}.ToString());
		}

		public static string ChangeApplicationName(this string connectionString, string applicationName) =>
			new connectionStringDialectSelector(connectionString).Select(
				() => new SqlConnectionStringBuilder(connectionString) {ApplicationName = applicationName}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {ApplicationName = applicationName}.ToString(),
				() => connectionString);

		public static DbConnection CreateConnection(this string connectionString)
		{
			var connection = new connectionStringDialectSelector(connectionString).Select<DbConnection>(
				() => new SqlConnection(), () => new NpgsqlConnection());
			connection.ConnectionString = connectionString;
			return connection;
		}

		private class connectionStringDialectSelector : ConnectionStringDialectSelector
		{
			public connectionStringDialectSelector(string connectionString) : base(connectionString)
			{
			}

			public T Select<T>(Func<T> sqlServer, Func<T> postgres) =>
				SelectDialect(sqlServer, postgres);
			
			public T Select<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis) =>
				SelectDialect(sqlServer, postgres, redis);
		}
	}
}