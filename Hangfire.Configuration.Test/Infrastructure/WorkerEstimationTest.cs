using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class WorkerEstimationTest
	{
		[Fact, CleanDatabase]
		public void ShouldGetDefaultGoalWorkerCount()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			var target = new WorkerDeterminer(repository, JobStorage.Current.GetMonitoringApi());

			var workers = target.DetermineStartingServerWorkerCount();

			Assert.Equal(10, workers);
		}

		[Fact, CleanDatabase]
		public void ShouldGetGoalWorkerCountForFirstServer()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(8);
			var target = new WorkerDeterminer(repository, JobStorage.Current.GetMonitoringApi());

			var workers = target.DetermineStartingServerWorkerCount();

			Assert.Equal(8, workers);
		}

		[Fact, CleanDatabase]
		public void ShouldGetGoalWorkerCountOnRestartOfSingleServer()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(8);
			using (var connection = JobStorage.Current.GetConnection())
			{
				connection.AnnounceServer("restartedServer", new ServerContext());
			}
			var target = new WorkerDeterminer(repository, JobStorage.Current.GetMonitoringApi());

			var workers = target.DetermineStartingServerWorkerCount();

			Assert.Equal(8, workers);
		}
		
		[Fact, CleanDatabase]
		public void ShouldDetermineHalfOfGoalForSecondServerAfterRestart()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(8);
			using (var connection = JobStorage.Current.GetConnection())
			{
				connection.AnnounceServer("server1", new ServerContext());
				connection.AnnounceServer("restartedServer", new ServerContext());
			}
			var target = new WorkerDeterminer(repository, JobStorage.Current.GetMonitoringApi());

			var workers = target.DetermineStartingServerWorkerCount();

			Assert.Equal(4, workers);
		}		
		
		[Fact, CleanDatabase]
		public void ShouldRoundDeterminedWorkerCountUp()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(10);
			using (var connection = JobStorage.Current.GetConnection())
			{
				connection.AnnounceServer("server1", new ServerContext());
				connection.AnnounceServer("server2", new ServerContext());
				connection.AnnounceServer("server3", new ServerContext());
			}
			var target = new WorkerDeterminer(repository, JobStorage.Current.GetMonitoringApi());

			var workers = target.DetermineStartingServerWorkerCount();

			Assert.Equal(4, workers);
		}
		
		[Fact, CleanDatabase]
		public void ShouldDetermineToOneIfWorkerGoalCountIsZero()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(0);
			var target = new WorkerDeterminer(repository, JobStorage.Current.GetMonitoringApi());

			var workers = target.DetermineStartingServerWorkerCount();

			Assert.Equal(1, workers);
		}		
		
		[Fact, CleanDatabase]
		public void ShouldDetermineToOneIfWorkerGoalCountIsNegative()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(-1);
			var target = new WorkerDeterminer(repository, JobStorage.Current.GetMonitoringApi());

			var workers = target.DetermineStartingServerWorkerCount();

			Assert.Equal(1, workers);
		}		
		
		[Fact, CleanDatabase]
		public void ShouldDetermineToMaxOneHundred()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(101);
			var target = new WorkerDeterminer(repository, JobStorage.Current.GetMonitoringApi());

			var workers = target.DetermineStartingServerWorkerCount();

			Assert.Equal(100, workers);
		}	
		
	}
}