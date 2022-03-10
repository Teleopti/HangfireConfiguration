using System;
using System.Data.SqlClient;
using Npgsql;

namespace Hangfire.Configuration;

public interface IDbVendorSelector
{
	T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres);
}

public static class DbVendorExtensions
{
	public static IDbVendorSelector ToDbVendorSelector(this string connectionString) => 
		new connectionStringDialectSelector(connectionString);
	
	public static void ExecuteDialect(this IDbVendorSelector selector, Action sqlServer, Action postgres)
	{
		selector.SelectDialect(() =>
		{
			sqlServer();
			return true;
		}, () =>
		{
			postgres();
			return true;
		});
	}
	
	public static T SelectDialect<T>(this IDbVendorSelector selector, T sqlServer, T postgres)
	{
		return selector.SelectDialect(() => sqlServer, () => postgres);
	}
		
	private class connectionStringDialectSelector : IDbVendorSelector
	{
		private readonly string _connectionString;
        
		public connectionStringDialectSelector(string connectionString)
		{
			_connectionString = connectionString;
		}
        
		public T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres)
		{
			if (isSqlServer())
				return sqlServer();
			if (isPostgreSql())
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
        
		private bool isPostgreSql()
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