using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Owin;

namespace Hangfire.Configuration
{
    public class ServerStarter
    {
        private readonly IAppBuilder _builder;
        private readonly IHangfire _hangfire;

        public ServerStarter(IAppBuilder builder, IHangfire hangfire)
        {
            _builder = builder;
            _hangfire = hangfire;
        }

        public void StartServers(
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            IEnumerable<StorageWithConfiguration> jobStorages,
            params IBackgroundProcess[] additionalProcesses)
        {
            var backgroundProcesses = new List<IBackgroundProcess>(additionalProcesses);
            serverOptions = serverOptions ?? new BackgroundJobServerOptions();

            jobStorages
                .Select(storage => startServer(storage, options, serverOptions, backgroundProcesses))
                .ToArray();
        }

        private JobStorage startServer(
            StorageWithConfiguration storage,
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            List<IBackgroundProcess> backgroundProcesses)
        {
            serverOptions = copyOptions(serverOptions);
            serverOptions.WorkerCount = WorkerDeterminer.DetermineWorkerCount(
                storage.JobStorage.GetMonitoringApi(),
                storage.Configuration.GoalWorkerCount,
                options
            );
            _hangfire.UseHangfireServer(
                _builder,
                storage.JobStorage,
                serverOptions,
                backgroundProcesses.ToArray()
            );
            backgroundProcesses.Clear();

            return storage.JobStorage;
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