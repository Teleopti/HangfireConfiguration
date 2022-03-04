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
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = "Data Source=Server;Integrated Security=SSPI;Initial Catalog=Test_Database;Application Name=Test",
                SchemaName = "schemaName",
                Active = true
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

            Assert.Equal(1, result.Id);
            Assert.Equal("Server", result.ServerName);
            Assert.Equal("Test_Database", result.DatabaseName);
            Assert.Equal("schemaName", result.SchemaName);
            Assert.Equal(true, result.Active);
        }

        [Fact]
        public void ShouldBuildConfiguration2()
        {
            var system = new SystemUnderTest();

            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 2,
                ConnectionString = "Data Source=Server2;Integrated Security=SSPI;Initial Catalog=Test_Database_2;Application Name=Test",
                SchemaName = "schemaName2",
                Active = false
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

            Assert.Equal(2, result.Id);
            Assert.Equal("Server2", result.ServerName);
            Assert.Equal("Test_Database_2", result.DatabaseName);
            Assert.Equal("schemaName2", result.SchemaName);
            Assert.Equal(false, result.Active);
        }

        [Fact]
        public void ShouldBuildWithNullValues()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = null,
                SchemaName = null,
                Active = null,
                GoalWorkerCount = null
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

            Assert.Equal(1, result.Id);
            Assert.Null(result.ServerName);
            Assert.Null(result.DatabaseName);
            Assert.Null(result.SchemaName);
            Assert.Null(result.Active);
            Assert.Null(result.Workers);
        }
        
        [Fact]
        public void ShouldBuildForMultipleConfigurations()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
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
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 10});

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(10, result.Single().Workers);
        }

        [Fact]
        public void ShouldBuildWithDefaultSchemaName()
        {
            var system = new SystemUnderTest();
            var storedConfiguration = new StoredConfiguration
            {
	            ConnectionString = new SqlConnectionStringBuilder { DataSource = "." }.ToString(),
	            SchemaName = null
            };

			system.ConfigurationStorage.Has(storedConfiguration);

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(DefaultSchemaName.SqlServer(), result.Single().SchemaName);
        }
        
        [Fact]
        public void ShouldBuildWithConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Name = "name"
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal("name", result.Single().Name);
        }
        
        [Fact]
        public void ShouldBuildWithMaxWorkersPerServer()
        {
            var system = new SystemUnderTest();
            system.WithMaxWorkersPerServer(5);

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.Equal(5, result.Single().MaxWorkersPerServer);
        }
    }
}