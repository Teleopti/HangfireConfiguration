using System;
using System.Data.SqlClient;
using Hangfire.Configuration.Internals;
using Npgsql;

namespace Hangfire.Configuration
{
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
		
		public T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis)
		{
			if (isSqlServer())
				return sqlServer();
			if (IsPostgreSql())
				return postgres();
			if (_connectionString.StartsWith(DatabaseProviderExtensions.RedisStart))
				return redis();
			return default;
		}

		// should be made extension. no throwing
		public void SelectDialectVoid(Action sqlServer, Action postgres)
		{
			if (isSqlServer())
			{
				sqlServer();
				return;
			}
			if (IsPostgreSql())
			{
				postgres();
				return;
			}

			throw new Exception("Invalid connectionstring");
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
