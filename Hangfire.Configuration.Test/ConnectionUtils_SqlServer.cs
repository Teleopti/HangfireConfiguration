using System;

namespace Hangfire.Configuration.Test
{
	public static class ConnectionUtils
	{
		private const string DatabaseVariable = "Hangfire_Configuration_SqlServer_DatabaseName";

		private const string MasterDatabaseName = "master";
		private const string DefaultDatabaseName = @"Hangfire.Configuration.SqlServer.Tests";

		private const string LoginUser = "HangfireTest";
		private const string LoginUserPassword = "test";
		
		private const string DefaultConnectionStringTemplate
			= @"Data Source=.;Integrated Security=True;Initial Catalog={0};";
		
		public static string GetDatabaseName()
		{
			return Environment.GetEnvironmentVariable(DatabaseVariable) ?? DefaultDatabaseName;
		}

		public static string GetMasterConnectionString()
		{
			return String.Format(DefaultConnectionStringTemplate, MasterDatabaseName);
		}

		public static string GetConnectionString()
		{
			return String.Format(DefaultConnectionStringTemplate, GetDatabaseName());
		}

		public static string GetLoginUser()
		{
			return LoginUser;
		}

		public static string GetLoginUserPassword()
		{
			return LoginUserPassword;
		}
	}
}