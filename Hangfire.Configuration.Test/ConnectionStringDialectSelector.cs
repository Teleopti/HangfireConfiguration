using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Npgsql;

namespace Hangfire.Configuration.Test
{
	//TODO: needed?
	public static class DefaultSchemaName
	{
		public static string SqlServer() =>
			typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
				.GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public).GetValue(null) as string;

		public static string Postgres() => new PostgreSqlStorageOptions().SchemaName;
	}
	
	//TODO - does this need to be kept?
	public static class ConnectionStringExtensions
	{
		public static string DatabaseName(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).InitialCatalog,
				() => new NpgsqlConnectionStringBuilder(connectionString).Database);
		}
		
		public static string PointToMasterDatabase(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) {InitialCatalog = "master"}.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) {Database = "postgres"}.ToString());
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
	}
	
	//TODO - does this need to be kept?
	internal class ConnectionStringDialectSelector
	{
		private readonly string _connectionString;

		public ConnectionStringDialectSelector(string connectionString)
		{
			_connectionString = connectionString;
		}

		public T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres)
		{
			if (isSqlServer())
				return sqlServer();
			if (IsPostgreSql())
				return postgres();
			return default;
		}

		private bool isSqlServer()
		{
			try
			{
				new SqlConnectionStringBuilder(_connectionString);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		// should be private
		public bool IsPostgreSql()
		{
			try
			{
				new NpgsqlConnectionStringBuilder(_connectionString);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
