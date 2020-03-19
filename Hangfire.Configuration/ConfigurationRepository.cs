using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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


        public ConfigurationRepository(string connectionString) : this(new ConfigurationConnection {ConnectionString = connectionString})
        {
        }

        public ConfigurationRepository(ConfigurationConnection options)
        {
            _connectionFactory = () =>
            {
                var conn = new SqlConnection(options.ConnectionString);
                conn.OpenWithRetry();
                return conn;
            };
        }

        public IEnumerable<StoredConfiguration> ReadConfigurations()
        {
            using (var connection = _connectionFactory())
                return connection.QueryWithRetry<StoredConfiguration>(
                    $@"
SELECT 
    Id, 
    Name, 
    ConnectionString, 
    SchemaName, 
    GoalWorkerCount, 
    Active 
FROM 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration
").ToArray();
        }

        public void WriteConfiguration(StoredConfiguration configuration)
        {
            using (var connection = _connectionFactory())
            {
                if (configuration.Id != null)
                    update(configuration, connection);
                else
                    insert(configuration, connection);
            }
        }

        private static void insert(StoredConfiguration configuration, IDbConnection connection)
        {
            connection.ExecuteWithRetry(
                $@"
INSERT INTO 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration 
(
    Name,
    [ConnectionString], 
    [SchemaName], 
    GoalWorkerCount, 
    Active
) VALUES (
    @Name,
    @ConnectionString, 
    @SchemaName, 
    @GoalWorkerCount, 
    @Active
);", configuration);
        }

        private static void update(StoredConfiguration configuration, IDbConnection connection)
        {
            connection.ExecuteWithRetry(
                $@"
UPDATE 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration 
SET 
    ConnectionString = @ConnectionString, 
    SchemaName = @SchemaName, 
    GoalWorkerCount = @GoalWorkerCount, 
    Active = @Active 
WHERE 
    Id = @Id;", configuration);
        }
    }
}