using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Owin;

namespace Hangfire.Configuration
{
    public interface IHangfire
    {
        IAppBuilder UseHangfireServer(
            IAppBuilder builder,
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses);

        JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options);
    }

    public class RealHangfire : IHangfire
    {
        public IAppBuilder UseHangfireServer(
            IAppBuilder builder,
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses) =>
            builder.UseHangfireServer(storage, options, additionalProcesses);

        public JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options) =>
            new SqlServerStorage(connectionString, options);
    }

    public class ServerStarter
    {
        private readonly IAppBuilder _builder;
        private readonly IHangfire _hangfire;
        private readonly IConfigurationRepository _repository;

        public ServerStarter(IAppBuilder builder, IHangfire hangfire, IConfigurationRepository repository)
        {
            _builder = builder;
            _hangfire = hangfire;
            _repository = repository;
        }

        public IEnumerable<RunningServer> StartServers(
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            SqlServerStorageOptions storageOptions,
            params IBackgroundProcess[] additionalProcesses)
        {
            var runningServers = new List<RunningServer>();
            var serverNumber = 1;

            configureDefaultStorage(options?.DefaultHangfireConnectionString, options?.DefaultSchemaName);

            //Mutation not good
            if (serverOptions != null)
                serverOptions.ServerName = null;
            else
                serverOptions = new BackgroundJobServerOptions();

            var storedConfigs = _repository.ReadConfigurations().ToArray();

            var activeConfiguration = storedConfigs.FirstOrDefault(x => x.Active ?? false) ?? storedConfigs.FirstOrDefault();
            
            storedConfigs.ForEach(storedConfig =>
            {
                var appliedStorageOptions = new SqlServerStorageOptions
                {
                    SchemaName = storedConfig.SchemaName
                };

                if (storageOptions != null)
                {
                    appliedStorageOptions.PrepareSchemaIfNecessary = storageOptions.PrepareSchemaIfNecessary;
                    appliedStorageOptions.QueuePollInterval = storageOptions.QueuePollInterval;
                    appliedStorageOptions.SlidingInvisibilityTimeout = storageOptions.SlidingInvisibilityTimeout;
                    appliedStorageOptions.JobExpirationCheckInterval = storageOptions.JobExpirationCheckInterval;
                    appliedStorageOptions.CountersAggregateInterval = storageOptions.CountersAggregateInterval;
                    appliedStorageOptions.DashboardJobListLimit = storageOptions.DashboardJobListLimit;
                    appliedStorageOptions.TransactionTimeout = storageOptions.TransactionTimeout;
                    appliedStorageOptions.DisableGlobalLocks = storageOptions.DisableGlobalLocks;
                    appliedStorageOptions.UsePageLocksOnDequeue = storageOptions.UsePageLocksOnDequeue;
                }

                var sqlJobStorage = _hangfire.MakeSqlJobStorage(storedConfig.ConnectionString, appliedStorageOptions);

                serverOptions.WorkerCount = WorkerDeterminer.DetermineWorkerCount
                (
                    sqlJobStorage.GetMonitoringApi(),
                    storedConfig.GoalWorkerCount,
                    options ?? new ConfigurationOptions()
                );

                var appliedAdditionalProcess = new IBackgroundProcess[] {};
                if ( storedConfig.Id == activeConfiguration.Id ) 
                    appliedAdditionalProcess = additionalProcesses;
                    
                _hangfire.UseHangfireServer(_builder, sqlJobStorage, serverOptions, appliedAdditionalProcess);

                runningServers.Add(new RunningServer() {Number = serverNumber, Storage = sqlJobStorage});
                serverNumber++;

            });

            return runningServers;
        }

        //TODO: unit of work
        private void configureDefaultStorage(string connectionString, string schemaName)
        {
            if (connectionString == null)
                return;
            var configurations = _repository.ReadConfigurations().ToArray();
            if (configurations.IsEmpty())
            {
                _repository.WriteConfiguration(new StoredConfiguration
                {
                    ConnectionString = connectionString,
                    SchemaName = schemaName,
                    Active = true
                });
            }
            else
            {
                var legacyConfiguration = configurations.First();
                legacyConfiguration.ConnectionString = connectionString;
                legacyConfiguration.SchemaName = schemaName;
                if (configurations.Where(x => (x.Active ?? false)).IsEmpty())
                    legacyConfiguration.Active = true;

                _repository.WriteConfiguration(legacyConfiguration);
            }
        }
    }
}