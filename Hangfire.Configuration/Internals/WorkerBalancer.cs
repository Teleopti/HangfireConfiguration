using System;
using Hangfire.Storage;

namespace Hangfire.Configuration.Internals;

internal class WorkerBalancer
{
	private readonly ServerCountCalculator _serverCountCalculator;

	internal WorkerBalancer(IKeyValueStore store)
	{
		_serverCountCalculator = new ServerCountCalculator(store);
	}

	internal int DetermineWorkerCount(
		IMonitoringApi monitor,
		StoredConfiguration configuration,
		WorkerBalancerOptions options)
	{
		var goal = configuration.GoalWorkerCount ?? options.DefaultGoalWorkerCount;
		if (goal > options.MaximumGoalWorkerCount)
			goal = options.MaximumGoalWorkerCount;

		var serverCount = _serverCountCalculator.ServerCount(monitor, options);

		var workerCount = Convert.ToInt32(Math.Ceiling(goal / (decimal) serverCount));

		if (configuration.MaxWorkersPerServer.HasValue)
		{
			if (workerCount > configuration.MaxWorkersPerServer.Value)
				workerCount = configuration.MaxWorkersPerServer.Value;
		}

		if (workerCount < options.MinimumWorkerCount)
			workerCount = options.MinimumWorkerCount;

		return workerCount;
	}
}