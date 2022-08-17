using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Server;

namespace Hangfire.Configuration
{
    public class WorkerServerStarter
    {
        private readonly IHangfire _hangfire;
        private readonly WorkerBalancer _workerBalancer;
        private readonly StateMaintainer _stateMaintainer;
        private readonly State _state;
        private readonly ServerCountSampleRecorder _recorder;

        internal WorkerServerStarter(
            IHangfire hangfire,
            WorkerBalancer workerBalancer,
            StateMaintainer stateMaintainer,
            State state,
            ServerCountSampleRecorder recorder)
        {
            _hangfire = hangfire;
            _workerBalancer = workerBalancer;
            _stateMaintainer = stateMaintainer;
            _state = state;
            _recorder = recorder;
        }

        public void Start() => Start(null);
        
        public void Start(IBackgroundProcess[] additionalProcesses)
        {
            var options = _state.ReadOptions();
            var backgroundProcesses = new List<IBackgroundProcess>();
            if (additionalProcesses != null)
                backgroundProcesses.AddRange(additionalProcesses);
            if (_state.ReadOptions().WorkerBalancerOptions.UseServerCountSampling)
				backgroundProcesses.Add(_recorder);
            var serverOptions = _state.ServerOptions ?? new BackgroundJobServerOptions();

            _stateMaintainer.Refresh();
            _state.Configurations
                .OrderBy(x => !(x.Configuration.Active ?? false))
                .ForEach(x => { startWorkerServer(x, options, serverOptions, backgroundProcesses); });
        }

        private void startWorkerServer(
            ConfigurationState configurationState,
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            List<IBackgroundProcess> backgroundProcesses)
        {
            serverOptions = copyOptions(serverOptions);

            //var enabled = configurationState.Configuration.GoalWorkerCountEnabled ?? true;
            if (options.UseWorkerBalancer) // && enabled)
                serverOptions.WorkerCount = _workerBalancer.DetermineWorkerCount(
                    configurationState.MonitoringApi,
                    configurationState.Configuration,
                    options.WorkerBalancerOptions
                );

            _hangfire.UseHangfireServer(
                configurationState.JobStorage,
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