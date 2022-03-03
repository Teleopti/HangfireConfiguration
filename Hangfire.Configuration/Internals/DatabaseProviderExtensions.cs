using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace Hangfire.Configuration.Internals
{
	internal static class DatabaseProviderExtensions
	{
		public static string DatabaseName(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).InitialCatalog,
				() => new NpgsqlConnectionStringBuilder(connectionString).Database);
		}

		public static string ServerName(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).DataSource,
				() => new NpgsqlConnectionStringBuilder(connectionString).Host);
		}

		public static string ApplicationName(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).ApplicationName,
				() => new NpgsqlConnectionStringBuilder(connectionString).ApplicationName);
		}

		public static string PointToMasterDatabase(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) { Database = "postgres" }.ToString());
		}

		public static string ChangeDatabase(this string connectionString, string newDatabase)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) { InitialCatalog = newDatabase }.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) { Database = newDatabase }.ToString());
		}

		public static string ChangeServer(this string connectionString, string server)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) { DataSource = server }.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) { Host = server }.ToString());
		}

		public static string UserName(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).UserID,
				() => new NpgsqlConnectionStringBuilder(connectionString).Username
			);
		}

		public static string Password(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).Password,
				() => new NpgsqlConnectionStringBuilder(connectionString).Password
			);
		}

		public static string SetUserNameAndPassword(this string connectionString, string userName, string password)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() =>
				{
					var ret= new SqlConnectionStringBuilder(connectionString) { UserID = userName, Password = password };
					ret.Remove("Integrated security");
					return ret.ToString();
				},
				() =>
				{
					var ret = new NpgsqlConnectionStringBuilder(connectionString) { Username = userName, Password = password };
					ret.Remove("Integrated security");
					return ret.ToString();
				}
			);
		}

		public static bool IntegratedSecurity(this string connectionString)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString).IntegratedSecurity,
				() => new NpgsqlConnectionStringBuilder(connectionString).IntegratedSecurity);
		}

		public static string SetCredentials(this string connectionString, bool useIntegratedSecurity, string userName, string password)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() =>
				{
					if (useIntegratedSecurity)
					{
						var ret = new SqlConnectionStringBuilder(connectionString) { IntegratedSecurity = true };
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
						return new NpgsqlConnectionStringBuilder(connectionString) 
							{ IntegratedSecurity = true, Username = null, Password = null}.ToString();
					}
					return SetUserNameAndPassword(connectionString, userName, password);
				});
		}

		public static string ChangeApplicationName(this string connectionString, string applicationName)
		{
			return new ConnectionStringDialectSelector(connectionString).SelectDialect(
				() => new SqlConnectionStringBuilder(connectionString) { ApplicationName = applicationName }.ToString(),
				() => new NpgsqlConnectionStringBuilder(connectionString) { ApplicationName = applicationName }.ToString());
		}

		public static DbConnection CreateConnection(this string connectionString)
		{
			var connection = new ConnectionStringDialectSelector(connectionString).SelectDialect<DbConnection>(
				() => new SqlConnection(), () => new NpgsqlConnection());
			connection.ConnectionString = connectionString;
			return connection;
		}
		
		//
		// public static IDialectSelector ToDialectSelector(this string connectionString)
		// {
		// 	return new connectionStringDialectSelector(connectionString);
		// }
		//
		//
		// private class connectionStringDialectSelector : IDialectSelector
		// {
		// 	private readonly string _connectionString;
		//
		// 	public connectionStringDialectSelector(string connectionString)
		// 	{
		// 		_connectionString = connectionString;
		// 	}
		//
		// 	public T Select<T>(Func<T> sqlServer, Func<T> postgres)
		// 	{
		// 		if (isSqlServer())
		// 			return sqlServer();
		// 		if (isPostgres())
		// 			return postgres();
		// 		return default;
		// 	}
		//
		// 	private bool isSqlServer()
		// 	{
		// 		try
		// 		{
		// 			new SqlConnectionStringBuilder(_connectionString);
		// 			return true;
		// 		}
		// 		catch (Exception)
		// 		{
		// 			return false;
		// 		}
		// 	}
		//
		// 	private bool isPostgres()
		// 	{
		// 		try
		// 		{
		// 			new NpgsqlConnectionStringBuilder(_connectionString);
		// 			return true;
		// 		}
		// 		catch (Exception)
		// 		{
		// 			return false;
		// 		}
		// 	}
		//}
	}
}
