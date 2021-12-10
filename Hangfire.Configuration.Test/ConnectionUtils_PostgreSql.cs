using System;

namespace Hangfire.Configuration.Test
{
	public static class ConnectionUtils
	{
		private const string DatabaseVariable = "Hangfire_Configuration_SqlServer_DatabaseName";
		private const string ConnectionStringTemplateVariable 
			= "Hangfire_Configuration_SqlServer_ConnectionStringTemplate";

		private const string MasterDatabaseName = "postgres";
		private const string DefaultDatabaseName = "Hangfire.Configuration.Tests";

		private const string LoginUser = "HangfireTest";
		private const string LoginUserPassword = "test";
		
		
		private const string DefaultConnectionStringTemplate
			//= @"Data Source=.;Integrated Security=SSPI;Initial Catalog={0};";
			= @"User ID=postgres;Password=Password12!;Host=localhost;Database=""{0}"";CommandTimeout=30;Pooling=false;";

		public static string GetFakeConnectionString(string dbName = "fakeDB")
		{
			return string.Format(DefaultConnectionStringTemplate, dbName);
		}

		public static string GetFakeConnectionStringWithApplicationName(string applicationName)
		{
			return $"{GetFakeConnectionString()}Application Name={applicationName};"; // is this really right application name ??
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