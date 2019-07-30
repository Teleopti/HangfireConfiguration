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
		
		public int DetermineStartingServerWorkerCount() =>
			DetermineWorkerCount
			(
				_monitor,
				_configuration.ReadGoalWorkerCount()
			);
		
		internal static int DetermineWorkerCount(IMonitoringApi monitor, int? goalWorkerCount, ConfigurationOptions options = null)
		{
			var goal = goalWorkerCount ?? options?.DefaultGoalWorkerCount ?? 10;

			var minimumWorkerCount = options?.MinimumWorkerCount ?? 1;
			if (goal <= minimumWorkerCount)
				return minimumWorkerCount;

			var maximumGoal = options?.MaximumGoalWorkerCount ?? 100;  
			if (goal > maximumGoal)
				goal = maximumGoal;

			var minimumServers = options?.MinimumServers ?? 2;
			var serverCount = monitor.Servers().Count; 
			if (serverCount < minimumServers)
				serverCount = minimumServers;
				
			return Convert.ToInt32(Math.Ceiling(goal / ((decimal)serverCount)));
		}
	}
}