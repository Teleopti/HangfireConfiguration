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

		private ConnectorBase currentConnector() => (ConnectorBase) _currentTransaction.Value ?? _connector;

		public void Transaction(Action action)
		{
			_currentTransaction.Value = new ConnectorTransaction(_connector.ConnectionString);
			action.Invoke();
			_currentTransaction.Value.Commit();
			_currentTransaction.Value = null;
		}
		
		private string tableName()
		{
			var c = currentConnector();
			return c.SelectDialect(
				$@"[{HangfireConfigurationSchemaInstaller.SchemaName}].Configuration",
				$@"{HangfireConfigurationSchemaInstaller.SchemaName}.Configuration");
		}

		const string Columns = @"
    Name, 
    ConnectionString, 
    SchemaName, 
    GoalWorkerCount, 
    Active,
	MaxWorkersPerServer,
	WorkerBalancerEnabled";

		const string ColumnMaps = @"
	@Name,
    @ConnectionString, 
    @SchemaName, 
    @GoalWorkerCount, 
    @Active,
    @MaxWorkersPerServer,
	@WorkerBalancerEnabled";

		public void LockConfiguration()
		{
			var c = currentConnector();
			var sql = c.SelectDialect(
				$@"SELECT * FROM {tableName()} WITH (TABLOCKX)",
				$@"LOCK TABLE {tableName()}");
			c.Execute(sql);
		}

		public IEnumerable<StoredConfiguration> ReadConfigurations()
		{
			var c = currentConnector();
			var sql = $@"SELECT Id, {Columns} FROM {tableName()}";
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
			var sql = $@"INSERT INTO {tableName()} ({Columns}) VALUES ({ColumnMaps});";
			var c = currentConnector();
			c.Execute(sql, configuration);
		}

		private void update(StoredConfiguration configuration)
		{
			var sql = $@"
UPDATE 
    {tableName()}
SET 
    Name = @Name,
    ConnectionString = @ConnectionString, 
    SchemaName = @SchemaName, 
    GoalWorkerCount = @GoalWorkerCount, 
    Active = @Active,
    MaxWorkersPerServer = @MaxWorkersPerServer,
	WorkerBalancerEnabled = @WorkerBalancerEnabled
WHERE 
    Id = @Id;";
			var c = currentConnector();
			c.Execute(sql, configuration);
		}
	}
}