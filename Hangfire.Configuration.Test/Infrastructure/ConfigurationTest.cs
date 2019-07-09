using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class ConfigurationTest
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
		
		[Fact, CleanDatabase]
		public void ShouldReadEmptyConfiguration()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			
			Assert.Null(repository.ReadConfiguration());
		}
		
		[Fact, CleanDatabase]
		public void ShouldReadConfiguration()
		{
			var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			repository.WriteGoalWorkerCount(1);
			repository.SaveConfigurationInfo("connectionString", "schemaName");

			var result = repository.ReadConfiguration();
			
			Assert.Equal(1, result.Id);
			Assert.Equal("connectionString", result.ConnectionString);
			Assert.Equal("schemaName", result.SchemaName);
			Assert.Equal(1, result.Workers);
			Assert.Equal(true, result.Active);
		}
	}
}