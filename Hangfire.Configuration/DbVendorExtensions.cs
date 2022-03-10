using System;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace Hangfire.Configuration;

public interface IDbVendorSelector
{
	T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis);
}

internal static class DbVendorExtensions
{
	public static IDbVendorSelector ToDbVendorSelector(this string connectionString) => 
		new connectionStringDialectSelector(connectionString);
	
	public static IDbVendorSelector ToDbVendorSelector(this DbConnection dbConnection) => 
		ToDbVendorSelector(dbConnection.ConnectionString);

	public static void ExecuteDialect(this IDbVendorSelector selector, Action sqlServer, Action postgres, Action redis = null)
	{
		selector.SelectDialect(() =>
		{
			sqlServer();
			return true;
		}, () =>
		{
			postgres();
			return true;
		}, () =>
		{
			redis();
			return true;
		});
	}
	
	public static T SelectDialect<T>(this IDbVendorSelector selector, Func<T> sqlServer, Func<T> postgres)
	{
		return selector.SelectDialect(sqlServer, postgres, () => default);
	}
	
	public static T SelectDialect<T>(this IDbVendorSelector selector, T sqlServer, T postgres, T redis = default)
	{
		return selector.SelectDialect(() => sqlServer, () => postgres, () => redis);
	}
		
	private class connectionStringDialectSelector : IDbVendorSelector
	{
		private readonly string _connectionString;
        
		public connectionStringDialectSelector(string connectionString)
		{
			_connectionString = connectionString;
		}
        
		public T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis)
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