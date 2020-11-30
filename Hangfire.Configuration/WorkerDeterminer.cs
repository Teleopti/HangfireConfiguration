using System;
using System.Linq;
using Hangfire.Storage;

namespace Hangfire.Configuration
{
    public class WorkerDeterminer
    {
        private readonly ServerCountDeterminer _serverCountDeterminer;

        public WorkerDeterminer(IServerCountSampleStorage serverCountSampleStorage)
        {
            _serverCountDeterminer = new ServerCountDeterminer(serverCountSampleStorage);
        }

        internal int DetermineWorkerCount(
            IMonitoringApi monitor, 
            int? goalWorkerCount,
            WorkerDeterminerOptions options)
        {
            var goal = goalWorkerCount ?? options.DefaultGoalWorkerCount;
            if (goal > options.MaximumGoalWorkerCount)
                goal = options.MaximumGoalWorkerCount;

            var serverCount = _serverCountDeterminer.DetermineServerCount(monitor, options);
            
            var workerCount = Convert.ToInt32(Math.Ceiling(goal / ((decimal) serverCount)));
            if (workerCount < options.MinimumWorkerCount)
                workerCount = options.MinimumWorkerCount;

            return workerCount;
        }

    }
}