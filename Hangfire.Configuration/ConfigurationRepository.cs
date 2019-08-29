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
        IEnumerable<StoredConfiguration> ReadConfigurations();
        void WriteConfiguration(StoredConfiguration configuration);
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
    }
}