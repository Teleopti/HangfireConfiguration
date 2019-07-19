using System;
using System.Collections.Generic;
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
        IEnumerable<StoredConfiguration> ReadConfiguration();
        void WriteNewStorageConfiguration(string connectionString, string schemaName, bool active);
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
                    $@"UPDATE [{SqlSetup.SchemaName}].Configuration SET GoalWorkerCount = @workers WHERE Active = @active;",
                    new {workers = workers, active = 1});

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

        public void WriteDefaultConfiguration(string connectionString, string schemaName)
        {
            using (var connection = _connectionFactory())
            {
                var updated = connection.Execute(
                    $@"UPDATE [{SqlSetup.SchemaName}].Configuration SET ConnectionString = @connectionString, Active = @active, SchemaName = @schemaName WHERE ConnectionString IS NULL;",
                    new {connectionString = connectionString, active = 1, schemaName = schemaName});

                if (updated == 0)
                    connection.Execute(
                        $@"INSERT INTO [{SqlSetup.SchemaName}].Configuration ([ConnectionString], [Active], [SchemaName]) VALUES (@connectionString, @active, @schemaName);",
                        new {connectionString = connectionString, active = 1, schemaName = schemaName});
            }
        }

        public void WriteNewStorageConfiguration(string connectionString, string schemaName, bool active)
        {
            using (var connection = _connectionFactory())
            {
                connection.Execute(
                    $@"INSERT INTO [{SqlSetup.SchemaName}].Configuration ([ConnectionString], [SchemaName], [Active]) VALUES (@connectionString, @schemaName, @active);",
                    new {connectionString = connectionString, schemaName = schemaName, active = 0});
            }
        }
        
        public IEnumerable<StoredConfiguration> ReadConfiguration()
        {
            using (var connection = _connectionFactory())
            {
                return connection.Query<StoredConfiguration>(
                    $@"SELECT Id, ConnectionString, SchemaName, GoalWorkerCount as Workers, Active  FROM [{SqlSetup.SchemaName}].Configuration"
                ).ToArray();
            };
        }

        public string ReadActiveConfigurationConnectionString()
        {
            using (var connection = _connectionFactory())
            {
                return connection.QuerySingleOrDefault<string>(
                    $@"SELECT [ConnectionString] FROM [{SqlSetup.SchemaName}].Configuration WHERE Active = @active",
                    new {active = 1}
                );
            };
        }
    }
}