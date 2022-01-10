using System;
using System.Data;
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
			if (_connectionString == null)
				throw new ArgumentException("Connectionstring is null");
		}

		public T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres)
		{
			if (isSqlServer())
				return sqlServer();
			if (IsPostgreSql())
				return postgres();
			throw new Exception("Invalid connectionstring");
		}

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

		public DbConnection GetConnection()
		{
			if (isSqlServer())
				return new SqlConnection(_connectionString);
			if (IsPostgreSql())
				return new NpgsqlConnection(_connectionString);
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
