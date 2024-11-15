using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.SqlServer;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class StartWorkerServersTest
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

            system.UseServerOptions(serverOptions);
            system.WorkerServerStarter.Start();

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

            system.UseServerOptions(new BackgroundJobServerOptions {ServerName = "server!"});
            system.WorkerServerStarter.Start();

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

            system.WorkerServerStarter.Start(new []{backgroundProcess});

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

            system.WorkerServerStarter.Start(new Worker());

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
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString =  @"Data Source=.;Initial Catalog=fakedb;" });

            system.WorkerServerStarter.Start();

            Assert.AreEqual( @"Data Source=.;Initial Catalog=fakedb;", system.Hangfire.StartedServers.Single().storage.ConnectionString);
        }

        [Test]
        public void ShouldConstructSqlHangfireStorageWithOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration { ConnectionString = "Data Source=."});

            system.UseStorageOptions(new SqlServerStorageOptions { PrepareSchemaIfNecessary = false});
            system.WorkerServerStarter.Start();

            Assert.False(system.Hangfire.StartedServers.Single().storage.SqlServerOptions.PrepareSchemaIfNecessary);
        }

        [Test]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = "SchemaName", 
	            ConnectionString = "Data Source=."
            });

            system.UseStorageOptions(new SqlServerStorageOptions { SchemaName = "Ignored" });
            system.WorkerServerStarter.Start();

            Assert.AreEqual("SchemaName", system.Hangfire.StartedServers.Single().storage.SqlServerOptions.SchemaName);
        }

        [Test]
        public void ShouldUseSchemaNameFromConfiguration2()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = "SchemaName", 
	            ConnectionString = "Data Source=."
            });

            system.WorkerServerStarter.Start();

            Assert.AreEqual("SchemaName", system.Hangfire.StartedServers.Single().storage.SqlServerOptions.SchemaName);
        }

        [Test]
        public void ShouldUseSchemaNameFromConfigurationOfTwoServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = "SchemaName1",
	            ConnectionString = "Data Source=."
            });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = "SchemaName2",
	            ConnectionString = "Data Source=."
            });

            system.WorkerServerStarter.Start();

            Assert.AreEqual("SchemaName1", system.Hangfire.StartedServers.First().storage.SqlServerOptions.SchemaName);
            Assert.AreEqual("SchemaName2", system.Hangfire.StartedServers.Last().storage.SqlServerOptions.SchemaName);
        }

        [Test]
        public void ShouldUseDefaultSchemaName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            SchemaName = null, 
	            ConnectionString =  @"Data Source=.;Initial Catalog=fakedb;"
            });

            system.WorkerServerStarter.Start();

            Assert.AreEqual(DefaultSchemaName.SqlServer(),
                system.Hangfire.StartedServers.Single().storage.SqlServerOptions.SchemaName);
        }

        [Test]
        public void ShouldUseDefaultSchemaNameWhenEmpty()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {SchemaName = "", ConnectionString =  @"Data Source=.;Initial Catalog=fakedb;" });

            system.WorkerServerStarter.Start();

            Assert.AreEqual(DefaultSchemaName.SqlServer(),
                system.Hangfire.StartedServers.Single().storage.SqlServerOptions.SchemaName);
        }

        [Test]
        public void ShouldPassStorageOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            ConnectionString = @"Data Source=."
            });
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
            };
            system.UseStorageOptions(options);
            
			system.WorkerServerStarter.Start();

			var storage = system.Hangfire.StartedServers.Single().storage;
			Assert.AreEqual(options.QueuePollInterval, storage.SqlServerOptions.QueuePollInterval);
			Assert.AreEqual(options.SlidingInvisibilityTimeout, storage.SqlServerOptions.SlidingInvisibilityTimeout);
			Assert.AreEqual(options.JobExpirationCheckInterval, storage.SqlServerOptions.JobExpirationCheckInterval);
			Assert.AreEqual(options.CountersAggregateInterval, storage.SqlServerOptions.CountersAggregateInterval);
			Assert.AreEqual(options.PrepareSchemaIfNecessary, storage.SqlServerOptions.PrepareSchemaIfNecessary);
			Assert.AreEqual(options.DashboardJobListLimit, storage.SqlServerOptions.DashboardJobListLimit);
			Assert.AreEqual(options.TransactionTimeout, storage.SqlServerOptions.TransactionTimeout);
			Assert.AreEqual(options.DisableGlobalLocks, storage.SqlServerOptions.DisableGlobalLocks);
		}

        [Test]
        public void ShouldPassDefaultStorageOptionsToHangfire()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            ConnectionString = @"Data Source=."
            });

            system.WorkerServerStarter.Start();

            var options = new SqlServerStorageOptions();
            var storage = system.Hangfire.StartedServers.Single().storage;
            Assert.AreEqual(options.QueuePollInterval, storage.SqlServerOptions.QueuePollInterval);
            Assert.AreEqual(options.SlidingInvisibilityTimeout, storage.SqlServerOptions.SlidingInvisibilityTimeout);
            Assert.AreEqual(options.JobExpirationCheckInterval, storage.SqlServerOptions.JobExpirationCheckInterval);
            Assert.AreEqual(options.CountersAggregateInterval, storage.SqlServerOptions.CountersAggregateInterval);
            Assert.AreEqual(options.PrepareSchemaIfNecessary, storage.SqlServerOptions.PrepareSchemaIfNecessary);
            Assert.AreEqual(options.DashboardJobListLimit, storage.SqlServerOptions.DashboardJobListLimit);
            Assert.AreEqual(options.TransactionTimeout, storage.SqlServerOptions.TransactionTimeout);
            Assert.AreEqual(options.DisableGlobalLocks, storage.SqlServerOptions.DisableGlobalLocks);
		}

        [Test]
        public void ShouldPassBackgroundProcessesToActiveServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = @"Data Source=.;Initial Catalog=inactive;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = @"Data Source=.;Initial Catalog=active;" });

            system.WorkerServerStarter.Start(new Worker());

            Assert.IsEmpty(system.Hangfire.StartedServers.Single(x => x.storage.ConnectionString == @"Data Source=.;Initial Catalog=inactive;")
				.backgroundProcesses);
            system.Hangfire.StartedServers
	            .Single(x => x.storage.ConnectionString == @"Data Source=.;Initial Catalog=active;").backgroundProcesses
	            .Should().Not.Be.Empty();
        }

        [Test]
        public void ShouldGetGoalWorkerCountForTwoServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 20});
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 100});

            system.WorkerServerStarter.Start(new ConfigurationOptions { ConnectionString = @"Data Source=.;Initial Catalog=fakedb;" });

            var actual = system.Hangfire.StartedServers.Select(x => x.options.WorkerCount).OrderBy(x => x).ToArray();
            Assert.AreEqual(new[] {20, 100}, actual);
        }
        
        [Test]
        public void ShouldUseSchemaNameFromConfigurationOfTwoServersWithOptions()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        SchemaName = "schema1",
		        ConnectionString = "Data Source=."
	        });
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        SchemaName = "schema2",
		        ConnectionString = "Data Source=."
	        });

	        system.UseStorageOptions(new SqlServerStorageOptions{CommandTimeout = TimeSpan.FromMinutes(1)});
	        system.WorkerServerStarter.Start();

	        var options1 = system.Hangfire.StartedServers.First().storage.SqlServerOptions;
	        var options2 = system.Hangfire.StartedServers.Last().storage.SqlServerOptions;
	        options1.SchemaName.Should().Be.EqualTo("schema1");
	        options1.CommandTimeout.Should().Be.EqualTo(TimeSpan.FromMinutes(1));
	        options2.SchemaName.Should().Be.EqualTo("schema2");
	        options2.CommandTimeout.Should().Be.EqualTo(TimeSpan.FromMinutes(1));
        }
    }
}