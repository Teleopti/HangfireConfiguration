using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class ConfigurationTest
	{
		[Fact, CleanDatabase]
		public void ShouldReadEmpty()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			
			Assert.Null(repository.ReadGoalWorkerCount());
		}
		
		[Fact, CleanDatabase]
		public void ShouldWrite()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			
			repository.WriteGoalWorkerCount(1);
			
			Assert.Equal(1, repository.ReadGoalWorkerCount());
		}
		
		[Fact, CleanDatabase]
		public void ShouldRead()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(1);
			
			Assert.Equal(1, repository.ReadGoalWorkerCount());
		}
		

		[Fact, CleanDatabase]
		public void ShouldWriteNull()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(1);
			
			repository.WriteGoalWorkerCount(null);
			
			Assert.Null(repository.ReadGoalWorkerCount());
		}
	}
}