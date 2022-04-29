using System;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace Hangfire.Configuration.Internals;

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

	public static T SelectDialect<T>(this IDbVendorSelector selector, Func<T> sqlServer, Func<T> postgres) =>
		selector.SelectDialect(sqlServer, postgres, () => default);

	public static T SelectDialect<T>(this IDbVendorSelector selector, T sqlServer, T postgres, T redis = default) =>
		selector.SelectDialect(() => sqlServer, () => postgres, () => redis);

	public static string ApplicationName(this string connectionString) =>
		connectionString.ToDbVendorSelector().SelectDialect(
			() => new SqlConnectionStringBuilder(connectionString).ApplicationName,
			() => new NpgsqlConnectionStringBuilder(connectionString).ApplicationName);

	public static string ChangeApplicationName(this string connectionString, string applicationName) =>
		connectionString.ToDbVendorSelector().SelectDialect(
			() => new SqlConnectionStringBuilder(connectionString) {ApplicationName = applicationName}.ToString(),
			() => new NpgsqlConnectionStringBuilder(connectionString) {ApplicationName = applicationName}.ToString(),
			() => connectionString);


	public static string SetUserNameAndPassword(this string connectionString, string userName, string password)
	{
		return new connectionStringDialectSelector(connectionString).SelectDialect(
			() =>
			{
				var ret = new SqlConnectionStringBuilder(connectionString) {UserID = userName, Password = password};
				ret.Remove("Integrated security");
				return ret.ToString();
			},
			() =>
			{
				var ret = new NpgsqlConnectionStringBuilder(connectionString) {Username = userName, Password = password};
				ret.Remove("Integrated security");
				return ret.ToString();
			}
		);
	}

	public static string SetCredentials(this string connectionString, bool useIntegratedSecurity, string userName, string password)
	{
		return new connectionStringDialectSelector(connectionString).SelectDialect(
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
			() =>
			{
				if (useIntegratedSecurity)
				{
					var ret = new NpgsqlConnectionStringBuilder(connectionString)
					{
						IntegratedSecurity = false,
						Username = null,
						Password = null
					};
					return NpgsqlConnectionStringBuilderWorkaround.SetIntegratedSecurity(ret.ToString());
				}

				return SetUserNameAndPassword(connectionString, userName, password);
			});
	}

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
				NpgsqlConnectionStringBuilderWorkaround.Parse(_connectionString);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}