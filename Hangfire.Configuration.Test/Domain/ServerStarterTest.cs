using System;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Microsoft.Owin.Builder;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ServerStarterTest
    {
        [Fact]
        public void ShouldStartServer()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null, null);

            Assert.NotEmpty(useHangfireServer.StartedServers);
        }

        [Fact]
        public void ShouldPassServerOptionsToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, new BackgroundJobServerOptions {Queues = new[] {"queue1", "queue2"}}, null);

            Assert.Equal(new[] {"queue1", "queue2"}, useHangfireServer.StartedServers.Single().options.Queues);
        }

        [Fact]
        public void ShouldPassNullServerNameToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, new BackgroundJobServerOptions {ServerName = "server!"}, null);

            Assert.Null(useHangfireServer.StartedServers.Single().options.ServerName);
        }

        [Fact]
        public void ShouldPassAppBuilderToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var appBuilder = new AppBuilder();
            var target = new ServerStarter(appBuilder, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null, null);

            Assert.Same(appBuilder, useHangfireServer.StartedServers.Single().builder);
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);
            var backgroundProcess = new Worker();

            target.StartServers(null, null, null, backgroundProcess);

            Assert.Same(backgroundProcess, useHangfireServer.StartedServers.Single().backgroundProcesses.Single());
        }

        [Fact]
        public void ShouldStartTwoServers()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null, null);

            Assert.Equal(2, useHangfireServer.StartedServers.Count());
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToFirstServer()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null, null, new Worker());

            Assert.NotEmpty(useHangfireServer.StartedServers.First().backgroundProcesses);
            Assert.Empty(useHangfireServer.StartedServers.Last().backgroundProcesses);
        }

        [Fact]
        public void ShouldConstructHangfireStorage()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null, null);

            Assert.NotNull(useHangfireServer.StartedServers.Single().storage);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorage()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {ConnectionString = "connectionString"});
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null, null);

            Assert.Equal("connectionString", (hangfire.StartedServers.Single().storage as FakeJobStorage).ConnectionString);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorageWithOptions()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null, new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});

            Assert.False((hangfire.StartedServers.Single().storage as FakeJobStorage).Options.PrepareSchemaIfNecessary);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {SchemaName = "SchemaName"});
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null, new SqlServerStorageOptions {SchemaName = "Ignored"});

            Assert.Equal("SchemaName", (hangfire.StartedServers.Single().storage as FakeJobStorage).Options.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration2()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {SchemaName = "SchemaName"});
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null, null);

            Assert.Equal("SchemaName", (hangfire.StartedServers.Single().storage as FakeJobStorage).Options.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfigurationOfTwoServers()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {SchemaName = "SchemaName1"});
            repository.Has(new StoredConfiguration {SchemaName = "SchemaName2"});
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null, null);

            Assert.Equal("SchemaName1", (hangfire.StartedServers.First().storage as FakeJobStorage).Options.SchemaName);
            Assert.Equal("SchemaName2", (hangfire.StartedServers.Last().storage as FakeJobStorage).Options.SchemaName);
        }

        [Fact]
        public void ShouldPassStorageOptionsToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(1.0),
                SlidingInvisibilityTimeout = TimeSpan.FromSeconds(2),
                InvisibilityTimeout = TimeSpan.FromMinutes(3),
                JobExpirationCheckInterval = TimeSpan.FromMinutes(4),
                CountersAggregateInterval = TimeSpan.FromMinutes(5.0),
                PrepareSchemaIfNecessary = !new SqlServerStorageOptions().PrepareSchemaIfNecessary,
                DashboardJobListLimit = 6,
                TransactionTimeout = TimeSpan.FromMinutes(7.0),
                DisableGlobalLocks = !new SqlServerStorageOptions().DisableGlobalLocks,
                UsePageLocksOnDequeue = !new SqlServerStorageOptions().UsePageLocksOnDequeue
            };
            target.StartServers(null, null, options);

            var storage = hangfire.StartedServers.Single().storage as FakeJobStorage;
            Assert.Equal(options.QueuePollInterval, storage.Options.QueuePollInterval);
            Assert.Equal(options.SlidingInvisibilityTimeout, storage.Options.SlidingInvisibilityTimeout);
            Assert.Equal(options.InvisibilityTimeout, storage.Options.InvisibilityTimeout);
            Assert.Equal(options.JobExpirationCheckInterval, storage.Options.JobExpirationCheckInterval);
            Assert.Equal(options.CountersAggregateInterval, storage.Options.CountersAggregateInterval);
            Assert.Equal(options.PrepareSchemaIfNecessary, storage.Options.PrepareSchemaIfNecessary);
            Assert.Equal(options.DashboardJobListLimit, storage.Options.DashboardJobListLimit);
            Assert.Equal(options.TransactionTimeout, storage.Options.TransactionTimeout);
            Assert.Equal(options.DisableGlobalLocks, storage.Options.DisableGlobalLocks);
            Assert.Equal(options.UsePageLocksOnDequeue, storage.Options.UsePageLocksOnDequeue);
        }

        [Fact]
        public void ShouldPassDefaultStorageOptionsToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null, null);

            var options = new SqlServerStorageOptions();
            var storage = hangfire.StartedServers.Single().storage as FakeJobStorage;
            Assert.Equal(options.QueuePollInterval, storage.Options.QueuePollInterval);
            Assert.Equal(options.SlidingInvisibilityTimeout, storage.Options.SlidingInvisibilityTimeout);
            Assert.Equal(options.InvisibilityTimeout, storage.Options.InvisibilityTimeout);
            Assert.Equal(options.JobExpirationCheckInterval, storage.Options.JobExpirationCheckInterval);
            Assert.Equal(options.CountersAggregateInterval, storage.Options.CountersAggregateInterval);
            Assert.Equal(options.PrepareSchemaIfNecessary, storage.Options.PrepareSchemaIfNecessary);
            Assert.Equal(options.DashboardJobListLimit, storage.Options.DashboardJobListLimit);
            Assert.Equal(options.TransactionTimeout, storage.Options.TransactionTimeout);
            Assert.Equal(options.DisableGlobalLocks, storage.Options.DisableGlobalLocks);
            Assert.Equal(options.UsePageLocksOnDequeue, storage.Options.UsePageLocksOnDequeue);
        }

        [Fact]
        public void ShouldReturnStartedServers()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            repository.Has(new StoredConfiguration());
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            var result = target.StartServers(null, null, null);

            Assert.Equal(1, result.First().Number);
            Assert.Same(hangfire.StartedServers.First().storage, result.First().Storage);
            Assert.Equal(2, result.Last().Number);
            Assert.Same(hangfire.StartedServers.Last().storage, result.Last().Storage);
        }
//        
//        [Fact]
//        public void ShouldStartWithWorkerCount()
//        {
//            var repository = new FakeConfigurationRepository();
//            var hangfire = new FakeHangfire();
//            var serverOptions = new BackgroundJobServerOptions
//            {
//                WorkerCount = 1
//            };
//            var target = new ServerStarter(null, new Configuration(repository), hangfire);
//
//            target.StartServers(serverOptions, null);            
//
//            Assert.Equal(1, hangfire.StartedServers.Single().options.WorkerCount);
//        }
    }
}