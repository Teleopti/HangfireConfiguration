using System.Reflection;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public static class DefaultSchemaName
    {
        public static string Name(string connectionString)
        {
	        return new ConnectionStringDialectSelector(connectionString).SelectDialect(
		        () => typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
			        .GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public).GetValue(null) as string,
		        () => new PostgreSqlStorageOptions().SchemaName
		        );
        }
    }
}