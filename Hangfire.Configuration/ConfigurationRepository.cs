using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using SqlSetup = Hangfire.Configuration.SqlServerObjectsInstaller;

namespace Hangfire.Configuration
{
    public interface IConfigurationRepository
    {
        void WriteGoalWorkerCount(int? workers);
        int? ReadGoalWorkerCount();
        StoredConfiguration ReadConfiguration();
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

        public StoredConfiguration ReadConfiguration()
        {
            throw new NotImplementedException();
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

        public void SaveConnectionString(string connectionString)
        {
            using (var connection = _connectionFactory())
            {
                var updated = connection.Execute(
                    $@"UPDATE [{SqlSetup.SchemaName}].Configuration SET ConnectionString = @connectionString, Active = @active WHERE Id = @id;",
                    new {connectionString = connectionString, active = 1, id = 1});

                if (updated == 0)
                    connection.Execute(
                        $@"INSERT INTO [{SqlSetup.SchemaName}].Configuration ([ConnectionString], [Active]) VALUES (@connectionString, @active);",
                        new {connectionString = connectionString, active = 1});
            }
        }

        public bool? IsActive()
        {
            using (var connection = _connectionFactory())
            {
                return connection.QueryFirstOrDefault<bool?>(
                    $@"SELECT TOP 1 [Active] FROM [{SqlSetup.SchemaName}].Configuration"
                );
            };
        }
    }
}