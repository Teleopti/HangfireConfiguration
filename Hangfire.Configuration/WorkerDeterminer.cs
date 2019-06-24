using System;
using Hangfire.Storage;

namespace Hangfire.Configuration
{
	public class WorkerDeterminer
	{
		private readonly IConfigurationRepository _repository;
		private readonly IMonitoringApi _monitor;

		public WorkerDeterminer(IConfigurationRepository repository, IMonitoringApi monitor)
		{
			_repository = repository;
			_monitor = monitor;
		}

		public int DetermineStartingServerWorkerCount()
		{
			var goalWorkerCount = _repository.ReadGoalWorkerCount() ?? 10; 
			
			if (goalWorkerCount <= 0)
				return 1;
			
			if (goalWorkerCount > 100)
				return 100;
			
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