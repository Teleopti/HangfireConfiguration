using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureUpdateConfigurationsTest
    {
        [Fact]
        public void ShouldConfigureUpdatedConfiguration()
        {
            var system = new SystemUnderTest();
            var connectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                UpdateConfigurations = new[]
                {
                    new UpdateStorage
                    {
                        Name = "name",
                        ConnectionString = connectionString,
                        SchemaName = "schema"
                    }
                }
            }, null, (SqlServerStorageOptions)null);

            var configuration = system.ConfigurationStorage.Data.Single();
            Assert.Equal("name", configuration.Name);
            Assert.Equal(connectionString, configuration.ConnectionString);
            Assert.Equal("schema", configuration.SchemaName);
        }

        [Fact]
        public void ShouldActivateOnFirstUpdate()
        {
            var system = new SystemUnderTest();
            var connectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                UpdateConfigurations = new[]
                {
                    new UpdateStorage
                    {
                        Name = "name",
                        ConnectionString = connectionString
                    }
                }
            }, null, (SqlServerStorageOptions)null);

            Assert.True(system.ConfigurationStorage.Data.Single().Active);
        }

        [Fact]
        public void ShouldUpdateConfiguration()
        {
            var system = new SystemUnderTest();
            var previous = new SqlConnectionStringBuilder {DataSource = "previous"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name", ConnectionString = previous});

            var newConnectionString = new SqlConnectionStringBuilder {DataSource = "new"}.ToString();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                UpdateConfigurations = new[]
                {
                    new UpdateStorage
                    {
                        Name = "name",
                        ConnectionString = newConnectionString
                    }
                }
            }, null, (SqlServerStorageOptions)null);

            var configuration = system.ConfigurationStorage.Data.Single();
            Assert.Equal(newConnectionString, configuration.ConnectionString);
        }

        [Fact]
        public void ShouldConfigureUpdatedConfigurations()
        {
            var system = new SystemUnderTest();
            var connectionString1 = new SqlConnectionStringBuilder {DataSource = "DataSource1"}.ToString();
            var connectionString2 = new SqlConnectionStringBuilder {DataSource = "DataSource2"}.ToString();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                UpdateConfigurations = new[]
                {
                    new UpdateStorage
                    {
                        Name = "name1",
                        ConnectionString = connectionString1,
                        SchemaName = "schema1"
                    },
                    new UpdateStorage
                    {
                        Name = "name2",
                        ConnectionString = connectionString2,
                        SchemaName = "schema2"
                    }
                }
            }, null, (SqlServerStorageOptions)null);

            var configuration = system.ConfigurationStorage.Data.OrderBy(x => x.Id);
            Assert.Equal("name1", configuration.ElementAt(0).Name);
            Assert.Equal(connectionString1, configuration.ElementAt(0).ConnectionString);
            Assert.Equal("schema1", configuration.ElementAt(0).SchemaName);
            Assert.Equal("name2", configuration.ElementAt(1).Name);
            Assert.Equal(connectionString2, configuration.ElementAt(1).ConnectionString);
            Assert.Equal("schema2", configuration.ElementAt(1).SchemaName);
        }
    }
}