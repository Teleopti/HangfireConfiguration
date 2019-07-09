using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
	public class ConfigurationTest
	{
		[Theory]
		[InlineData(1)]
		[InlineData(2)]		
		[InlineData(null)]		
		public void ShouldReadGoalWorkerCount(int? existing)
		{
			var repository = new FakeConfigurationRepository();
			repository.Has(new StoredConfiguration()
			{
				Workers = existing
			});
			var configuration = new Configuration(repository);
			
			Assert.Equal(existing, configuration.ReadGoalWorkerCount());
		}
		
		[Theory]
		[InlineData(1)]
		[InlineData(2)]		
		public void ShouldWriteGoalWorkerCount(int workers)
		{
			var repository = new FakeConfigurationRepository();
			var configuration = new Configuration(repository);
			
			configuration.WriteGoalWorkerCount(workers);
			
			Assert.Equal(workers, repository.Workers);
		}
		
		[Fact]
		public void ShouldWriteNullableGoalWorkerCount()
		{
			var repository = new FakeConfigurationRepository();
			repository.Has(new StoredConfiguration
			{
				Workers = 1
			});
			var configuration = new Configuration(repository);
			
			configuration.WriteGoalWorkerCount(null);
			
			Assert.Null(repository.Workers);
		}
		
		[Fact]
		public void ShouldGetConfiguration()
		{
			var repository = new FakeConfigurationRepository();
			repository.Has(new StoredConfiguration()
			{
				Id = 1,
				ConnectionString = "Data Source=Server;Integrated Security=SSPI;Initial Catalog=Test_Database;Application Name=Test",
				SchemaName = "schemaName",
				Active = true
			});
			var configuration = new Configuration(repository);
			var storedConfiguration = repository.ReadConfiguration();

			var result = configuration.GetConfiguration();
			
			Assert.Equal(storedConfiguration.Id, result.Id);
			Assert.Equal("Server", result.ServerName);
			Assert.Equal("Test_Database", result.DatabaseName);
			Assert.Equal(storedConfiguration.SchemaName, result.SchemaName);
			Assert.Equal("Active", result.Active);
		}
		
		[Fact]
		public void ShouldGetConfiguration2()
		{
			var repository = new FakeConfigurationRepository();
			repository.Has(new StoredConfiguration()
			{
				Id = 2,
				ConnectionString = "Data Source=Server2;Integrated Security=SSPI;Initial Catalog=Test_Database_2;Application Name=Test",
				SchemaName = "schemaName2",
				Active = false
			});
			var configuration = new Configuration(repository);
			var storedConfiguration = repository.ReadConfiguration();

			var result = configuration.GetConfiguration();
			
			Assert.Equal(storedConfiguration.Id, result.Id);
			Assert.Equal("Server2", result.ServerName);
			Assert.Equal("Test_Database_2", result.DatabaseName);
			Assert.Equal(storedConfiguration.SchemaName, result.SchemaName);
			Assert.Equal("Inactive", result.Active);
		}
	}
}