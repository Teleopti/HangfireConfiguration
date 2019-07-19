using System.Collections.Generic;
using System.Linq;
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

			var result = configuration.GetAllConfigurations();
			
			Assert.Equal(1, result.Single().Id);
			Assert.Equal("Server", result.Single().ServerName);
			Assert.Equal("Test_Database", result.Single().DatabaseName);
			Assert.Equal("schemaName", result.Single().SchemaName);
			Assert.Equal("Active", result.Single().Active);
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

			var result = configuration.GetAllConfigurations();
			
			Assert.Equal(2, result.Single().Id);
			Assert.Equal("Server2", result.Single().ServerName);
			Assert.Equal("Test_Database_2", result.Single().DatabaseName);
			Assert.Equal("schemaName2", result.Single().SchemaName);
			Assert.Equal("Inactive", result.Single().Active);
		}
		
		[Fact]
		public void ShouldGetMultipleConfigurations()
		{
			var repository = new FakeConfigurationRepository();
			var storedConfigurations = new List<StoredConfiguration>();
			storedConfigurations.Add(new StoredConfiguration(){
				Id = 1,
				ConnectionString = "Data Source=Server1;Integrated Security=SSPI;Initial Catalog=Test_Database_1;Application Name=Test",
				SchemaName = "schemaName1",
				Active = true
			});
			storedConfigurations.Add(new StoredConfiguration(){
				Id = 2,
				ConnectionString = "Data Source=Server2;Integrated Security=SSPI;Initial Catalog=Test_Database_2;Application Name=Test",
				SchemaName = "schemaName2",
				Active = false
			});
		
			repository.Has(storedConfigurations);
			var configuration = new Configuration(repository);
			
			var result = configuration.GetAllConfigurations();
			
			Assert.Equal(2, result.Count());
		}
	}
}