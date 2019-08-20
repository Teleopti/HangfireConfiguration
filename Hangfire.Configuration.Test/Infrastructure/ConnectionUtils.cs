using System;
using System.Data.SqlClient;

namespace Hangfire.Configuration.Test.Infrastructure
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
		
		
		private const string DefaultConnectionStringTemplate
			//= @"Server=.\sqlexpress;Database={0};Trusted_Connection=True;";
			= @"Data Source=.;Integrated Security=SSPI;Initial Catalog={0};";
		
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