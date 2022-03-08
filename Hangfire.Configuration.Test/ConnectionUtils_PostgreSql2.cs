using System;

namespace Hangfire.Configuration.Test
{
	public static class ConnectionUtilsPostgres
	{
		private const string DatabaseVariable = "Hangfire_Configuration_SqlServer_DatabaseName";

		private const string MasterDatabaseName = "postgres";
		private const string DefaultDatabaseName = "Hangfire.Configuration.Tests";

		// pooling needs to be off for some reason?
		private const string DefaultConnectionStringTemplate
			= @"User ID=postgres;Password=root;Host=localhost;Database=""{0}"";Pooling=false;";

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
	}
}