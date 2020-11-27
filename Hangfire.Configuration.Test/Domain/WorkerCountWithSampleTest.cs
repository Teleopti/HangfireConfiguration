using System;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerCountWithSampleTest
    {
        [Fact]
        public void ShouldDetermineWorkersFromSample()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(10);
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 2});

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldDetermineWorkersFromSample2()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(10);
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 5});

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(10 / 5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldDetermineWorkersWithoutSample()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(10);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(10, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldApplyMinimumServerCount()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(10);
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 1});

            system.WorkerServerStarter.Start(
                new ConfigurationOptionsForTest
                {
                    MinimumServerCount = 2
                },
                null, null);

            Assert.Equal(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldDisableServerCountSampling()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(10);
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 10});
            system.Monitor.AnnounceServer("server", null);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                UseServerCountSampling = false
            }, null, null);

            Assert.Equal(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldCalculateWithoutServerCountFromServerRecycling()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(10);
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 3});
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 2});
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 2});

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldCalculateWithoutServerCountFromServerRecycling_2()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(10);
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 2});
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 2});
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 3});

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldCalculateWithEarliestSample()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(8);
            system.ServerCountSampleRepository.Has(new ServerCountSample
            {
                Count = 2,
                Timestamp = DateTime.Parse("2020-11-27 09:00")
            });
            system.ServerCountSampleRepository.Has(new ServerCountSample
            {
                Count = 4,
                Timestamp = DateTime.Parse("2020-11-27 08:00")
            });

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(8 / 4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldIgnoreSampleWhenServerCountIsZero()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.HasGoalWorkerCount(8);
            system.ServerCountSampleRepository.Has(new ServerCountSample {Count = 0});

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
    }
}