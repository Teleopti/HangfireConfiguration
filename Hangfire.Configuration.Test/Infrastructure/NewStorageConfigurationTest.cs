using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("Infrastructure")]
    public class NewStorageConfigurationTest
    {
        [Fact, CleanDatabase]
        public void ShouldReadEmptyConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());

            Assert.Equal(0, repository.ReadConfigurations().Count());
        }
        
        [Fact, CleanDatabase]
        public void ShouldWrite()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			
            repository.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = "connection string",
                SchemaName = "schema name",
                Active = false
            });

            var configuration = repository.ReadConfigurations().Single();
            Assert.Equal("connection string", configuration.ConnectionString);
            Assert.Equal("schema name", configuration.SchemaName);
            Assert.Equal(false, configuration.Active);
        }
        
        [Fact, CleanDatabase]
        public void ShouldRead()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = "connectionString",
                SchemaName = "schemaName",
                GoalWorkerCount = 3,
                Active = true
            });

            var result = repository.ReadConfigurations().Single();

            Assert.Equal(1, result.Id);
            Assert.Equal("connectionString", result.ConnectionString);
            Assert.Equal("schemaName", result.SchemaName);
            Assert.Equal(3, result.GoalWorkerCount);
            Assert.Equal(true, result.Active);
        }
    }
}