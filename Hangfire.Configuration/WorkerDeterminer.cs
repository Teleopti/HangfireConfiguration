using System;
using Hangfire.Storage;

namespace Hangfire.Configuration
{
    public class WorkerDeterminer
    {
        private readonly ServerCountDeterminer _serverCountDeterminer;

        public WorkerDeterminer(IKeyValueStore store)
        {
            _serverCountDeterminer = new ServerCountDeterminer(store);
        }

        internal int DetermineWorkerCount(IMonitoringApi monitor,
	        StoredConfiguration configuration,
	        WorkerDeterminerOptions options)
        {
            var goal = configuration.GoalWorkerCount ?? options.DefaultGoalWorkerCount;
            if (goal > options.MaximumGoalWorkerCount)
                goal = options.MaximumGoalWorkerCount;

            var serverCount = _serverCountDeterminer.DetermineServerCount(monitor, options);
            
            var workerCount = Convert.ToInt32(Math.Ceiling(goal / ((decimal) serverCount)));
            
            var maxWorkers = configuration.MaxWorkersPerServer;
            if (maxWorkers.HasValue && workerCount > maxWorkers)
	            workerCount = maxWorkers.Value;
            
            if (workerCount < options.MinimumWorkerCount)
                workerCount = options.MinimumWorkerCount;

            return workerCount;
        }

    }
}