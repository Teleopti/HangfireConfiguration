using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration
{
	public class ConfigurationStorage : IConfigurationStorage
	{
		private readonly Connector _connector;

		internal ConfigurationStorage(Connector connector)
		{
			_connector = connector;
		}

		private readonly ThreadLocal<ConnectorTransaction> _currentTransaction = new();
		
		private IConnector currentConnector() => _currentTransaction.Value as IConnector ?? _connector;

		public void Transaction(Action action)
		{
			_currentTransaction.Value = new ConnectorTransaction(_connector.ConnectionString);
			action.Invoke();
			_currentTransaction.Value.Commit();
			_currentTransaction.Value = null;
		}

		public void LockConfiguration()
		{
			var c = currentConnector();
			var sql = c.SelectDialect(
				$@"SELECT * FROM [{SqlServerObjectsInstaller.SchemaName}].Configuration WITH (TABLOCKX)",
				$@"LOCK TABLE {SqlServerObjectsInstaller.SchemaName}.configuration");
			c.Execute(sql);
		}

		public IEnumerable<StoredConfiguration> ReadConfigurations()
		{
			const string sqlServer = $@"
SELECT 
    Id, 
    Name, 
    ConnectionString, 
    SchemaName, 
    GoalWorkerCount, 
    Active,
	MaxWorkersPerServer
FROM 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration";
			const string postgreSql = $@"
SELECT 
    Id, 
    Name, 
    ConnectionString, 
    SchemaName, 
    GoalWorkerCount, 
    Active,
	MaxWorkersPerServer
FROM 
    {SqlServerObjectsInstaller.SchemaName}.configuration 
ORDER BY Id";
			var c = currentConnector();
			var sql = c.SelectDialect(sqlServer, postgreSql);
			return c.Query<StoredConfiguration>(sql).ToArray();
		}

		public void WriteConfiguration(StoredConfiguration configuration)
		{
			if (configuration.Id != null)
				update(configuration);
			else
				insert(configuration);
		}

		private void insert(StoredConfiguration configuration)
		{
			var c = currentConnector();
			var sql = c.SelectDialect($@"
INSERT INTO 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration 
    (
        Name,
    [ConnectionString], 
[SchemaName], 
GoalWorkerCount, 
Active,
MaxWorkersPerServer
    ) VALUES (
    @Name,
    @ConnectionString, 
    @SchemaName, 
    @GoalWorkerCount, 
    @Active,
    @MaxWorkersPerServer
);", $@"
INSERT INTO 
    {SqlServerObjectsInstaller.SchemaName}.Configuration 
(
    Name,
    ConnectionString, 
    SchemaName, 
    GoalWorkerCount, 
    Active,
	MaxWorkersPerServer
) VALUES (
    @Name,
    @ConnectionString, 
    @SchemaName, 
    @GoalWorkerCount, 
    @Active,
    @MaxWorkersPerServer
);");
			c.Execute(sql, configuration);
		}

		private void update(StoredConfiguration configuration)
		{
			var c = currentConnector();
			var sql = c.SelectDialect($@"
UPDATE 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration 
SET 
    Name = @Name,
    ConnectionString = @ConnectionString, 
    SchemaName = @SchemaName, 
    GoalWorkerCount = @GoalWorkerCount, 
    Active = @Active,
    MaxWorkersPerServer = @MaxWorkersPerServer    
WHERE 
    Id = @Id;", $@"
UPDATE 
    {SqlServerObjectsInstaller.SchemaName}.Configuration 
SET 
    Name = @Name,
    ConnectionString = @ConnectionString, 
    SchemaName = @SchemaName, 
    GoalWorkerCount = @GoalWorkerCount, 
    Active = @Active,
    MaxWorkersPerServer = @MaxWorkersPerServer    
WHERE 
    Id = @Id;");

			c.Execute(sql, configuration);
		}
	}
}