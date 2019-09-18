using System;
using System.Linq;
using Hangfire.Common;
using Hangfire.Storage;

namespace Hangfire.Configuration
{
	public class WorkerDeterminer
	{
		private readonly IConfigurationRepository _repository;

		public WorkerDeterminer(IConfigurationRepository repository)
		{
			_repository = repository;
		}
		
		[Obsolete("Dont use, will be removed")]
		public int DetermineStartingServerWorkerCount()
		{
			return DetermineWorkerCount
			(
				JobStorage.Current.GetMonitoringApi(),
				_repository.ReadConfigurations().FirstOrDefault()?.GoalWorkerCount
			);
		}

		internal int DetermineWorkerCount(IMonitoringApi monitor, int? goalWorkerCount, ConfigurationOptions options = null)
		{
			options = options ?? new ConfigurationOptions();
			
			var goal = goalWorkerCount ?? options.DefaultGoalWorkerCount;
			if (goal > options.MaximumGoalWorkerCount)
				goal = options.MaximumGoalWorkerCount;

			var serverCount = monitor.Servers().Count; 
			if (serverCount < options.MinimumServers)
				serverCount = options.MinimumServers;
			if (serverCount == 0)
				serverCount = 1;
				
			var workerCount =  Convert.ToInt32(Math.Ceiling(goal / ((decimal)serverCount)));
			if (workerCount < options.MinimumWorkerCount)
                workerCount = options.MinimumWorkerCount;

            return workerCount;
        }
	}
}