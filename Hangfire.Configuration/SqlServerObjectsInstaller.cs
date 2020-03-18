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
        public const int SchemaVersion = 3;

        public static readonly string SqlScript = GetStringResource(
            typeof(SqlServerObjectsInstaller).GetTypeInfo().Assembly,
            "Hangfire.Configuration.Install.sql");

        public static void Install(DbConnection connection) =>
            Install(connection, SchemaVersion);

        public static void Install(DbConnection connection, int schemaVersion)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            var scriptWithSchema = SqlScript
                    .Replace("$(HangfireConfigurationSchema)", SchemaName)
                    .Replace("$(HangfireConfigurationSchemaVersion)", schemaVersion.ToString())
                ;
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