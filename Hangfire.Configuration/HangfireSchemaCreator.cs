using Dapper;
using Hangfire.Configuration.Internals;
using Npgsql;

namespace Hangfire.Configuration
{
	public class HangfireSchemaCreator : IHangfireSchemaCreator
	{
		public void TryConnect(string connectionString)
		{
			using var conn = connectionString.CreateConnection();
			conn.Open();
		}

		public void CreateHangfireStorageSchema(string schemaName, string connectionString)
		{
			var dialectSelector = new ConnectionStringDialectSelector(connectionString);

			dialectSelector.SelectDialectVoid(
				() =>
				{
					using var conn = connectionString.CreateConnection();
					conn.Open();
					SqlServer.SqlServerObjectsInstaller.Install(conn, schemaName, true);
				},
				() =>
				{
					using var conn = connectionString.CreateConnection();
					conn.Open();
					if (string.IsNullOrWhiteSpace(schemaName))
						PostgreSql.PostgreSqlObjectsInstaller.Install((NpgsqlConnection) conn);
					else
						PostgreSql.PostgreSqlObjectsInstaller.Install((NpgsqlConnection) conn, schemaName);
				}
			);
		}

		public bool HangfireStorageSchemaExists(string schemaName, string connectionString)
		{
			using var conn = connectionString.CreateConnection();
			return conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{schemaName}'") > 0;
		}
	}
}