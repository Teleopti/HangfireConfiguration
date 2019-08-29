using System.Data.SqlClient;

namespace Hangfire.Configuration
{
    public interface IHangfireSchemaCreator
    {
        void TryConnect(string connectionString);
        void CreateHangfireSchema(string schemaName, string connectionStringForCreate);
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
                Hangfire.SqlServer.SqlServerObjectsInstaller.Install(conn, schemaName, true);
        }
    }
}