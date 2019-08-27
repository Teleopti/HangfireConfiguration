using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Newtonsoft.Json;
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
        void UseStorage(JobStorage jobStorage);
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

        public void UseStorage(JobStorage jobStorage) => GlobalConfiguration.Configuration.UseStorage(jobStorage);
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

        public IEnumerable<RunningServer> StartServers(ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            params IBackgroundProcess[] additionalProcesses)
        {
            new DefaultServerConfigurator(_repository)
                .Configure(options?.DefaultHangfireConnectionString, options?.DefaultSchemaName);
            var storageOptions = options?.StorageOptions ?? new SqlServerStorageOptions();
            
            var backgroundProcesses = new List<IBackgroundProcess>(additionalProcesses);
            serverOptions = serverOptions ?? new BackgroundJobServerOptions();

            return _repository
                .ReadConfigurations()
                .OrderBy(x => !(x.Active ?? false))
                .ThenBy(x => x.Id)
                .Select(configuration => new
                {
                    JobStorage = makeJobStorage(configuration, storageOptions),
                    WorkerCount = configuration.GoalWorkerCount
                })
                .Select(x => startServer(x.WorkerCount, options, x.JobStorage, serverOptions, backgroundProcesses))
                .Select((s, i) => new RunningServer
                {
                    Number = i+1,
                    Storage = s
                })
                .ToArray();
        }

        private JobStorage startServer(
            int? workerCount,
            ConfigurationOptions options,
            JobStorage jobStorage,
            BackgroundJobServerOptions serverOptions,
            List<IBackgroundProcess> backgroundProcesses)
        {
            serverOptions = copyOptions(serverOptions);
            serverOptions.WorkerCount = WorkerDeterminer.DetermineWorkerCount(jobStorage.GetMonitoringApi(), workerCount, options);
            _hangfire.UseHangfireServer(
                _builder,
                jobStorage,
                serverOptions,
                backgroundProcesses.ToArray()
            );
            backgroundProcesses.Clear();

            return jobStorage;
        }

        private JobStorage makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            storageOptions = copyOptions(storageOptions);
            storageOptions.SchemaName = configuration.SchemaName;
            var jobStorage = _hangfire.MakeSqlJobStorage(configuration.ConnectionString, storageOptions);
            if (configuration.Active == true)
                _hangfire.UseStorage(jobStorage);
            return jobStorage;
        }

        private static BackgroundJobServerOptions copyOptions(BackgroundJobServerOptions serverOptions) =>
            new BackgroundJobServerOptions
            {
                Queues = serverOptions.Queues,
                HeartbeatInterval = serverOptions.HeartbeatInterval,
                ServerTimeout = serverOptions.ServerTimeout,
                ShutdownTimeout = serverOptions.ShutdownTimeout,
                StopTimeout = serverOptions.StopTimeout,
                CancellationCheckInterval = serverOptions.CancellationCheckInterval,
                SchedulePollingInterval = serverOptions.SchedulePollingInterval,
                ServerCheckInterval = serverOptions.ServerCheckInterval,
                WorkerCount = serverOptions.WorkerCount
            };

        private static SqlServerStorageOptions copyOptions(SqlServerStorageOptions storageOptions) =>
            JsonConvert.DeserializeObject<SqlServerStorageOptions>(
                JsonConvert.SerializeObject(storageOptions)
            );
    }
}