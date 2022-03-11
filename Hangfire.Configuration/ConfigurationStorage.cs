using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
	public class ConfigurationStorage : IConfigurationStorage
	{
		private readonly UnitOfWork _unitOfWork;

		public ConfigurationStorage(string connectionString) : this(new UnitOfWork {ConnectionString = connectionString})
		{
		}

		internal ConfigurationStorage(UnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void UnitOfWork(Action<IUnitOfWork> action)
		{
			using var transaction = new UnitOfWorkTransaction(_unitOfWork.ConnectionString);
			action.Invoke(transaction);
			transaction.Commit();
		}

		public void LockConfiguration(IUnitOfWork unitOfWork)
		{
			var sql = unitOfWork.SelectDialect(
				$@"SELECT * FROM [{SqlServerObjectsInstaller.SchemaName}].Configuration WITH (TABLOCKX)",
				$@"SELECT * FROM {SqlServerObjectsInstaller.SchemaName}.configuration");
			unitOfWork.Execute(sql);
		}

		public IEnumerable<StoredConfiguration> ReadConfigurations(IUnitOfWork unitOfWork = null)
		{
			var sqlServer = $@"
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
			var postgreSql = $@"
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
			unitOfWork = getUnitOfWork(unitOfWork);
			return unitOfWork.SelectDialect(
				() => unitOfWork.Query<StoredConfiguration>(sqlServer).ToArray(),
				() => unitOfWork.Query<StoredConfiguration>(postgreSql).ToArray());
		}

		public void WriteConfiguration(StoredConfiguration configuration, IUnitOfWork unitOfWork = null)
		{
			if (configuration.Id != null)
				update(configuration, getUnitOfWork(unitOfWork));
			else
				insert(configuration, getUnitOfWork(unitOfWork));
		}

		private IUnitOfWork getUnitOfWork(IUnitOfWork unitOfWork)
		{
			return (unitOfWork ?? _unitOfWork);
		}

		private static void insert(StoredConfiguration configuration, IUnitOfWork unitOfWork)
		{
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

		private static void update(StoredConfiguration configuration, IUnitOfWork unitOfWork)
		{
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