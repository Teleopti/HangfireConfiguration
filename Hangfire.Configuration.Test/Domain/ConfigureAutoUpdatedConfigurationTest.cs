using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureAutoUpdatedConfigurationTest
    {
        [Fact]
        public void ShouldConfigureAutoUpdatedServer()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString()
            }, null, null);

            var dataSource = new SqlConnectionStringBuilder(system.Repository.Data.Single().ConnectionString).DataSource;
            Assert.Equal("DataSource", dataSource);
        }

        [Fact]
        public void ShouldNotConfigureAutoUpdatedServerIfNoneGiven()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions(), null, null);

            Assert.Empty(system.Repository.Data);
        }

        [Fact]
        public void ShouldMarkAutoUpdatedConnectionString()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName"}.ToString()
            }, null, null);

            var applicationName = new SqlConnectionStringBuilder(system.Repository.Data.Single().ConnectionString).ApplicationName;
            Assert.Equal("ApplicationName.AutoUpdate", applicationName);
        }

        [Fact]
        public void ShouldMarkAutoUpdatedConnectionStringWhenNoApplicationName()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString()
            }, null, null);

            var applicationName = new SqlConnectionStringBuilder(system.Repository.Data.Single().ConnectionString).ApplicationName;
            Assert.Equal("Hangfire.AutoUpdate", applicationName);
        }

        [Fact]
        public void ShouldNotUpdateExistingConfigurationThatIsNotMarked()
        {
            var system = new SystemUnderTest();
            var existing = new SqlConnectionStringBuilder {DataSource = "existing"}.ToString();
            system.Repository.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "autoupdated"}.ToString()
            }, null, null);

            var actual = system.Repository.Data.OrderBy(x => x.Id).First();
            Assert.Equal(existing, actual.ConnectionString);
        }

        [Fact]
        public void ShouldNotUpdateExistingConfigurationThatIsNotMarked2()
        {
            var system = new SystemUnderTest();
            var existing = new SqlConnectionStringBuilder {DataSource = "AnotherDataSourceWith.AutoUpdate.InIt"}.ToString();
            system.Repository.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "autoupdated"}.ToString()
            }, null, null);

            var actual = system.Repository.Data.OrderBy(x => x.Id).First();
            Assert.Equal(existing, actual.ConnectionString);
        }

        [Fact]
        public void ShouldNotUpdateExistingConfigurationThatIsNotMarked3()
        {
            var system = new SystemUnderTest();
            var existing = new SqlConnectionStringBuilder {ApplicationName = "ExistingApplicationWith.AutoUpdate.InIt"}.ToString();
            system.Repository.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "autoupdated"}.ToString()
            }, null, null);

            var actual = system.Repository.Data.OrderBy(x => x.Id).First();
            Assert.Equal(existing, actual.ConnectionString);
        }

        [Fact]
        public void ShouldAddAutoUpdatedConfigurationIfNoMarkedExists()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {DataSource = "existing"}.ToString()});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "autoupdated"}.ToString()
            }, null, null);

            var actual = system.Repository.Data.OrderBy(x => x.Id).Last();
            Assert.Equal("autoupdated", new SqlConnectionStringBuilder(actual.ConnectionString).DataSource);
        }

        [Fact]
        public void ShouldUpdate()
        {
            var system = new SystemUnderTest();
            var existing = new SqlConnectionStringBuilder {DataSource = "existingDataSource", ApplicationName = "existingApplicationName.AutoUpdate"}.ToString();
            system.Repository.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "newDataSource", ApplicationName = "newApplicationName"}.ToString()
            }, null, null);

            var updatedConnectionString = new SqlConnectionStringBuilder(system.Repository.Data.Single().ConnectionString);
            Assert.Equal("newDataSource", updatedConnectionString.DataSource);
            Assert.Equal("newApplicationName.AutoUpdate", updatedConnectionString.ApplicationName);
        }

        [Fact]
        public void ShouldUpdateLegacyConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {GoalWorkerCount = 55});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "dataSource", ApplicationName = "applicationName"}.ToString()
            }, null, null);

            var expected = new SqlConnectionStringBuilder {DataSource = "dataSource", ApplicationName = "applicationName.AutoUpdate"}.ToString();
            Assert.Equal(55, system.Repository.Data.Single().GoalWorkerCount);
            Assert.Equal(expected, system.Repository.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldUpdateOneOfTwo()
        {
            var system = new SystemUnderTest();
            var one = new SqlConnectionStringBuilder {DataSource = "One"}.ToString();
            var two = new SqlConnectionStringBuilder {DataSource = "Two", ApplicationName = "Two.AutoUpdate"}.ToString();
            system.Repository.Has(new StoredConfiguration {ConnectionString = one});
            system.Repository.Has(new StoredConfiguration {ConnectionString = two});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "UpdatedTwo", ApplicationName = "UpdatedTwo"}.ToString()
            }, null, null);

            var expected = new SqlConnectionStringBuilder {DataSource = "UpdatedTwo", ApplicationName = "UpdatedTwo.AutoUpdate"}.ToString();
            Assert.Equal(expected, system.Repository.Data.Last().ConnectionString);
        }

        [Fact]
        public void ShouldActivateOnFirstUpdate()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString()
            }, null, null);

            Assert.True(system.Repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldActivateLegacyConfigurationWhenConfiguredAsDefault()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {GoalWorkerCount = 4});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString()
            }, null, null);

            Assert.True(system.Repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldBeActiveOnUpdateIfActiveBefore()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(),
                Active = true
            });

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString()
            }, null, null);

            Assert.True(system.Repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldNotActivateWhenUpdating()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(), Active = false});
            system.Repository.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(), Active = true});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "AutoUpdate"}.ToString()
            }, null, null);

            Assert.False(system.Repository.Data.First().Active);
            Assert.True(system.Repository.Data.Last().Active);
        }

        [Fact]
        public void ShouldSaveSchemaName()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "AutoUpdate"}.ToString(),
                AutoUpdatedHangfireSchemaName = "schemaName"
            }, null, null);

            Assert.Equal("schemaName", system.Repository.Data.Single().SchemaName);
        }

        [Fact]
        public void ShouldSaveSchemaNameOnLegacyConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {GoalWorkerCount = 4});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "AutoUpdate"}.ToString(),
                AutoUpdatedHangfireSchemaName = "schemaName"
            }, null, null);

            Assert.Equal("schemaName", system.Repository.Data.Single().SchemaName);
        }

        [Fact]
        public void ShouldOnlyAutoUpdateOnce()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "FirstUpdate"}.ToString()
            }, null, null);

            system.PublisherQueries.QueryPublishers(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "SecondUpdate"}.ToString()
            }, null);

            var dataSource = new SqlConnectionStringBuilder(system.Repository.Data.Single().ConnectionString).DataSource;
            Assert.Equal("FirstUpdate", dataSource);
        }

        [Fact]
        public void ShouldAutoUpdateTwiceIfAllConfigurationsWhereRemoved()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "FirstUpdate"}.ToString()
            }, null, null);
            system.Repository.Data = Enumerable.Empty<StoredConfiguration>();
            
            system.PublisherQueries.QueryPublishers(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "SecondUpdate"}.ToString()
            }, null);

            var dataSource = new SqlConnectionStringBuilder(system.Repository.Data.Single().ConnectionString).DataSource;
            Assert.Equal("SecondUpdate", dataSource);
        }

        [Fact]
        public void ShouldAutoUpdateWithDefaultConfigurationName()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString()
            }, null, null);

            Assert.Equal("Hangfire", system.Repository.Data.Single().Name);
        }

        [Fact]
        public void ShouldAutoUpdateWithDefaultConfigurationName2()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(), Active = false});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString()
            }, null, null);

            Assert.Equal("Hangfire", system.Repository.Data.Single().Name);
        }
    }
}