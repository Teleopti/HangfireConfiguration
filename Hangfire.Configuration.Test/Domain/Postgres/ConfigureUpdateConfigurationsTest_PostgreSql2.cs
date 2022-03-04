using System.Linq;
using Hangfire.PostgreSql;
using Npgsql;
using Xunit;

namespace Hangfire.Configuration.Test.Domain.Postgres
{
    public class ConfigureUpdateConfigurationsTest
    {
        [Fact]
        public void ShouldConfigureUpdatedConfiguration()
        {
            var system = new SystemUnderTest();
            var connectionString = new NpgsqlConnectionStringBuilder() {Host = "host"}.ToString();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                UpdateConfigurations = new[]
                {
                    new UpdateStorageConfiguration
                    {
                        Name = "name",
                        ConnectionString = connectionString,
                        SchemaName = "schema"
                    }
                }
            }, null, (PostgreSqlStorageOptions)null);

            var configuration = system.ConfigurationStorage.Data.Single();
            Assert.Equal("name", configuration.Name);
            Assert.Equal(connectionString, configuration.ConnectionString);
            Assert.Equal("schema", configuration.SchemaName);
        }

        [Fact]
        public void ShouldActivateOnFirstUpdate()
        {
            var system = new SystemUnderTest();
            var connectionString = new NpgsqlConnectionStringBuilder() { Host = "host" }.ToString();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                UpdateConfigurations = new[]
                {
                    new UpdateStorageConfiguration
                    {
                        Name = "name",
                        ConnectionString = connectionString
                    }
                }
            }, null, (PostgreSqlStorageOptions)null);

            Assert.True(system.ConfigurationStorage.Data.Single().Active);
        }

        [Fact]
        public void ShouldUpdateConfiguration()
        {
            var system = new SystemUnderTest();
            var previous = new NpgsqlConnectionStringBuilder() { Host = "previous" }.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name", ConnectionString = previous});

            var newConnectionString = new NpgsqlConnectionStringBuilder() {Host = "new"}.ToString();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                UpdateConfigurations = new[]
                {
                    new UpdateStorageConfiguration
                    {
                        Name = "name",
                        ConnectionString = newConnectionString
                    }
                }
            }, null, (PostgreSqlStorageOptions)null);

            var configuration = system.ConfigurationStorage.Data.Single();
            Assert.Equal(newConnectionString, configuration.ConnectionString);
        }

        [Fact]
        public void ShouldConfigureUpdatedConfigurations()
        {
            var system = new SystemUnderTest();
            var connectionString1 = new NpgsqlConnectionStringBuilder() { Host = "host1" }.ToString();
            var connectionString2 = new NpgsqlConnectionStringBuilder() { Host = "host2" }.ToString();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                UpdateConfigurations = new[]
                {
                    new UpdateStorageConfiguration
                    {
                        Name = "name1",
                        ConnectionString = connectionString1,
                        SchemaName = "schema1"
                    },
                    new UpdateStorageConfiguration
                    {
                        Name = "name2",
                        ConnectionString = connectionString2,
                        SchemaName = "schema2"
                    }
                }
            }, null, (PostgreSqlStorageOptions)null);

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