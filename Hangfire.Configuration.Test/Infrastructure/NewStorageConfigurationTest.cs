using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("Infrastructure")]
    public class NewStorageConfigurationTest
    {
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

            var configuration = repository.ReadConfigurations();
            Assert.Equal("connection string", configuration.Single().ConnectionString);
            Assert.Equal("schema name", configuration.Single().SchemaName);
            Assert.Equal(false, configuration.Single().Active);
        }
    }
}