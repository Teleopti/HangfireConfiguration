using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.SqlServer;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class StartWorkerServersTest
    {
        [Fact]
        public void ShouldStartServer()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.NotEmpty(system.Hangfire.StartedServers);
        }

        [Fact]
        public void ShouldPassServerOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            var serverOptions = new BackgroundJobServerOptions
            {
                Queues = new[] {"queue1", "queue2"},
                ServerTimeout = new TimeSpan(20),
                HeartbeatInterval = new TimeSpan(30),
                ShutdownTimeout = new TimeSpan(40),
                ServerCheckInterval = new TimeSpan(50),
                CancellationCheckInterval = new TimeSpan(60),
                SchedulePollingInterval = new TimeSpan(70),
                StopTimeout = new TimeSpan(80),
                Activator = new JobActivator(),
                FilterProvider = new JobFilterCollection(),
                TaskScheduler = TaskScheduler.Current,
                TimeZoneResolver = new DefaultTimeZoneResolver()
            };

            system.WorkerServerStarter.Start(null, serverOptions, null);

            Assert.Equal(serverOptions.Queues, system.Hangfire.StartedServers.Single().options.Queues);
            Assert.Equal(serverOptions.ServerTimeout, system.Hangfire.StartedServers.Single().options.ServerTimeout);
            Assert.Equal(serverOptions.HeartbeatInterval, system.Hangfire.StartedServers.Single().options.HeartbeatInterval);
            Assert.Equal(serverOptions.ShutdownTimeout, system.Hangfire.StartedServers.Single().options.ShutdownTimeout);
            Assert.Equal(serverOptions.ServerCheckInterval, system.Hangfire.StartedServers.Single().options.ServerCheckInterval);
            Assert.Equal(serverOptions.CancellationCheckInterval, system.Hangfire.StartedServers.Single().options.CancellationCheckInterval);
            Assert.Equal(serverOptions.SchedulePollingInterval, system.Hangfire.StartedServers.Single().options.SchedulePollingInterval);
            Assert.Equal(serverOptions.StopTimeout, system.Hangfire.StartedServers.Single().options.StopTimeout);
        }

        [Fact]
        public void ShouldPassNullServerNameToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, new BackgroundJobServerOptions {ServerName = "server!"}, null);

            Assert.Null(system.Hangfire.StartedServers.Single().options.ServerName);
        }

        [Fact]
        public void ShouldPassAppBuilderToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Same(system.AppBuilder, system.Hangfire.StartedServers.Single().builder);
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            var backgroundProcess = new Worker();

            system.WorkerServerStarter.Start(null, null, null, backgroundProcess);

            Assert.Same(backgroundProcess, system.Hangfire.StartedServers.Single().backgroundProcesses.Single());
        }

        [Fact]
        public void ShouldStartTwoServers()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            system.Repository.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(2, system.Hangfire.StartedServers.Count());
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToFirstServer()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            system.Repository.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, null, new Worker());

            Assert.NotEmpty(system.Hangfire.StartedServers.First().backgroundProcesses);
            Assert.Empty(system.Hangfire.StartedServers.Last().backgroundProcesses);
        }

        [Fact]
        public void ShouldConstructHangfireStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.NotNull(system.Hangfire.StartedServers.Single().storage);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = "connectionString"});

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal("connectionString", (system.Hangfire.StartedServers.Single().storage).ConnectionString);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorageWithOptions()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});

            Assert.False((system.Hangfire.StartedServers.Single().storage).Options.PrepareSchemaIfNecessary);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {SchemaName = "SchemaName"});

            system.WorkerServerStarter.Start(null, null, new SqlServerStorageOptions {SchemaName = "Ignored"});

            Assert.Equal("SchemaName", (system.Hangfire.StartedServers.Single().storage).Options.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration2()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {SchemaName = "SchemaName"});

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal("SchemaName", (system.Hangfire.StartedServers.Single().storage).Options.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfigurationOfTwoServers()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {SchemaName = "SchemaName1"});
            system.Repository.Has(new StoredConfiguration {SchemaName = "SchemaName2"});

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal("SchemaName1", (system.Hangfire.StartedServers.First().storage).Options.SchemaName);
            Assert.Equal("SchemaName2", (system.Hangfire.StartedServers.Last().storage).Options.SchemaName);
        }

        [Fact]
        public void ShouldPassStorageOptionsToHangfire()
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
            system.WorkerServerStarter.Start(null, null, options);

            var storage = system.Hangfire.StartedServers.Single().storage;
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
        public void ShouldPassDefaultStorageOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, null);

            var options = new SqlServerStorageOptions();
            var storage = system.Hangfire.StartedServers.Single().storage;
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

        [Fact(Skip = "Not sure")]
        public void ShouldReturnStartedServers()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            system.Repository.Has(new StoredConfiguration());
//
//            var result = system.WorkerServerStarter.Start(null, null, null);
//
//            Assert.Equal(1, result.First().Number);
//            Assert.Same(system.Hangfire.StartedServers.First().storage, result.First().Storage);
//            Assert.Equal(2, result.Last().Number);
//            Assert.Same(system.Hangfire.StartedServers.Last().storage, result.Last().Storage);
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToActiveServer()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = "inactive"});
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "active"});

            system.WorkerServerStarter.Start(null, null, null, new Worker());

            Assert.Empty(system.Hangfire.StartedServers.Single(x => x.storage.ConnectionString == "inactive").backgroundProcesses);
            Assert.NotEmpty(system.Hangfire.StartedServers.Single(x => x.storage.ConnectionString == "active").backgroundProcesses);
        }

        [Fact]
        public void ShouldGetWorkerCountForTwoServers()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration() {GoalWorkerCount = 20});
            system.Repository.Has(new StoredConfiguration() {GoalWorkerCount = 100});
            var configurationOptions = new ConfigurationOptions()
            {
                MinimumServers = 1
            };

            system.WorkerServerStarter.Start(configurationOptions, null, null);

            var actual = system.Hangfire.StartedServers.Select(x => x.options.WorkerCount).OrderBy(x => x).ToArray();
            Assert.Equal(new[] {20, 100}, actual);
        }
    }
}