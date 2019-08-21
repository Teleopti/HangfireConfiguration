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

            new DefaultServerConfigurator(_repository).
                Configure(options?.DefaultHangfireConnectionString, options?.DefaultSchemaName);

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

                var workerCount = WorkerDeterminer.DetermineWorkerCount
                (
                    sqlJobStorage.GetMonitoringApi(),
                    storedConfig.GoalWorkerCount,
                    options ?? new ConfigurationOptions()
                );

                var appliedServerOptions = new BackgroundJobServerOptions
                {
                    WorkerCount = workerCount,
                };

                if (serverOptions != null)
                {
                    appliedServerOptions.Queues = serverOptions.Queues;
                    appliedServerOptions.Activator = serverOptions.Activator;
                    appliedServerOptions.FilterProvider = serverOptions.FilterProvider;
                    appliedServerOptions.HeartbeatInterval = serverOptions.HeartbeatInterval;
                    appliedServerOptions.ServerTimeout = serverOptions.ServerTimeout;
                    appliedServerOptions.ShutdownTimeout = serverOptions.ShutdownTimeout;
                    appliedServerOptions.StopTimeout = serverOptions.StopTimeout;
                    appliedServerOptions.TaskScheduler = serverOptions.TaskScheduler;
                    appliedServerOptions.CancellationCheckInterval = serverOptions.CancellationCheckInterval;
                    appliedServerOptions.SchedulePollingInterval = serverOptions.SchedulePollingInterval;
                    appliedServerOptions.ServerCheckInterval = serverOptions.ServerCheckInterval;
                    appliedServerOptions.TimeZoneResolver = serverOptions.TimeZoneResolver;
                }

                var appliedAdditionalProcess = new IBackgroundProcess[] {};
                if ( storedConfig.Id == activeConfiguration.Id ) 
                    appliedAdditionalProcess = additionalProcesses;
                    
                _hangfire.UseHangfireServer(_builder, sqlJobStorage, appliedServerOptions, appliedAdditionalProcess);

                runningServers.Add(new RunningServer() {Number = serverNumber, Storage = sqlJobStorage});
                serverNumber++;
            });

            return runningServers;
        }
    }
}