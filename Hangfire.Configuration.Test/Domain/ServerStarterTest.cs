using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ServerStarterTest
    {
        [Fact]
        public void ShouldStartServer()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages);

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
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, serverOptions, storages);

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
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, new BackgroundJobServerOptions {ServerName = "server!"}, storages);

            Assert.Null(system.Hangfire.StartedServers.Single().options.ServerName);
        }

        [Fact]
        public void ShouldPassAppBuilderToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages);

            Assert.Same(system.AppBuilder, system.Hangfire.StartedServers.Single().builder);
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToHangfire()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            var backgroundProcess = new Worker();
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages, backgroundProcess);

            Assert.Same(backgroundProcess, system.Hangfire.StartedServers.Single().backgroundProcesses.Single());
        }

        [Fact]
        public void ShouldStartTwoServers()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            system.Repository.Has(new StoredConfiguration());
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages);

            Assert.Equal(2, system.Hangfire.StartedServers.Count());
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToFirstServer()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            system.Repository.Has(new StoredConfiguration());
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages, new Worker());

            Assert.NotEmpty(system.Hangfire.StartedServers.First().backgroundProcesses);
            Assert.Empty(system.Hangfire.StartedServers.Last().backgroundProcesses);
        }

        [Fact]
        public void ShouldConstructHangfireStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages);

            Assert.NotNull(system.Hangfire.StartedServers.Single().storage);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = "connectionString"});
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages);

            Assert.Equal("connectionString", (system.Hangfire.StartedServers.Single().storage).ConnectionString);
        }
        
        [Fact]
        public void ShouldStartServersWithStorages()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());
            system.Repository.Has(new StoredConfiguration());
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages);

            Assert.Same(system.Hangfire.StartedServers.First().storage, storages.First().JobStorage);
            Assert.Same(system.Hangfire.StartedServers.Last().storage, storages.Last().JobStorage);
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToActiveServer()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {ConnectionString = "inactive"});
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "active"});
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(null, null, storages, new Worker());

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
            var storages = system.HangfireStarter.Start(null, null);

            system.ServerStarter.StartServers(configurationOptions, null, storages);

            var actual = system.Hangfire.StartedServers.Select(x => x.options.WorkerCount).OrderBy(x => x).ToArray();
            Assert.Equal(new[] {20, 100}, actual);
        }
    }
}