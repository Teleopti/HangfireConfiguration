using System.Data.SqlClient;
using Dapper;

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
            using (var conn = new SqlConnection(connectionString))
                conn.Open();
        }

        public void CreateHangfireSchema(string schemaName, string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
                SqlServer.SqlServerObjectsInstaller.Install(conn, schemaName, true);
        }

        public bool SchemaExists(string schemaName, string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
                return conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{schemaName}'") > 0;
        }
    }
}