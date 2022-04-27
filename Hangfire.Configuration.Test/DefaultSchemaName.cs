using System.Reflection;
using Hangfire.PostgreSql;
#if Redis
using Hangfire.Pro.Redis;
#endif
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Test
{
	//TODO: needed?
	public static class DefaultSchemaName
	{
		public static string SqlServer() =>
			typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
				.GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public).GetValue(null) as string;

		public static string Postgres() => new PostgreSqlStorageOptions().SchemaName;

#if Redis
		public static string Redis() => new RedisStorageOptions().Prefix;
#endif
		
	}
}
