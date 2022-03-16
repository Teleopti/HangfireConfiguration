using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration
{
	public class ConfigurationStorage : IConfigurationStorage
	{
		private readonly UnitOfWork _unitOfWork;

		// for testing
		public ConfigurationStorage(string connectionString) : this(new UnitOfWork {ConnectionString = connectionString})
		{
		}

		internal ConfigurationStorage(UnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		private readonly ThreadLocal<UnitOfWorkTransaction> _currentTransaction = new();
		
		private IUnitOfWork getUnitOfWork() => _currentTransaction.Value as IUnitOfWork ?? _unitOfWork;

		public void Transaction(Action action)
		{
			_currentTransaction.Value = new UnitOfWorkTransaction(_unitOfWork.ConnectionString);
			action.Invoke();
			_currentTransaction.Value.Commit();
			_currentTransaction.Value = null;
		}

		public void LockConfiguration()
		{
			var unitOfWork = getUnitOfWork();
			var sql = unitOfWork.SelectDialect(
				$@"SELECT * FROM [{SqlServerObjectsInstaller.SchemaName}].Configuration WITH (TABLOCKX)",
				$@"LOCK TABLE {SqlServerObjectsInstaller.SchemaName}.configuration");
			unitOfWork.Execute(sql);
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
			var unitOfWork = getUnitOfWork();
			var sql = unitOfWork.SelectDialect(sqlServer, postgreSql);
			return unitOfWork.Query<StoredConfiguration>(sql).ToArray();
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
			var unitOfWork = getUnitOfWork();
			var sql = unitOfWork.SelectDialect($@"
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
			unitOfWork.Execute(sql, configuration);
		}

		private void update(StoredConfiguration configuration)
		{
			var unitOfWork = getUnitOfWork();
			var sql = unitOfWork.SelectDialect($@"
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

			unitOfWork.Execute(sql, configuration);
		}
	}
}