using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    // delete?
    public class ReadActiveConfigurationTest
    {
        [Fact]
        public void ShouldReadEmptyActiveConnectionString()
        {
            var configuration = new Configuration(new FakeConfigurationRepository());

            var connectionString = configuration.ReadActiveConfigurationConnectionString();

            Assert.Null(connectionString);
        }

        [Fact]
        public void ShouldReadActiveConnectionString()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration
            {
                ConnectionString = "connectionString",
                Active = true
            });
            var configuration = new Configuration(repository);

            var connectionString = configuration.ReadActiveConfigurationConnectionString();

            Assert.Equal("connectionString", connectionString);
        }

    }
}