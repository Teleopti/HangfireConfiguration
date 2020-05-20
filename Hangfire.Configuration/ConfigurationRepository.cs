using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Hangfire.Configuration
{
    public interface IConfigurationRepository
    {
        IEnumerable<StoredConfiguration> ReadConfigurations(IConfigurationConnection connection = null);
        void WriteConfiguration(StoredConfiguration configuration, IConfigurationConnection connection = null);

        void UsingTransaction(Action<IConfigurationConnection> action);
        void LockConfiguration(IConfigurationConnection connection);
    }


    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly ConfigurationConnection _connection;

        public ConfigurationRepository(string connectionString) : this(new ConfigurationConnection
            {ConnectionString = connectionString})
        {
        }

        public ConfigurationRepository(ConfigurationConnection connection)
        {
            _connection = connection;
        }

        public void UsingTransaction(Action<IConfigurationConnection> action)
        {
            using (var transaction = new ConfigurationConnectionTransaction(_connection.ConnectionString))
            {
                action.Invoke(transaction);
                transaction.Commit();
            }
        }

        public void LockConfiguration(IConfigurationConnection connection)
        {
            connection.UseConnection(c =>
            {
                c.ExecuteWithRetry($@"SELECT * FROM [{SqlServerObjectsInstaller.SchemaName}].Configuration WITH (TABLOCKX)", connection.Transaction());
            });
        }

        public IEnumerable<StoredConfiguration> ReadConfigurations(IConfigurationConnection connection = null)
        {
            IEnumerable<StoredConfiguration> result = null;
            (connection ?? _connection).UseConnection(c =>
            {
                result = c.QueryWithRetry<StoredConfiguration>(
                    $@"
SELECT 
    Id, 
    Name, 
    ConnectionString, 
    SchemaName, 
    GoalWorkerCount, 
    Active 
FROM 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration",connection?.Transaction()).ToArray();
            });
            return result;
        }

        public void WriteConfiguration(StoredConfiguration configuration, IConfigurationConnection connection = null)
        {
            var conn = connection ?? _connection;
            conn.UseConnection(c =>
            {
                if (configuration.Id != null)
                    update(configuration, c, conn.Transaction());
                else
                    insert(configuration, c, conn.Transaction());
            });
        }

        private static void insert(StoredConfiguration configuration, IDbConnection connection,
            IDbTransaction transaction)
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
);", configuration,transaction);
        }

        private static void update(StoredConfiguration configuration, IDbConnection connection,
            IDbTransaction transaction)
        {
            connection.ExecuteWithRetry(
                $@"
UPDATE 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration 
SET 
    Name = @Name,
    ConnectionString = @ConnectionString, 
    SchemaName = @SchemaName, 
    GoalWorkerCount = @GoalWorkerCount, 
    Active = @Active 
WHERE 
    Id = @Id;", configuration, transaction);
        }
    }
}