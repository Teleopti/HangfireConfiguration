using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Server;

namespace Hangfire.Configuration;

public class WorkerServerStarter
{
	private readonly IHangfire _hangfire;
	private readonly WorkerBalancer _workerBalancer;
	private readonly StateMaintainer _stateMaintainer;
	private readonly State _state;
	private readonly ServerCountSampleRecorder _recorder;
	private readonly object _appBuilder;

	internal WorkerServerStarter(
		IHangfire hangfire,
		WorkerBalancer workerBalancer,
		StateMaintainer stateMaintainer,
		State state,
		ServerCountSampleRecorder recorder,
		object appBuilder)
	{
		_hangfire = hangfire;
		_workerBalancer = workerBalancer;
		_stateMaintainer = stateMaintainer;
		_state = state;
		_recorder = recorder;
		_appBuilder = appBuilder;
	}

	public IDisposable Start() => Start(null);

	public IDisposable Start(IBackgroundProcess[] additionalProcesses)
	{
		var options = _state.ReadOptions();
		var backgroundProcesses = new List<IBackgroundProcess>();
		if (additionalProcesses != null)
			backgroundProcesses.AddRange(additionalProcesses);
		if (_state.ReadOptions().WorkerBalancerOptions.UseServerCountSampling)
			backgroundProcesses.Add(_recorder);
		var serverOptions = _state.ServerOptions ?? new BackgroundJobServerOptions();

		_stateMaintainer.Refresh();

		var servers = _state.Configurations
			.Where(x => !x.IsShutdown())
			.OrderBy(x => !x.IsPublisher())
			.Select(x => startWorkerServer(x, options, serverOptions, backgroundProcesses))
			.Where(x => x != null)
			.ToArray();

		var lifetime = new ServerLifetime(servers);
		ServerLifetime.HookApplicationLifetime(_appBuilder, lifetime);
		return lifetime;
	}

	private IBackgroundProcessingServer startWorkerServer(
		ConfigurationState configurationState,
		ConfigurationOptions options,
		BackgroundJobServerOptions serverOptions,
		List<IBackgroundProcess> backgroundProcesses)
	{
		serverOptions = copyOptions(serverOptions);

		applyWorkerBalancer(configurationState, options, serverOptions);

		var server = _hangfire.StartBackgroundJobServer(
			configurationState.JobStorage,
			serverOptions,
			backgroundProcesses.ToArray()
		);

		backgroundProcesses.Clear();
		return server;
	}

	private void applyWorkerBalancer(ConfigurationState configurationState, ConfigurationOptions options, BackgroundJobServerOptions serverOptions)
	{
		var enabled = configurationState.WorkerBalancerIsEnabled();
		if (!enabled)
			return;

		serverOptions.WorkerCount =
			_workerBalancer.CalculateWorkerCount(
				configurationState.MonitoringApi,
				configurationState.Configuration,
				options.WorkerBalancerOptions
			);
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
			WorkerCount = serverOptions.WorkerCount,
			Activator = serverOptions.Activator
		};
}