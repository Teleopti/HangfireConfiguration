using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.PostgreSql;
using Hangfire.Server;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class StartWorkerServersPostgresTest
    {
        [Test]
        public void ShouldStartServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start();

            system.Hangfire.StartedServers
	            .Should().Not.Be.Empty();
        }

        [Test]
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

            Assert.AreEqual(serverOptions.Queues, system.Hangfire.StartedServers.Single().options.Queues);
            Assert.AreEqual(serverOptions.ServerTimeout, system.Hangfire.StartedServers.Single().options.ServerTimeout);
            Assert.AreEqual(serverOptions.HeartbeatInterval,
                system.Hangfire.StartedServers.Single().options.HeartbeatInterval);
            Assert.AreEqual(serverOptions.ShutdownTimeout,
                system.Hangfire.StartedServers.Single().options.ShutdownTimeout);
            Assert.AreEqual(serverOptions.ServerCheckInterval,
                system.Hangfire.StartedServers.Single().options.ServerCheckInterval);
            Assert.AreEqual(serverOptions.CancellationCheckInterval,
                system.Hangfire.StartedServers.Single().options.CancellationCheckInterval);
            Assert.AreEqual(serverOptions.SchedulePollingInterval,
                system.Hangfire.StartedServers.Single().options.SchedulePollingInterval);
            Assert.AreEqual(serverOptions.StopTimeout, system.Hangfire.StartedServers.Single().options.StopTimeout);
        }

        [Test]
        public void ShouldPassNullServerNameToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, new BackgroundJobServerOptions {ServerName = "server!"}, (PostgreSqlStorageOptions)null);

            Assert.Null(system.Hangfire.StartedServers.Single().options.ServerName);
        }

        [Test]
        public void ShouldPassAppBuilderToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start();

            system.Hangfire.StartedServers.Single().builder
	            .Should().Be.SameInstanceAs(system.ApplicationBuilder);
        }

        [Test]
        public void ShouldPassBackgroundProcessesToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            var backgroundProcess = new Worker();

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null, backgroundProcess);

            Assert.Contains(backgroundProcess, system.Hangfire.StartedServers.Single().backgroundProcesses);
        }

        [Test]
        public void ShouldStartTwoServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start();

            Assert.AreEqual(2, system.Hangfire.StartedServers.Count());
        }

        [Test]
        public void ShouldPassBackgroundProcessesToFirstServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null, new Worker());

            system.Hangfire.StartedServers.First().backgroundProcesses
	            .Should().Not.Be.Empty();
            Assert.IsEmpty(system.Hangfire.StartedServers.Last().backgroundProcesses);
        }

        [Test]
        public void ShouldConstructHangfireStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start();

            Assert.NotNull(system.Hangfire.StartedServers.Single().storage);
        }

        [Test]
        public void ShouldConstructSqlHangfireStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = @"Host=localhost;Database=fakedb;"});

            system.WorkerServerStarter.Start();

            Assert.AreEqual(@"Host=localhost;Database=fakedb;", (system.Hangfire.StartedServers.Single().storage).ConnectionString);
        }

        [Test]
        public void ShouldConstructSqlHangfireStorageWithOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration { ConnectionString = @"Host=localhost;Database=fakedb;" });

            system.Options.UseStorageOptions(new PostgreSqlStorageOptions { PrepareSchemaIfNecessary = false});
            system.WorkerServerStarter.Start();

            Assert.False(system.Hangfire.StartedServers.Single().storage.PostgresOptions.PrepareSchemaIfNecessary);
        }

        [Test]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = "SchemaName", 
	            ConnectionString = @"Host=localhost;Database=fakedb;"
            });

            system.Options.UseStorageOptions(new PostgreSqlStorageOptions { SchemaName = "Ignored" });
            system.WorkerServerStarter.Start();

            Assert.AreEqual("SchemaName", system.Hangfire.StartedServers.Single().storage.PostgresOptions.SchemaName);
        }

        [Test]
        public void ShouldUseSchemaNameFromConfiguration2()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = "SchemaName", 
	            ConnectionString = @"Host=localhost;Database=fakedb;"
            });

            system.WorkerServerStarter.Start();

            Assert.AreEqual("SchemaName", system.Hangfire.StartedServers.Single().storage.PostgresOptions.SchemaName);
        }

        [Test]
        public void ShouldUseSchemaNameFromConfigurationOfTwoServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = "SchemaName1", 
	            ConnectionString = @"Host=localhost;Database=fakedb;"
            });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = "SchemaName2", 
	            ConnectionString = @"Host=localhost;Database=fakedb;"
            });

            system.WorkerServerStarter.Start();

            Assert.AreEqual("SchemaName1", system.Hangfire.StartedServers.First().storage.PostgresOptions.SchemaName);
            Assert.AreEqual("SchemaName2", system.Hangfire.StartedServers.Last().storage.PostgresOptions.SchemaName);
        }

        [Test]
        public void ShouldUseDefaultSchemaName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = null, ConnectionString = @"Host=localhost;Database=fakedb;"});

            system.WorkerServerStarter.Start();

            Assert.AreEqual(DefaultSchemaName.Postgres(),
                system.Hangfire.StartedServers.Single().storage.PostgresOptions.SchemaName);
        }

        [Test]
        public void ShouldUseDefaultSchemaNameWhenEmpty()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = "", ConnectionString = @"Host=localhost;Database=fakedb;" });

            system.WorkerServerStarter.Start();

            Assert.AreEqual(DefaultSchemaName.Postgres(),
                system.Hangfire.StartedServers.Single().storage.PostgresOptions.SchemaName);
        }

        [Test]
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
            Assert.AreEqual(options.QueuePollInterval, storage.PostgresOptions.QueuePollInterval);
            Assert.AreEqual(options.DeleteExpiredBatchSize, storage.PostgresOptions.DeleteExpiredBatchSize);
            Assert.AreEqual(options.JobExpirationCheckInterval, storage.PostgresOptions.JobExpirationCheckInterval);
            Assert.AreEqual(options.DistributedLockTimeout, storage.PostgresOptions.DistributedLockTimeout);
            Assert.AreEqual(options.PrepareSchemaIfNecessary, storage.PostgresOptions.PrepareSchemaIfNecessary);
            Assert.AreEqual(options.EnableTransactionScopeEnlistment, storage.PostgresOptions.EnableTransactionScopeEnlistment);
            Assert.AreEqual(options.InvisibilityTimeout, storage.PostgresOptions.InvisibilityTimeout);
            Assert.AreEqual(options.TransactionSynchronisationTimeout, storage.PostgresOptions.TransactionSynchronisationTimeout);
            Assert.AreEqual(options.UseNativeDatabaseTransactions, storage.PostgresOptions.UseNativeDatabaseTransactions);
		}

        [Test]
        public void ShouldPassDefaultStorageOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            ConnectionString = @"Host=localhost;Database=active;"
            });

            system.WorkerServerStarter.Start();

            var options = new PostgreSqlStorageOptions();
            var storage = system.Hangfire.StartedServers.Single().storage;
			Assert.AreEqual(options.QueuePollInterval, storage.PostgresOptions.QueuePollInterval);
			Assert.AreEqual(options.DeleteExpiredBatchSize, storage.PostgresOptions.DeleteExpiredBatchSize);
			Assert.AreEqual(options.JobExpirationCheckInterval, storage.PostgresOptions.JobExpirationCheckInterval);
			Assert.AreEqual(options.DistributedLockTimeout, storage.PostgresOptions.DistributedLockTimeout);
			Assert.AreEqual(options.PrepareSchemaIfNecessary, storage.PostgresOptions.PrepareSchemaIfNecessary);
			Assert.AreEqual(options.EnableTransactionScopeEnlistment, storage.PostgresOptions.EnableTransactionScopeEnlistment);
			Assert.AreEqual(options.InvisibilityTimeout, storage.PostgresOptions.InvisibilityTimeout);
			Assert.AreEqual(options.SchemaName, storage.PostgresOptions.SchemaName);
			Assert.AreEqual(options.TransactionSynchronisationTimeout, storage.PostgresOptions.TransactionSynchronisationTimeout);
			Assert.AreEqual(options.UseNativeDatabaseTransactions, storage.PostgresOptions.UseNativeDatabaseTransactions);
		}

        [Test]
        public void ShouldPassBackgroundProcessesToActiveServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = @"Host=localhost;Database=inactive;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = @"Host=localhost;Database=active;" });

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null, new Worker());

            Assert.IsEmpty(system.Hangfire.StartedServers.Single(x => x.storage.ConnectionString == @"Host=localhost;Database=inactive;")
                .backgroundProcesses);
            system.Hangfire.StartedServers
	            .Single(x => x.storage.ConnectionString == @"Host=localhost;Database=active;").backgroundProcesses
	            .Should().Not.Be.Empty();
        }

        [Test]
        public void ShouldGetGoalWorkerCountForTwoServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 20, ConnectionString = @"Host=localhost;Database=fakedb;"});
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 100, ConnectionString = @"Host=localhost;Database=fakedb;" });

            system.WorkerServerStarter.Start(new ConfigurationOptions(), null, (PostgreSqlStorageOptions)null);

            var actual = system.Hangfire.StartedServers.Select(x => x.options.WorkerCount).OrderBy(x => x).ToArray();
            Assert.AreEqual(new[] {20, 100}, actual);
        }
    }
}