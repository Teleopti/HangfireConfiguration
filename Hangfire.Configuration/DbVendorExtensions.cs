using System;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace Hangfire.Configuration;

internal static class DbVendorExtensions
{
	private const string redisStart = "redis$$";
	
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
	
	public static T SelectDialect<T>(this IDbVendorSelector selector, Func<T> sqlServer, Func<T> postgres) => 
		selector.SelectDialect(sqlServer, postgres, () => default);

	public static T SelectDialect<T>(this IDbVendorSelector selector, T sqlServer, T postgres, T redis = default) => 
		selector.SelectDialect(() => sqlServer, () => postgres, () => redis);

	public static string TrimRedisPrefix(this string connectionString) =>
		connectionString.ToDbVendorSelector().SelectDialect(
			() => connectionString,
			() => connectionString,
			() => connectionString.Substring(redisStart.Length)) 
		?? connectionString;

	public static string ApplicationName(this string connectionString) =>
		connectionString.ToDbVendorSelector().SelectDialect(
			() => new SqlConnectionStringBuilder(connectionString).ApplicationName,
			() => new NpgsqlConnectionStringBuilder(connectionString).ApplicationName);

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
			return redis();
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