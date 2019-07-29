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
			determineStartingServerWorkerCount(new WorkerCalculationOptions
			{ 
				DefaultGoalWorkerCount = 10, 
				MinimumWorkerCount = 1,
				MaximumGoalWorkerCount = 100, 
				MinimumServers = 2
			});

		private int determineStartingServerWorkerCount(WorkerCalculationOptions options)
		{
			var goalWorkerCount = _configuration.ReadGoalWorkerCount() ?? options.DefaultGoalWorkerCount; 
			
			if (goalWorkerCount <= options.MinimumWorkerCount)
				return options.MinimumWorkerCount;
			
			if (goalWorkerCount > options.MaximumGoalWorkerCount)
				goalWorkerCount = options.MaximumGoalWorkerCount;
			
			var serverCount = _monitor.Servers().Count; 
			if (serverCount < options.MinimumServers)
				serverCount = options.MinimumServers;
				
			return Convert.ToInt32(Math.Ceiling(goalWorkerCount / ((decimal)serverCount)));
		}
		
		private class WorkerCalculationOptions
		{
			public int DefaultGoalWorkerCount;
			public int MinimumWorkerCount;
			public int MaximumGoalWorkerCount; 
			public int MinimumServers; 
		}

	}
}