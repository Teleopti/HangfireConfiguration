using Dapper;
using Npgsql;

namespace Hangfire.Configuration
{
	public interface IHangfireSchemaCreator
	{
		void TryConnect(string connectionString);
		void CreateHangfireSchema(string schemaName, string connectionString);
		bool SchemaExists(string schemaName, string connectionString);
	}

	public class HangfireSchemaCreator : IHangfireSchemaCreator
	{
		public void TryConnect(string connectionString)
		{
			using (var conn = new ConnectionStringDialectSelector(connectionString).GetConnection())
				conn.Open();
		}

		public void CreateHangfireSchema(string schemaName, string connectionString)
		{
			
			var dialectSelector = new ConnectionStringDialectSelector(connectionString);
			using (var conn = dialectSelector.GetConnection())
			{
				
				dialectSelector.SelectDialectVoid(
					() => SqlServer.SqlServerObjectsInstaller.Install(conn, schemaName, true),
					() =>
					{
						conn.Open();
						if (string.IsNullOrWhiteSpace(schemaName))
						{
							PostgreSql.PostgreSqlObjectsInstaller.Install((NpgsqlConnection)conn);
						}
						else
						{
							PostgreSql.PostgreSqlObjectsInstaller.Install((NpgsqlConnection)conn, schemaName);
						}
					}
				);
			}
		}

		public bool SchemaExists(string schemaName, string connectionString)
		{
			using (var conn = new ConnectionStringDialectSelector(connectionString).GetConnection())
				return conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{schemaName}'") > 0;
		}
	}
}