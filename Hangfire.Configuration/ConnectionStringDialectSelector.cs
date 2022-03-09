using System;
using System.Data.Common;
using System.Data.SqlClient;
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
