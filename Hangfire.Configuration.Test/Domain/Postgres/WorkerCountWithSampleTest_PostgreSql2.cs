using System;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.PostgreSql;
using Xunit;

namespace Hangfire.Configuration.Test.Domain.Postgres
{
    public class WorkerCountWithSampleTest
    {
        [Fact]
        public void ShouldDetermineWorkersFromSample()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);
            system.KeyValueStore.Has(new ServerCountSample {Count = 2});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions)null);

            Assert.Equal(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldDetermineWorkersFromSample2()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);
            system.KeyValueStore.Has(new ServerCountSample {Count = 5});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(10 / 5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldDetermineWorkersWithoutSample()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(10, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldApplyMinimumServerCount()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);
            system.KeyValueStore.Has(new ServerCountSample {Count = 1});

            system.Options.UseOptions(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 2,
                ConnectionString = @"Host=localhost;Database=fakedb;"
			});
            system.WorkerServerStarter.Start();

            Assert.Equal(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldDisableServerCountSampling()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);
            system.KeyValueStore.Has(new ServerCountSample {Count = 10});
            system.Monitor.AnnounceServer("server", null);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                UseServerCountSampling = false,
                ConnectionString = @"Host=localhost;Database=fakedb;"
			}, null, (PostgreSqlStorageOptions)null);

            Assert.Equal(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldCalculateWithoutServerCountFromServerRecycling()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);
            system.KeyValueStore.Has(new ServerCountSample {Count = 3});
            system.KeyValueStore.Has(new ServerCountSample {Count = 2});
            system.KeyValueStore.Has(new ServerCountSample {Count = 2});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldCalculateWithoutServerCountFromServerRecycling_2()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);
            system.KeyValueStore.Has(new ServerCountSample {Count = 2});
            system.KeyValueStore.Has(new ServerCountSample {Count = 2});
            system.KeyValueStore.Has(new ServerCountSample {Count = 3});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldCalculateWithEarliestSample()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);
            system.KeyValueStore.Has(new ServerCountSample
            {
                Count = 2,
                Timestamp = DateTime.Parse("2020-11-27 09:00")
            });
            system.KeyValueStore.Has(new ServerCountSample
            {
                Count = 4,
                Timestamp = DateTime.Parse("2020-11-27 08:00")
            });

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(8 / 4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldIgnoreSampleWhenServerCountIsZero()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);
            system.KeyValueStore.Has(new ServerCountSample {Count = 0});

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
    }
}