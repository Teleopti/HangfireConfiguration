using System;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace Hangfire.Configuration.Internals;

internal static class DbVendorExtensions
{
	public static IDbSelector ToDbSelector(this string connectionString) =>
		new connectionStringDialectSelector(connectionString);

	public static IDbSelector ToDbSelector(this DbConnection dbConnection) =>
		ToDbSelector(dbConnection.ConnectionString);

	public static void PickAction(this IDbSelector selector, Action sqlServer, Action postgres, Action redis = null)
	{
		selector.PickFunc(() =>
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

	public static T PickFunc<T>(this IDbSelector selector, Func<T> sqlServer, Func<T> postgres) =>
		selector.PickFunc(sqlServer, postgres, () => default);

	public static T PickDialect<T>(this IDbSelector selector, T sqlServer, T postgres, T redis = default) =>
		selector.PickFunc(() => sqlServer, () => postgres, () => redis);

	public static string ApplicationName(this string connectionString) =>
		connectionString.ToDbSelector().PickFunc(
			() => new SqlConnectionStringBuilder(connectionString).ApplicationName,
			() => new NpgsqlConnectionStringBuilder(connectionString).ApplicationName);

	public static string ChangeApplicationName(this string connectionString, string applicationName) =>
		connectionString.ToDbSelector().PickFunc(
			() => new SqlConnectionStringBuilder(connectionString) {ApplicationName = applicationName}.ToString(),
			() => new NpgsqlConnectionStringBuilder(connectionString) {ApplicationName = applicationName}.ToString(),
			() => connectionString);
	
	public static string SetUserNameAndPassword(this string connectionString, string userName, string password)
	{
		return new connectionStringDialectSelector(connectionString).PickFunc(
			() =>
			{
				var ret = new SqlConnectionStringBuilder(connectionString)
				{
					UserID = userName, 
					Password = password
				};
				ret.Remove("Integrated security");
				return ret.ToString();
			},
			() => new NpgsqlConnectionStringBuilder(connectionString)
			{
				Username = userName,
				Password = password
			}.ToString());
	}

	public static string SetCredentials(this string connectionString, bool useIntegratedSecurity, string userName, string password)
	{
		return new connectionStringDialectSelector(connectionString).PickFunc(
			() =>
			{
				if (useIntegratedSecurity)
				{
					var ret = new SqlConnectionStringBuilder(connectionString) {IntegratedSecurity = true};
					ret.Remove("User Id");
					ret.Remove("Password");
					return ret.ToString();
				}

				return SetUserNameAndPassword(connectionString, userName, password);
			},
			() => SetUserNameAndPassword(connectionString, userName, password));
	}

	public static DbConnection CreateConnection(this string connectionString)
	{
		var connection = connectionString.ToDbSelector().PickFunc<DbConnection>(
			() => new SqlConnection(), () => new NpgsqlConnection());
		connection.ConnectionString = connectionString;
		return connection;
	}

	private class connectionStringDialectSelector : IDbSelector
	{
		private readonly string _connectionString;

		public connectionStringDialectSelector(string connectionString)
		{
			_connectionString = connectionString;
		}

		public T PickFunc<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis)
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