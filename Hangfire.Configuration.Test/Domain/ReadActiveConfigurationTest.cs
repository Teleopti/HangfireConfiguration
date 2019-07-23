using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
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

        [Fact]
        public void ShouldReadConnectionStringFromDefaultActivatedConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            configuration.CreateStorageConfiguration(new NewStorageConfiguration
            {
                Database = "newDatabase",
                SchemaName = "newSchemaName",
            });
            configuration.ConfigureDefaultStorage("connectionString", "schemaName");

            var result = configuration.ReadActiveConfigurationConnectionString();

            Assert.Equal("connectionString", result);
        }
    }
}