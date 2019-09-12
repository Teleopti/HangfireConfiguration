using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class StartPublishersTest
    {
        [Fact]
        public void ShouldConfigureAndStartWithAutoUpdatedConnectionString()
        {
            var system = new SystemUnderTest();

            system.PublisherStarter.Start(
                new ConfigurationOptions
                {
                    AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder
                    {
                        DataSource = "Hangfire"
                    }.ToString()
                }, null);

            Assert.NotNull(system.Hangfire.LastCreatedStorage);
        }

        //Should this maybe throw???? 
        [Fact]
        public void ShouldNotStart()
        {
            var system = new SystemUnderTest();

            system.PublisherStarter.Start(null, null);

            Assert.Null(system.Hangfire.LastCreatedStorage);
        }

        [Fact]
        public void ShouldStartWithExistingActiveConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true});

            system.PublisherStarter.Start(null, null);

            Assert.NotNull(system.Hangfire.LastCreatedStorage);
        }

        [Fact]
        public void ShouldStartWithActiveStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            system.Repository.Has(new StoredConfiguration() {Active = true, SchemaName = "ActiveSchema"});

            system.PublisherStarter.Start(null, null);

            Assert.Equal("ActiveSchema", system.Hangfire.LastCreatedStorage.Options.SchemaName);
        }

        [Fact]
        public void ShouldPassDefaultStorageOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true});

            system.PublisherStarter.Start(null, null);

            var options = new SqlServerStorageOptions();
            var storage = system.Hangfire.CreatedStorages.Single();
            Assert.Equal(options.QueuePollInterval, storage.Options.QueuePollInterval);
            Assert.Equal(options.SlidingInvisibilityTimeout, storage.Options.SlidingInvisibilityTimeout);
            Assert.Equal(options.JobExpirationCheckInterval, storage.Options.JobExpirationCheckInterval);
            Assert.Equal(options.CountersAggregateInterval, storage.Options.CountersAggregateInterval);
            Assert.Equal(options.PrepareSchemaIfNecessary, storage.Options.PrepareSchemaIfNecessary);
            Assert.Equal(options.DashboardJobListLimit, storage.Options.DashboardJobListLimit);
            Assert.Equal(options.TransactionTimeout, storage.Options.TransactionTimeout);
            Assert.Equal(options.DisableGlobalLocks, storage.Options.DisableGlobalLocks);
            Assert.Equal(options.UsePageLocksOnDequeue, storage.Options.UsePageLocksOnDequeue);
        }

        [Fact]
        public void ShouldUseStorageOptions()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true});
            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(1.0),
                SlidingInvisibilityTimeout = TimeSpan.FromSeconds(2),
                JobExpirationCheckInterval = TimeSpan.FromMinutes(4),
                CountersAggregateInterval = TimeSpan.FromMinutes(5.0),
                PrepareSchemaIfNecessary = !new SqlServerStorageOptions().PrepareSchemaIfNecessary,
                DashboardJobListLimit = 6,
                TransactionTimeout = TimeSpan.FromMinutes(7.0),
                DisableGlobalLocks = !new SqlServerStorageOptions().DisableGlobalLocks,
                UsePageLocksOnDequeue = !new SqlServerStorageOptions().UsePageLocksOnDequeue
            };

            system.PublisherStarter.Start(null, options);

            var storage = system.Hangfire.CreatedStorages.Single();
            Assert.Equal(options.QueuePollInterval, storage.Options.QueuePollInterval);
            Assert.Equal(options.SlidingInvisibilityTimeout, storage.Options.SlidingInvisibilityTimeout);
            Assert.Equal(options.JobExpirationCheckInterval, storage.Options.JobExpirationCheckInterval);
            Assert.Equal(options.CountersAggregateInterval, storage.Options.CountersAggregateInterval);
            Assert.Equal(options.PrepareSchemaIfNecessary, storage.Options.PrepareSchemaIfNecessary);
            Assert.Equal(options.DashboardJobListLimit, storage.Options.DashboardJobListLimit);
            Assert.Equal(options.TransactionTimeout, storage.Options.TransactionTimeout);
            Assert.Equal(options.DisableGlobalLocks, storage.Options.DisableGlobalLocks);
            Assert.Equal(options.UsePageLocksOnDequeue, storage.Options.UsePageLocksOnDequeue);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true, SchemaName = "SchemaName"});

            system.PublisherStarter.Start(null, null);

            Assert.Equal("SchemaName", system.Hangfire.CreatedStorages.Single().Options.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromTwoConfigurations()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true, SchemaName = "SchemaName1"});
            system.Repository.Has(new StoredConfiguration {Active = true, SchemaName = "SchemaName2"});

            system.PublisherStarter.Start(null, null);

            var storages = system.Hangfire.CreatedStorages;
            Assert.Equal("SchemaName1", storages.First().Options.SchemaName);
            Assert.Equal("SchemaName2", storages.Last().Options.SchemaName);
        }

        [Fact]
        public void ShouldNotCreateInactiveStorages()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = false});
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "active"});
            system.Repository.Has(new StoredConfiguration {Active = false});

            system.PublisherStarter.Start(null, null);

            Assert.Same("active", system.Hangfire.CreatedStorages.Single().ConnectionString);
        }
    }
}