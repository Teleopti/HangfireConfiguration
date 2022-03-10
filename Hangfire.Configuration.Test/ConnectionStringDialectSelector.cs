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
