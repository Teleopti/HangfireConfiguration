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
			options = options ?? new ConfigurationOptions();
			
			var goal = goalWorkerCount ?? options.DefaultGoalWorkerCount;

			if (goal <= options.MinimumWorkerCount)
				return options.MinimumWorkerCount;

			if (goal > options.MaximumGoalWorkerCount)
				goal = options.MaximumGoalWorkerCount;

			var serverCount = monitor.Servers().Count; 
			if (serverCount < options.MinimumServers)
				serverCount = options.MinimumServers;
				
			return Convert.ToInt32(Math.Ceiling(goal / ((decimal)serverCount)));
		}
	}
}