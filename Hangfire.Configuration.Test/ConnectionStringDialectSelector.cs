using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Npgsql;

namespace Hangfire.Configuration.Test
{
	//TODO: needed?
	public static class DefaultSchemaName
	{
		public static string SqlServer() =>
			typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
				.GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public).GetValue(null) as string;

		public static string Postgres() => new PostgreSqlStorageOptions().SchemaName;
	}
}
