using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class NewStorageConfigurationTest
    {
        [Fact]
        public void ShouldSaveNewStorageConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);
            var connectionString = "Data Source=AwesomeServer;Initial Catalog=TestDatabase;User ID=testUser;Password=awesomePassword";
            var schemaName = "awesomeSchema";

            configuration.SaveNewStorageConfiguration(new NewStorageConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaName = schemaName
            });

            var storedConfiguration = repository.ReadConfiguration();
            Assert.Equal(connectionString, storedConfiguration.ConnectionString);
            Assert.Equal(schemaName, storedConfiguration.SchemaName);
        }
        
        [Fact]
        public void ShouldBeInactiveOnSave()
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);

            configuration.SaveNewStorageConfiguration(new NewStorageConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaName = "awesomeSchema"
            });

            var storedConfiguration = repository.ReadConfiguration();
            Assert.Equal(false, storedConfiguration.Active);
        }
    }
}