using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class GoalWorkerCountTest
	{
		[Fact, CleanDatabase]
		public void ShouldReadEmptyGoalWorkerCount()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			
			Assert.Null(repository.ReadGoalWorkerCount());
		}
		
		[Fact, CleanDatabase]
		public void ShouldWriteGoalWorkerCount()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			
			repository.WriteGoalWorkerCount(1);
			
			Assert.Equal(1, repository.ReadGoalWorkerCount());
		}
		
		[Fact, CleanDatabase]
		public void ShouldReadGoalWorkerCount()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(1);
			
			Assert.Equal(1, repository.ReadGoalWorkerCount());
		}
		

		[Fact, CleanDatabase]
		public void ShouldWriteNullGoalWorkerCount()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(1);
			
			repository.WriteGoalWorkerCount(null);
			
			Assert.Null(repository.ReadGoalWorkerCount());
		}
	}
}