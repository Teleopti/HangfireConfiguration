using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public interface IConfigurationStorage
    {
        IEnumerable<StoredConfiguration> ReadConfigurations(IUnitOfWork unitOfWork = null);
        void WriteConfiguration(StoredConfiguration configuration, IUnitOfWork unitOfWork = null);

        void UnitOfWork(Action<IUnitOfWork> action);
        void LockConfiguration(IUnitOfWork unitOfWork);
    }

    public class ConfigurationStorage : IConfigurationStorage
    {
        private readonly UnitOfWork _unitOfWork;

        public ConfigurationStorage(string connectionString) : this(new UnitOfWork{ConnectionString = connectionString}){}

        internal ConfigurationStorage(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void UnitOfWork(Action<IUnitOfWork> action)
        {
            using (var transaction = new UnitOfWorkTransaction(_unitOfWork.ConnectionString))
            {
                action.Invoke(transaction);
                transaction.Commit();
            }
        }

        public void LockConfiguration(IUnitOfWork unitOfWork)
        {
            unitOfWork.Execute($@"SELECT * FROM [{SqlServerObjectsInstaller.SchemaName}].Configuration WITH (TABLOCKX)");
        }

        public IEnumerable<StoredConfiguration> ReadConfigurations(IUnitOfWork unitOfWork = null) =>
            getUnitOfWork(unitOfWork)
                .Query<StoredConfiguration>(
                    $@"
SELECT 
    Id, 
    Name, 
    ConnectionString, 
    SchemaName, 
    GoalWorkerCount, 
    Active 
FROM 
    [{SqlServerObjectsInstaller.SchemaName}].Configuration").ToArray();

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
            unitOfWork.Execute(
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

        private static void update(StoredConfiguration configuration, IUnitOfWork unitOfWork)
        {
            unitOfWork.Execute(
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
    Id = @Id;", configuration);
        }
    }
}