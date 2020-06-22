using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Common;
using Hangfire.Storage;
using Polly;

namespace Hangfire.Configuration
{
	public class WorkerDeterminer
	{
		private readonly IConfigurationRepository _repository;
		
		private static readonly Policy _retry = Policy.Handle<SqlException>(DetectTransientSqlException.IsTransient)
			.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
			.WaitAndRetry(6, i => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(i, 2))));

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

			var serverCount = _retry.Execute(() => monitor.Servers().Count); 
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