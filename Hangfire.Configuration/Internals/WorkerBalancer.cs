using System;
using Hangfire.Storage;

namespace Hangfire.Configuration.Internals;

internal class WorkerBalancer
{
	private readonly ServerCountCalculator _serverCountCalculator;

	internal WorkerBalancer(ServerCountCalculator serverCountCalculator)
	{
		_serverCountCalculator = serverCountCalculator;
	}

	internal int CalculateWorkerCount(
		IMonitoringApi monitor,
		ContainerConfiguration container,
		WorkerBalancerOptions options)
	{
		var goal = container.GoalWorkerCount ?? options.DefaultGoalWorkerCount;
		if (goal > options.MaximumGoalWorkerCount)
			goal = options.MaximumGoalWorkerCount;

		var serverCount = _serverCountCalculator.ServerCount(monitor, container.Queues, options);

		var workerCount = Convert.ToInt32(Math.Ceiling(goal / (decimal) serverCount));

		if (container.MaxWorkersPerServer.HasValue)
		{
			if (workerCount > container.MaxWorkersPerServer.Value)
				workerCount = container.MaxWorkersPerServer.Value;
		}

		if (workerCount < options.MinimumWorkerCount)
			workerCount = options.MinimumWorkerCount;

		return workerCount;
	}
}