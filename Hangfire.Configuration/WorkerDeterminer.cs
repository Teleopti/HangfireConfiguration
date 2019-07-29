using System;
using Hangfire.Storage;

namespace Hangfire.Configuration
{
	public class WorkerDeterminer
	{
		private readonly Configuration _configuration;
		private readonly IMonitoringApi _monitor;

		public WorkerDeterminer(Configuration configuration, IMonitoringApi monitor)
		{
			_configuration = configuration;
			_monitor = monitor;
		}

		public int DetermineStartingServerWorkerCount()
		{
			return  determineStartingServerWorkerCount(10, 1, 100);
		}

		private int determineStartingServerWorkerCount(int defaultWorkerCount, int minimumWorkerCount, int maximumWorkerCount)
		{
			var goalWorkerCount = _configuration.ReadGoalWorkerCount() ?? defaultWorkerCount; 
			
			if (goalWorkerCount <= minimumWorkerCount)
				return minimumWorkerCount;
			
			if (goalWorkerCount > maximumWorkerCount)
				goalWorkerCount = maximumWorkerCount;
			
			var serverCount = _monitor.Servers().Count;
			if (serverCount > 0)
				return calculateAsIfRestartAndNotRemoved(goalWorkerCount, serverCount);

			return goalWorkerCount;
		}

		private static int calculateAsIfRestartAndNotRemoved(int goalWorkerCount, int serverCount)
		{
			return Convert.ToInt32(Math.Ceiling(goalWorkerCount / ((decimal)serverCount)));
		}
	}
}