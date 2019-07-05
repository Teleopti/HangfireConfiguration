using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using SqlSetup = Hangfire.Configuration.SqlServerObjectsInstaller;

namespace Hangfire.Configuration
{
    public interface IConfigurationRepository
    {
        void WriteGoalWorkerCount(int? workers);
        int? ReadGoalWorkerCount();
    }

    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public ConfigurationRepository(string connectionString)
        {
            _connectionFactory = () =>
            {
                var conn = new SqlConnection(connectionString);
                conn.Open();
                return conn;
            };
        }

        public void WriteGoalWorkerCount(int? workers)
        {
            using (var connection = _connectionFactory())
            {
                var updated = connection.Execute(
                    $@"UPDATE [{SqlSetup.SchemaName}].Configuration SET GoalWorkerCount = @workers WHERE Id = @id;", 
                    new {workers = workers, id = 1});
                
                if (updated == 0) 
                    connection.Execute(
                        $@"INSERT INTO [{SqlSetup.SchemaName}].Configuration ([GoalWorkerCount]) VALUES (@workers);", 
                        new {workers = workers});
            }
        }

        public int? ReadGoalWorkerCount()
        {
            using (var connection = _connectionFactory())
            {
                return connection.QueryFirstOrDefault<int?>(
                    $@"SELECT TOP 1 [GoalWorkerCount] FROM [{SqlSetup.SchemaName}].Configuration"
                );
            };
        }

        public string ReadConnectionString()
        {
            using (var connection = _connectionFactory())
            {
                return connection.QueryFirstOrDefault<string>(
                    $@"SELECT TOP 1 [ConnectionString] FROM [{SqlSetup.SchemaName}].Configuration"
                );
            };
        }

        public void SaveConnectionString(string connectionStringToBeStored)
        {
            using (var connection = _connectionFactory())
            {
                connection.Execute(
                        $@"INSERT INTO [{SqlSetup.SchemaName}].Configuration ([ConnectionString]) VALUES (@connectionString);", 
                        new {connectionString = connectionStringToBeStored});
            }
        }
    }
}