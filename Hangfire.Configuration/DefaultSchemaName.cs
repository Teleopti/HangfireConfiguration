using System.Reflection;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public static class DefaultSchemaName
    {
        public static string Name()
        {
            return typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
                .GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public).GetValue(null) as string;
        }
    }
}