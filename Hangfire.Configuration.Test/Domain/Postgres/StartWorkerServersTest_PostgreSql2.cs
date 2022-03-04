using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Domain.Postgres
{
    public class StartWorkerServersTest
    {
        [Fact]
        public void ShouldStartServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.NotEmpty(system.Hangfire.StartedServers);
        }

        [Fact]
        public void ShouldPassServerOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
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

            system.WorkerServerStarter.Start(null, serverOptions, (PostgreSqlStorageOptions)null);

            Assert.Equal(serverOptions.Queues, system.Hangfire.StartedServers.Single().options.Queues);
            Assert.Equal(serverOptions.ServerTimeout, system.Hangfire.StartedServers.Single().options.ServerTimeout);
            Assert.Equal(serverOptions.HeartbeatInterval,
                system.Hangfire.StartedServers.Single().options.HeartbeatInterval);
            Assert.Equal(serverOptions.ShutdownTimeout,
                system.Hangfire.StartedServers.Single().options.ShutdownTimeout);
            Assert.Equal(serverOptions.ServerCheckInterval,
                system.Hangfire.StartedServers.Single().options.ServerCheckInterval);
            Assert.Equal(serverOptions.CancellationCheckInterval,
                system.Hangfire.StartedServers.Single().options.CancellationCheckInterval);
            Assert.Equal(serverOptions.SchedulePollingInterval,
                system.Hangfire.StartedServers.Single().options.SchedulePollingInterval);
            Assert.Equal(serverOptions.StopTimeout, system.Hangfire.StartedServers.Single().options.StopTimeout);
        }

        [Fact]
        public void ShouldPassNullServerNameToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, new BackgroundJobServerOptions {ServerName = "server!"}, (PostgreSqlStorageOptions)null);

            Assert.Null(system.Hangfire.StartedServers.Single().options.ServerName);
        }

        [Fact]
        public void ShouldPassAppBuilderToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.Same(system.ApplicationBuilder, system.Hangfire.StartedServers.Single().builder);
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            var backgroundProcess = new Worker();

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null, backgroundProcess);

            Assert.Contains(backgroundProcess, system.Hangfire.StartedServers.Single().backgroundProcesses);
        }

        [Fact]
        public void ShouldStartTwoServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.Equal(2, system.Hangfire.StartedServers.Count());
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToFirstServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null, new Worker());

            Assert.NotEmpty(system.Hangfire.StartedServers.First().backgroundProcesses);
            Assert.Empty(system.Hangfire.StartedServers.Last().backgroundProcesses);
        }

        [Fact]
        public void ShouldConstructHangfireStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.NotNull(system.Hangfire.StartedServers.Single().storage);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = @"Host=localhost;Database=fakedb;"});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.Equal(@"Host=localhost;Database=fakedb;", (system.Hangfire.StartedServers.Single().storage).ConnectionString);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorageWithOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration{ ConnectionString = @"Host=localhost;Database=fakedb;" });

            system.WorkerServerStarter.Start(null, null,
                new PostgreSqlStorageOptions() {PrepareSchemaIfNecessary = false});

            Assert.False(system.Hangfire.StartedServers.Single().storage.PostgresOptions.PrepareSchemaIfNecessary);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = "SchemaName"});

            system.WorkerServerStarter.Start(null, null, new PostgreSqlStorageOptions { SchemaName = "Ignored"});

            Assert.Equal("SchemaName", (system.Hangfire.StartedServers.Single().storage).SqlServerOptions.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration2()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = "SchemaName"});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.Equal("SchemaName", (system.Hangfire.StartedServers.Single().storage).SqlServerOptions.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfigurationOfTwoServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = "SchemaName1"});
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = "SchemaName2"});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.Equal("SchemaName1", (system.Hangfire.StartedServers.First().storage).SqlServerOptions.SchemaName);
            Assert.Equal("SchemaName2", (system.Hangfire.StartedServers.Last().storage).SqlServerOptions.SchemaName);
        }

        [Fact]
        public void ShouldUseDefaultSchemaName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = null, ConnectionString = @"Host=localhost;Database=fakedb;"});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.Equal(DefaultSchemaName.Postgres(),
                system.Hangfire.StartedServers.Single().storage.PostgresOptions.SchemaName);
        }

        [Fact]
        public void ShouldUseDefaultSchemaNameWhenEmpty()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = "", ConnectionString = @"Host=localhost;Database=fakedb;" });

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.Equal(DefaultSchemaName.Postgres(),
                system.Hangfire.StartedServers.Single().storage.PostgresOptions.SchemaName);
        }

        [Fact]
        public void ShouldPassStorageOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {});

            var options = new PostgreSqlStorageOptions
			{
                QueuePollInterval = TimeSpan.FromSeconds(1.0),
                DeleteExpiredBatchSize = 1,
                JobExpirationCheckInterval = TimeSpan.FromMinutes(4),
                DistributedLockTimeout = TimeSpan.FromMinutes(5.0),
                PrepareSchemaIfNecessary = !new PostgreSqlStorageOptions().PrepareSchemaIfNecessary,
                EnableTransactionScopeEnlistment = true,
                InvisibilityTimeout = TimeSpan.FromMinutes(7.0),
                TransactionSynchronisationTimeout = TimeSpan.FromMinutes(4),
				UseNativeDatabaseTransactions = true
            };
            system.WorkerServerStarter.Start(new ConfigurationOptions{ConnectionString = @"Host=localhost;Database=fakedb;"}, null, options);

            var storage = system.Hangfire.StartedServers.Single().storage;
            Assert.Equal(options.QueuePollInterval, storage.PostgresOptions.QueuePollInterval);
            Assert.Equal(options.DeleteExpiredBatchSize, storage.PostgresOptions.DeleteExpiredBatchSize);
            Assert.Equal(options.JobExpirationCheckInterval, storage.PostgresOptions.JobExpirationCheckInterval);
            Assert.Equal(options.DistributedLockTimeout, storage.PostgresOptions.DistributedLockTimeout);
            Assert.Equal(options.PrepareSchemaIfNecessary, storage.PostgresOptions.PrepareSchemaIfNecessary);
            Assert.Equal(options.EnableTransactionScopeEnlistment, storage.PostgresOptions.EnableTransactionScopeEnlistment);
            Assert.Equal(options.InvisibilityTimeout, storage.PostgresOptions.InvisibilityTimeout);
            Assert.Equal(options.TransactionSynchronisationTimeout, storage.PostgresOptions.TransactionSynchronisationTimeout);
            Assert.Equal(options.UseNativeDatabaseTransactions, storage.PostgresOptions.UseNativeDatabaseTransactions);
		}

        [Fact]
        public void ShouldPassDefaultStorageOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration { ConnectionString = @"Host=localhost;Database=active;"});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            var options = new PostgreSqlStorageOptions();
            var storage = system.Hangfire.StartedServers.Single().storage;
			Assert.Equal(options.QueuePollInterval, storage.PostgresOptions.QueuePollInterval);
			Assert.Equal(options.DeleteExpiredBatchSize, storage.PostgresOptions.DeleteExpiredBatchSize);
			Assert.Equal(options.JobExpirationCheckInterval, storage.PostgresOptions.JobExpirationCheckInterval);
			Assert.Equal(options.DistributedLockTimeout, storage.PostgresOptions.DistributedLockTimeout);
			Assert.Equal(options.PrepareSchemaIfNecessary, storage.PostgresOptions.PrepareSchemaIfNecessary);
			Assert.Equal(options.EnableTransactionScopeEnlistment, storage.PostgresOptions.EnableTransactionScopeEnlistment);
			Assert.Equal(options.InvisibilityTimeout, storage.PostgresOptions.InvisibilityTimeout);
			Assert.Equal(options.SchemaName, storage.PostgresOptions.SchemaName);
			Assert.Equal(options.TransactionSynchronisationTimeout, storage.PostgresOptions.TransactionSynchronisationTimeout);
			Assert.Equal(options.UseNativeDatabaseTransactions, storage.PostgresOptions.UseNativeDatabaseTransactions);
		}

        [Fact]
        public void ShouldPassBackgroundProcessesToActiveServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = @"Host=localhost;Database=inactive;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = @"Host=localhost;Database=active;" });

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null, new Worker());

            Assert.Empty(system.Hangfire.StartedServers.Single(x => x.storage.ConnectionString == @"Host=localhost;Database=inactive;")
                .backgroundProcesses);
            Assert.NotEmpty(system.Hangfire.StartedServers.Single(x => x.storage.ConnectionString == @"Host=localhost;Database=active;")
                .backgroundProcesses);
        }

        [Fact]
        public void ShouldGetGoalWorkerCountForTwoServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 20, ConnectionString = @"Host=localhost;Database=fakedb;"});
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 100, ConnectionString = @"Host=localhost;Database=fakedb;" });

            system.WorkerServerStarter.Start(new ConfigurationOptions(), null, (PostgreSqlStorageOptions)null);

            var actual = system.Hangfire.StartedServers.Select(x => x.options.WorkerCount).OrderBy(x => x).ToArray();
            Assert.Equal(new[] {20, 100}, actual);
        }
    }
}