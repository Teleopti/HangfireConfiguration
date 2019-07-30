using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Hangfire.Storage;
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

            foreach (var storedConfig in storedConfigs)
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
                    appliedStorageOptions.InvisibilityTimeout = storageOptions.InvisibilityTimeout;
                    appliedStorageOptions.JobExpirationCheckInterval = storageOptions.JobExpirationCheckInterval;
                    appliedStorageOptions.CountersAggregateInterval = storageOptions.CountersAggregateInterval;
                    appliedStorageOptions.DashboardJobListLimit = storageOptions.DashboardJobListLimit;
                    appliedStorageOptions.TransactionTimeout = storageOptions.TransactionTimeout;
                    appliedStorageOptions.DisableGlobalLocks = storageOptions.DisableGlobalLocks;
                    appliedStorageOptions.UsePageLocksOnDequeue = storageOptions.UsePageLocksOnDequeue;
                }

                var sqlJobStorage = _hangfire.MakeSqlJobStorage(storedConfig.ConnectionString, appliedStorageOptions);
                
                serverOptions.WorkerCount = determineWorkerCount(sqlJobStorage.GetMonitoringApi(), storedConfig);

                _hangfire.UseHangfireServer(_builder, sqlJobStorage, serverOptions, additionalProcesses);

                runningServers.Add(new RunningServer() {Number = serverNumber, Storage = sqlJobStorage});
                serverNumber++;

                additionalProcesses = new IBackgroundProcess[] { };
            }

            return runningServers;
        }

        private void configureDefaultStorage(string connectionString, string schemaName)
        {
            if (connectionString == null)
                return;
            var configurations = _repository.ReadConfigurations();
            var legacyConfiguration = configurations.SingleOrDefault(x => x.ConnectionString == null);

            if (legacyConfiguration != null)
            {
                legacyConfiguration.ConnectionString = connectionString;
                legacyConfiguration.SchemaName = schemaName;
                legacyConfiguration.Active = true;
                _repository.WriteConfiguration(legacyConfiguration);
            }

            if (!configurations.Any())
            {
                _repository.WriteConfiguration(new StoredConfiguration
                {
                    ConnectionString = connectionString,
                    SchemaName = schemaName,
                    Active = true
                });
            }
        }
        
        private int determineWorkerCount(IMonitoringApi monitor, StoredConfiguration storedConfiguration) => 
            determineWorkerCount(monitor, storedConfiguration, new WorkerCalculationOptions
            { 
                DefaultGoalWorkerCount = 10, 
                MinimumWorkerCount = 1,
                MaximumGoalWorkerCount = 100, 
                MinimumServers = 2
            });

        private int determineWorkerCount(IMonitoringApi monitor, StoredConfiguration storedConfiguration, WorkerCalculationOptions options)
        {
            var goalWorkerCount = storedConfiguration.GoalWorkerCount ?? options.DefaultGoalWorkerCount; 
			
            if (goalWorkerCount <= options.MinimumWorkerCount)
                return options.MinimumWorkerCount;
			
            if (goalWorkerCount > options.MaximumGoalWorkerCount)
                goalWorkerCount = options.MaximumGoalWorkerCount;
			
            var serverCount = monitor.Servers().Count; 
            if (serverCount < options.MinimumServers)
                serverCount = options.MinimumServers;
				
            return Convert.ToInt32(Math.Ceiling(goalWorkerCount / ((decimal)serverCount)));
        }
        
        private class WorkerCalculationOptions
        {
            public int DefaultGoalWorkerCount;
            public int MinimumWorkerCount;
            public int MaximumGoalWorkerCount; 
            public int MinimumServers; 
        }        
        
    }
}