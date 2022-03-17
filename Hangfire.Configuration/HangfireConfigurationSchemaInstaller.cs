using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Dapper;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration
{
    public static class HangfireConfigurationSchemaInstaller
    {
        public const string SchemaName = "HangfireConfiguration";
        public const int SchemaVersion = 5;

        private static readonly string sqlServerScript = getStringResource(
            typeof(HangfireConfigurationSchemaInstaller).GetTypeInfo().Assembly,
			"Hangfire.Configuration.InstallSqlServer.sql");

        private static readonly string postgreSqlScript = getStringResource(
	        typeof(HangfireConfigurationSchemaInstaller).GetTypeInfo().Assembly,
	        "Hangfire.Configuration.InstallPostgreSql.sql");

		public static void Install(DbConnection connection) =>
            Install(connection, SchemaVersion);

        public static void Install(DbConnection connection, int schemaVersion)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
			var dbScript = connection.ToDbVendorSelector().SelectDialect(
				() => sqlServerScript, 
				() => postgreSqlScript);
            var scriptWithSchema = dbScript
					.Replace("$(HangfireConfigurationSchema)", SchemaName)
                    .Replace("$(HangfireConfigurationSchemaVersion)", schemaVersion.ToString())
                ;
            connection.Execute(scriptWithSchema, commandTimeout: 0);
        }

        private static string getStringResource(Assembly assembly, string resourceName)
        {
	        using var stream = assembly.GetManifestResourceStream(resourceName);
	        if (stream == null)
		        throw new InvalidOperationException($"Requested resource `{resourceName}` was not found in the assembly `{assembly}`.");
	        using var reader = new StreamReader(stream);
	        return reader.ReadToEnd();
        }
    }
}