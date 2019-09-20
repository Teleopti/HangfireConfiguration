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
        private readonly StateMaintainer _stateMaintainer;
        private readonly State _state;

        public WorkerServerStarter(
            IHangfire hangfire,
            WorkerDeterminer workerDeterminer,
            StateMaintainer stateMaintainer,
            State state)
        {
            _hangfire = hangfire;
            _workerDeterminer = workerDeterminer;
            _stateMaintainer = stateMaintainer;
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

            _stateMaintainer.Refresh(options, storageOptions);
            _state.Configurations
                .OrderBy(x => !(x.Configuration.Active ?? false))
                .ForEach(x =>
                {
                    startWorkerServer(x, options, serverOptions, backgroundProcesses);
                });
        }

        private void startWorkerServer(
            ConfigurationAndStorage configurationAndStorage,
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            List<IBackgroundProcess> backgroundProcesses)
        {
            serverOptions = copyOptions(serverOptions);
            
            if (options.UseWorkerDeterminer)
                serverOptions.WorkerCount = _workerDeterminer.DetermineWorkerCount(
                    configurationAndStorage.CreateJobStorage().GetMonitoringApi(),
                    configurationAndStorage.Configuration.GoalWorkerCount,
                    options
                );
            
            _hangfire.UseHangfireServer(
                configurationAndStorage.CreateJobStorage(),
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