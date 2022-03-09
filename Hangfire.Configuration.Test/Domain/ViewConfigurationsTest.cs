using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class ViewConfigurationsTest
    {
        [Test]
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

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Server", result.ServerName);
            Assert.AreEqual("Test_Database", result.DatabaseName);
            Assert.AreEqual("schemaName", result.SchemaName);
            Assert.AreEqual(true, result.Active);
        }

        [Test]
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

            Assert.AreEqual(2, result.Id);
            Assert.AreEqual("Server2", result.ServerName);
            Assert.AreEqual("Test_Database_2", result.DatabaseName);
            Assert.AreEqual("schemaName2", result.SchemaName);
            Assert.AreEqual(false, result.Active);
        }

        [Test]
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

            Assert.AreEqual(1, result.Id);
            Assert.Null(result.ServerName);
            Assert.Null(result.DatabaseName);
            Assert.Null(result.SchemaName);
            Assert.Null(result.Active);
            Assert.Null(result.Workers);
        }
        
        [Test]
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

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void ShouldBuildWithWorkers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 10});

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.AreEqual(10, result.Single().Workers);
        }

        [Test]
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

            Assert.AreEqual(DefaultSchemaName.SqlServer(), result.Single().SchemaName);
        }
        
        [Test]
        public void ShouldBuildWithConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Name = "name"
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.AreEqual("name", result.Single().Name);
        }
        
        [Test]
        public void ShouldBuildWithMaxWorkersPerServer()
        {
            var system = new SystemUnderTest();
            system.WithMaxWorkersPerServer(5);

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.AreEqual(5, result.Single().MaxWorkersPerServer);
        }
    }
}