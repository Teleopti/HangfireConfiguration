using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
	public class ConfigurationTest
	{
		[Theory]
		[InlineData(1)]
		[InlineData(2)]		
		[InlineData(null)]		
		public void ShouldRead(int? existing)
		{
			var repository = new FakeConfigurationRepository();
			repository.Has(existing);
			var configuration = new Configuration(repository);
			
			Assert.Equal(existing, configuration.ReadGoalWorkerCount());
		}
		
		[Theory]
		[InlineData(1)]
		[InlineData(2)]		
		public void ShouldWrite(int workers)
		{
			var repository = new FakeConfigurationRepository();
			var configuration = new Configuration(repository);
			
			configuration.WriteGoalWorkerCount(workers);
			
			Assert.Equal(workers, repository.Workers);
		}
		
		[Fact]
		public void ShouldWriteNullable()
		{
			var repository = new FakeConfigurationRepository();
			repository.Has(1);
			var configuration = new Configuration(repository);
			
			configuration.WriteGoalWorkerCount(null);
			
			Assert.Null(repository.Workers);
		}
	}

	public class FakeConfigurationRepository: IConfigurationRepository
	{
		public int? Workers { get; private set; }

		public void WriteGoalWorkerCount(int? workers)
		{
			Workers = workers;
		}

		public int? ReadGoalWorkerCount()
		{
			return Workers;
		}

		public void Has(int? workers)
		{
			Workers = workers;
		}
	}
}