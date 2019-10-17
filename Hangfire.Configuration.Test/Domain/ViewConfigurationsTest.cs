using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ViewConfigurationsTest
    {
        [Fact]
        public void ShouldBuildConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = "Data Source=Server;Integrated Security=SSPI;Initial Catalog=Test_Database;Application Name=Test",
                SchemaName = "schemaName",
                Active = true
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(1, result.Single().Id);
            Assert.Equal("Server", result.Single().ServerName);
            Assert.Equal("Test_Database", result.Single().DatabaseName);
            Assert.Equal("schemaName", result.Single().SchemaName);
            Assert.Equal("Active", result.Single().Active);
        }

        [Fact]
        public void ShouldBuildConfiguration2()
        {
            var system = new SystemUnderTest();

            system.Repository.Has(new StoredConfiguration
            {
                Id = 2,
                ConnectionString = "Data Source=Server2;Integrated Security=SSPI;Initial Catalog=Test_Database_2;Application Name=Test",
                SchemaName = "schemaName2",
                Active = false
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(2, result.Single().Id);
            Assert.Equal("Server2", result.Single().ServerName);
            Assert.Equal("Test_Database_2", result.Single().DatabaseName);
            Assert.Equal("schemaName2", result.Single().SchemaName);
            Assert.Equal("Inactive", result.Single().Active);
        }

        [Fact]
        public void ShouldBuildWithNullValues()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = null,
                SchemaName = null,
                Active = null,
                GoalWorkerCount = null
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(1, result.Single().Id);
            Assert.Null(result.Single().ServerName);
            Assert.Null(result.Single().DatabaseName);
            Assert.Null(result.Single().SchemaName);
            Assert.Null(result.Single().Active);
            Assert.Null(result.Single().Workers);
        }
        
        [Fact]
        public void ShouldBuildForMultipleConfigurations()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
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

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void ShouldBuildWithWorkers()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {GoalWorkerCount = 10});

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(10, result.Single().Workers);
        }

        [Fact]
        public void ShouldBuildWithDefaultSchemaName()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                ConnectionString = new SqlConnectionStringBuilder {DataSource = "."}.ToString(),
                SchemaName = null
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(DefaultSchemaName.Name(), result.Single().SchemaName);
        }
    }
}