using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Server;

namespace Hangfire.Configuration
{
    public class WorkerServerStarter
    {
        private readonly IHangfire _hangfire;
        private readonly WorkerDeterminer _workerDeterminer;
        private readonly StateMaintainer _stateMaintainer;
        private readonly State _state;
        private readonly ServerCountSampleRecorder _recorder;

        internal WorkerServerStarter(
            IHangfire hangfire,
            WorkerDeterminer workerDeterminer,
            StateMaintainer stateMaintainer,
            State state,
            ServerCountSampleRecorder recorder)
        {
            _hangfire = hangfire;
            _workerDeterminer = workerDeterminer;
            _stateMaintainer = stateMaintainer;
            _state = state;
            _recorder = recorder;
        }

        public void Start(IBackgroundProcess[] additionalProcesses)
        {
            var options = _state.ReadOptions();
            var backgroundProcesses = new List<IBackgroundProcess>();
            if (additionalProcesses != null)
                backgroundProcesses.AddRange(additionalProcesses);
            if (_state.ReadOptions().WorkerDeterminerOptions.UseServerCountSampling)
				backgroundProcesses.Add(_recorder);
            var serverOptions = _state.ServerOptions ?? new BackgroundJobServerOptions();

            _stateMaintainer.Refresh();
            _state.Configurations
                .OrderBy(x => !(x.Configuration.Active ?? false))
                .ForEach(x => { startWorkerServer(x, options, serverOptions, backgroundProcesses); });
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
                    configurationAndStorage.Configuration,
                    options.WorkerDeterminerOptions
                );

            _hangfire.UseHangfireServer(
                configurationAndStorage.CreateJobStorage(),
                serverOptions,
                backgroundProcesses.ToArray()
            );

            backgroundProcesses.Clear();
        }

        private static BackgroundJobServerOptions copyOptions(BackgroundJobServerOptions serverOptions) =>
            new()
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