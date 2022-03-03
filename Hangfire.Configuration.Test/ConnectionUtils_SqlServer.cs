using System;
using System.Reflection;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Test
{
	public static class ConnectionUtils
	{
		private const string DatabaseVariable = "Hangfire_Configuration_SqlServer_DatabaseName";
		private const string ConnectionStringTemplateVariable 
			= "Hangfire_Configuration_SqlServer_ConnectionStringTemplate";

		private const string MasterDatabaseName = "master";
		private const string DefaultDatabaseName = @"Hangfire.Configuration.SqlServer.Tests";

		private const string LoginUser = "HangfireTest";
		private const string LoginUserPassword = "test";
		
		// tests shouldnt have to do this I think
		public static string DefaultSchemaName() =>
			typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
				.GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public).GetValue(null) as string;

		private const string DefaultConnectionStringTemplate
			//= @"Server=.\sqlexpress;Database={0};Trusted_Connection=True;";
			= @"Data Source=.;Integrated Security=SSPI;Initial Catalog={0};";
		
		public static string GetFakeConnectionString(string dbName = "fakeDB")
		{
			return string.Format(DefaultConnectionStringTemplate, dbName);
		}

		public static string GetFakeConnectionStringWithApplicationName(string applicationName)
		{
			return $"{GetFakeConnectionString()}Application Name={applicationName};";
		}

		public static string GetDatabaseName()
		{
			return Environment.GetEnvironmentVariable(DatabaseVariable) ?? DefaultDatabaseName;
		}

		public static string GetMasterConnectionString()
		{
			return String.Format(getConnectionStringTemplate(), MasterDatabaseName);
		}

		public static string GetConnectionString()
		{
			return String.Format(getConnectionStringTemplate(), GetDatabaseName());
		}

		public static string GetLoginUser()
		{
			return LoginUser;
		}

		public static string GetLoginUserPassword()
		{
			return LoginUserPassword;
		}

		private static string getConnectionStringTemplate()
		{
			return Environment.GetEnvironmentVariable(ConnectionStringTemplateVariable)
				   ?? DefaultConnectionStringTemplate;
		}
	}
}