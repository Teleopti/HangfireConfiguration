using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ViewConfigurationsTest
    {
        [Fact]
        public void ShouldBuildConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = "Data Source=Server;Integrated Security=SSPI;Initial Catalog=Test_Database;Application Name=Test",
                SchemaName = "schemaName",
                Active = true
            });
            var configuration = new Configuration(repository);

            var result = configuration.BuildConfigurations();

            Assert.Equal(1, result.Single().Id);
            Assert.Equal("Server", result.Single().ServerName);
            Assert.Equal("Test_Database", result.Single().DatabaseName);
            Assert.Equal("schemaName", result.Single().SchemaName);
            Assert.Equal("Active", result.Single().Active);
        }

        [Fact]
        public void ShouldBuildConfiguration2()
        {
            var repository = new FakeConfigurationRepository();

            repository.Has(new StoredConfiguration
            {
                Id = 2,
                ConnectionString = "Data Source=Server2;Integrated Security=SSPI;Initial Catalog=Test_Database_2;Application Name=Test",
                SchemaName = "schemaName2",
                Active = false
            });
            var configuration = new Configuration(repository);

            var result = configuration.BuildConfigurations();

            Assert.Equal(2, result.Single().Id);
            Assert.Equal("Server2", result.Single().ServerName);
            Assert.Equal("Test_Database_2", result.Single().DatabaseName);
            Assert.Equal("schemaName2", result.Single().SchemaName);
            Assert.Equal("Inactive", result.Single().Active);
        }

        [Fact]
        public void ShouldBuildForMultipleConfigurations()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration
                {
                    Id = 1,
                    ConnectionString = "Data Source=Server1;Integrated Security=SSPI;Initial Catalog=Test_Database_1;Application Name=Test",
                    SchemaName = "schemaName1",
                    Active = true
                },
                new StoredConfiguration()
                {
                    Id = 2,
                    ConnectionString = "Data Source=Server2;Integrated Security=SSPI;Initial Catalog=Test_Database_2;Application Name=Test",
                    SchemaName = "schemaName2",
                    Active = false
                });
            var configuration = new Configuration(repository);

            var result = configuration.BuildConfigurations();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void ShouldBuildWithWorkers()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {GoalWorkerCount = 10});
            var configuration = new Configuration(repository);

            var result = configuration.BuildConfigurations();

            Assert.Equal(10, result.Single().Workers);
        }
    }
}