using System;
using Hangfire.PostgreSql;

namespace Hangfire.Configuration.Test
{
	public static class ConnectionUtils
	{
		private const string DatabaseVariable = "Hangfire_Configuration_SqlServer_DatabaseName";

		private const string MasterDatabaseName = "postgres";
		private const string DefaultDatabaseName = "Hangfire.Configuration.Tests";

		// tests shouldnt have to do this I think
		public static string DefaultSchemaName() => new PostgreSqlStorageOptions().SchemaName;

		// pooling needs to be off for some reason?
		private const string DefaultConnectionStringTemplate
			= @"User ID=postgres;Password=root;Host=localhost;Database=""{0}"";Pooling=false;";

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
			return String.Format(DefaultConnectionStringTemplate, MasterDatabaseName);
		}

		public static string GetConnectionString()
		{
			return String.Format(DefaultConnectionStringTemplate, GetDatabaseName());
		}
	}
}