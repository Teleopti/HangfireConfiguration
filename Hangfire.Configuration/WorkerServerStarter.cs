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

        public WorkerServerStarter(
            IHangfire hangfire,
            WorkerDeterminer workerDeterminer,
            StorageCreator storageCreator)
        {
            _hangfire = hangfire;
            _workerDeterminer = workerDeterminer;
            _storageCreator = storageCreator;
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

            foreach (var storage in _storageCreator.Create(options, storageOptions))
                startWorkerServer(storage, options, serverOptions, backgroundProcesses);
        }

        private void startWorkerServer(
            HangfireStorage hangfireStorage,
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            List<IBackgroundProcess> backgroundProcesses)
        {
            serverOptions = copyOptions(serverOptions);
            
            if (options.UseWorkerDeterminer)
                serverOptions.WorkerCount = _workerDeterminer.DetermineWorkerCount(
                    hangfireStorage.JobStorage.GetMonitoringApi(),
                    hangfireStorage.Configuration.GoalWorkerCount,
                    options
                );
            
            _hangfire.UseHangfireServer(
                hangfireStorage.JobStorage,
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