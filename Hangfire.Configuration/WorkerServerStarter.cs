using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class WorkerServerStarter
    {
        private readonly IHangfire _hangfire;
        private readonly WorkerDeterminer _workerDeterminer;
        private readonly StorageCreator _storageCreator;
        private readonly StorageState _state;

        public WorkerServerStarter(
            IHangfire hangfire,
            WorkerDeterminer workerDeterminer,
            StorageCreator storageCreator,
            StorageState state)
        {
            _hangfire = hangfire;
            _workerDeterminer = workerDeterminer;
            _storageCreator = storageCreator;
            _state = state;
        }

        public void Start(
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            SqlServerStorageOptions storageOptions,
            params IBackgroundProcess[] additionalProcesses)
        {
            options = options ?? new ConfigurationOptions();
            var backgroundProcesses = new List<IBackgroundProcess>(additionalProcesses);
            serverOptions = serverOptions ?? new BackgroundJobServerOptions();

            _storageCreator.Create(options, storageOptions);
            foreach (var storage in _state.State)
                startWorkerServer(storage, options, serverOptions, backgroundProcesses);
        }

        private void startWorkerServer(
            Storage storage,
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            List<IBackgroundProcess> backgroundProcesses)
        {
            serverOptions = copyOptions(serverOptions);
            
            if (options.UseWorkerDeterminer)
                serverOptions.WorkerCount = _workerDeterminer.DetermineWorkerCount(
                    storage.JobStorage.GetMonitoringApi(),
                    storage.Configuration.GoalWorkerCount,
                    options
                );
            
            _hangfire.UseHangfireServer(
                storage.JobStorage,
                serverOptions,
                backgroundProcesses.ToArray()
            );
            
            backgroundProcesses.Clear();
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
    }
}