using System;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.SqlServer;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class HangfireStarterTest
    {

        [Fact]
        public void ShouldStart()
        {
            var system = new SystemUnderTest();

            system.HangfireStarter.Start(new ConfigurationOptions() {DefaultHangfireConnectionString = "HangfireConnectionString"}, null);

            Assert.NotNull(system.Hangfire.CurrentStorage);
        }
        
        //Should this maybe throw???? 
        [Fact]
        public void ShouldNotStart()
        {
            var system = new SystemUnderTest();

            system.HangfireStarter.Start(null, null);

            Assert.Null(system.Hangfire.CurrentStorage);
        }

        [Fact]
        public void ShouldStartWithExistingActiveConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true});

            system.HangfireStarter.Start(null, null);

            Assert.NotNull(system.Hangfire.CurrentStorage);
        }

        [Fact]
        public void ShouldStartWithActiveStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            system.Repository.Has(new StoredConfiguration() {Active = true, SchemaName = "ActiveSchema"});

            system.HangfireStarter.Start(null, null);

            Assert.Equal("ActiveSchema", (system.Hangfire.CurrentStorage as FakeJobStorage).Options.SchemaName);
        }
        
        [Fact]
        public void ShouldReturnConfiguredStorages()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration() {SchemaName = "FirstSchema"});
            system.Repository.Has(new StoredConfiguration() {SchemaName = "SecondSchema"});

            var storages = system.HangfireStarter.Start(null, null);
            
            Assert.Equal("FirstSchema", (storages.First() as FakeJobStorage).Options.SchemaName);
            Assert.Equal("SecondSchema", (storages.Last() as FakeJobStorage).Options.SchemaName);
        }        
        
        [Fact]
        public void ShouldPassDefaultStorageOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            var storages = system.HangfireStarter.Start(null, null);

            var options = new SqlServerStorageOptions();
            var storage = storages.Single() as FakeJobStorage;
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
            system.Repository.Has(new StoredConfiguration());
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

            var storages = system.HangfireStarter.Start(null, options);

            var storage = storages.Single() as FakeJobStorage;
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
            system.Repository.Has(new StoredConfiguration {SchemaName = "SchemaName"});

            system.ServerStarter.StartServers(null, null);

            Assert.Equal("SchemaName", (system.Hangfire.StartedServers.Single().storage).Options.SchemaName);
        }
        
        [Fact]
        public void ShouldUseSchemaNameFromTwoConfigurations()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {SchemaName = "SchemaName1"});
            system.Repository.Has(new StoredConfiguration {SchemaName = "SchemaName2"});

            system.ServerStarter.StartServers(null, null);

            Assert.Equal("SchemaName1", (system.Hangfire.StartedServers.First().storage).Options.SchemaName);
            Assert.Equal("SchemaName2", (system.Hangfire.StartedServers.Last().storage).Options.SchemaName);
        }        

        

    }
}