using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Dapper;

namespace Hangfire.Configuration
{
    public static class SqlServerObjectsInstaller
    {
        public const string SchemaName = "HangfireConfiguration";
        public const int SchemaVersion = 5;

        public static readonly string SqlScript = GetStringResource(
            typeof(SqlServerObjectsInstaller).GetTypeInfo().Assembly,
			"Hangfire.Configuration.InstallSqlServer.sql");

        public static readonly string PostgreSqlScript = GetStringResource(
	        typeof(SqlServerObjectsInstaller).GetTypeInfo().Assembly,
	        "Hangfire.Configuration.InstallPostgreSql.sql");

		public static void Install(DbConnection connection) =>
            Install(connection, SchemaVersion);

        public static void Install(DbConnection connection, int schemaVersion)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
			var dbScript= new ConnectionStringDialectSelector(connection.ConnectionString).SelectDialect(
				() => SqlScript, 
				() => PostgreSqlScript);
            var scriptWithSchema = dbScript
					.Replace("$(HangfireConfigurationSchema)", SchemaName)
                    .Replace("$(HangfireConfigurationSchemaVersion)", schemaVersion.ToString())
                ;
   //         var cmd = new NpgsqlCommand(scriptWithSchema, (NpgsqlConnection)connection);
			//connection.Open();
			// cmd.CommandType = CommandType.Text;
            //cmd.ExecuteNonQuery();
			//connection.Close();
            connection.Execute(scriptWithSchema, commandTimeout: 0);
        }

        private static string GetStringResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Requested resource `{resourceName}` was not found in the assembly `{assembly}`.");
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }
}