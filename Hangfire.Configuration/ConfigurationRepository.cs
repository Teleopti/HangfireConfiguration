using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using SqlSetup = Hangfire.Configuration.SqlServerObjectsInstaller;

namespace Hangfire.Configuration
{
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

        public IEnumerable<StoredConfiguration> ReadConfigurations()
        {
            using (var connection = _connectionFactory())
            {
                return connection.Query<StoredConfiguration>(
                    $@"SELECT Id, ConnectionString, SchemaName, GoalWorkerCount, Active FROM [{SqlSetup.SchemaName}].Configuration"
                ).ToArray();
            };
        }

        public void WriteConfiguration(StoredConfiguration configuration)
        {
            using (var connection = _connectionFactory())
            {
                if (configuration.Id != null)
                {
                    connection.Execute(
                        $@"UPDATE [{SqlSetup.SchemaName}].Configuration SET ConnectionString = @connectionString, SchemaName = @schemaName, GoalWorkerCount = @goalWorkerCount, Active = @active WHERE Id = @id;",
                        new {id = configuration.Id, connectionString = configuration.ConnectionString, schemaName = configuration.SchemaName, goalWorkerCount = configuration.GoalWorkerCount, active = configuration.Active});
                }
                else
                {
                    connection.Execute(
                        $@"INSERT INTO [{SqlSetup.SchemaName}].Configuration ([ConnectionString], [SchemaName], GoalWorkerCount, Active) VALUES (@connectionString, @schemaName, @goalWorkerCount, @active);",
                        new {connectionString = configuration.ConnectionString, schemaName = configuration.SchemaName, goalWorkerCount = configuration.GoalWorkerCount, active = configuration.Active});
                }
            }
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

        public void ActivateStorage(int configurationId)
        {
            throw new NotImplementedException();
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