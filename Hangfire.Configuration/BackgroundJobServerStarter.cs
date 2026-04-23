using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Server;

namespace Hangfire.Configuration;

public class BackgroundJobServerStarter
{
	private readonly IHangfire _hangfire;
	private readonly WorkerBalancer _workerBalancer;
	private readonly StateMaintainer _stateMaintainer;
	private readonly State _state;
	private readonly ServerCountSampleRecorder _recorder;
	private readonly object _appBuilder;
	private readonly QueueCalculator _queueCalculator;

	internal BackgroundJobServerStarter(
		IHangfire hangfire,
		WorkerBalancer workerBalancer,
		StateMaintainer stateMaintainer,
		State state,
		ServerCountSampleRecorder recorder,
		object appBuilder,
		QueueCalculator queueCalculator)
	{
		_hangfire = hangfire;
		_workerBalancer = workerBalancer;
		_stateMaintainer = stateMaintainer;
		_state = state;
		_recorder = recorder;
		_appBuilder = appBuilder;
		_queueCalculator = queueCalculator;
	}

	public IDisposable Start() => Start(null);

	public IDisposable Start(IBackgroundProcess[] additionalProcesses)
	{
		var options = _state.ReadOptions();
		var backgroundProcesses = new List<IBackgroundProcess>();
		if (additionalProcesses != null)
			backgroundProcesses.AddRange(additionalProcesses);
		backgroundProcesses.Add(_recorder);
		var serverOptions = _state.ReadServerOptions();

		_stateMaintainer.Refresh();

		var servers = _state.Configurations
			.Where(x => !x.IsShutdown())
			.OrderBy(x => !x.IsPublisher())
			.Select(x => startBackgroundJobServer(x, options, serverOptions, backgroundProcesses))
			.Where(x => x != null)
			.ToArray();

		var lifetime = new ServerLifetime(servers);
		ServerLifetime.HookApplicationLifetime(_appBuilder, lifetime);
		return lifetime;
	}

	private IBackgroundProcessingServer startBackgroundJobServer(
		ConfigurationState configurationState,
		ConfigurationOptions options,
		BackgroundJobServerOptions serverOptions,
		List<IBackgroundProcess> backgroundProcesses)
	{
		serverOptions = copyOptions(serverOptions);

		var container = findContainer(configurationState.Configuration.Containers, options);
		if (container == null)
			return null;
		applyQueues(container, configurationState.Configuration.Containers, serverOptions);

		applyWorkerBalancer(container, configurationState, options, serverOptions);

		var server = _hangfire.StartBackgroundJobServer(
			configurationState.JobStorage,
			serverOptions,
			backgroundProcesses.ToArray()
		);

		backgroundProcesses.Clear();
		return server;
	}

	private static ContainerConfiguration findContainer(
		ContainerConfiguration[] containers,
		ConfigurationOptions options)
	{
		var tag = string.IsNullOrEmpty(options.ContainerTag)
			? DefaultContainerTag.Tag()
			: options.ContainerTag;

		var container = containers.FirstOrDefault(c => c.Tag == tag);
		if (container != null)
			return container;

		return containers.FirstOrDefault(c => c.Tag == null);
	}

	private void applyQueues(
		ContainerConfiguration container,
		ContainerConfiguration[] containers,
		BackgroundJobServerOptions serverOptions)
	{
		var appliedQueues = _queueCalculator.CalculateAppliedQueues(container, containers);
		if (appliedQueues.Length > 0)
			serverOptions.Queues = appliedQueues;
	}

	private void applyWorkerBalancer(ContainerConfiguration container, ConfigurationState configurationState, ConfigurationOptions options, BackgroundJobServerOptions serverOptions)
	{
		if (!container.WorkerBalancerIsEnabled())
			return;

		serverOptions.WorkerCount =
			_workerBalancer.CalculateWorkerCount(
				configurationState.MonitoringApi,
				container,
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