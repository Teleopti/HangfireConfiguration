using System.Data.SqlClient;

namespace Hangfire.Configuration
{
    public class HangfireSchemaCreator : IHangfireSchemaCreator
    {
        public void TryConnect(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
            }
        }

        public void CreateHangfireSchema(string schemaName, string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                Hangfire.SqlServer.SqlServerObjectsInstaller.Install(conn, schemaName, true);
            }
        }
    }
}