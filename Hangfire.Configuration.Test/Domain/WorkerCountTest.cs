using System.Linq;
using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerCountTest
    {
        [Fact]
        public void ShouldGetHalfOfDefaultForFirstServer()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(
                new ConfigurationOptions {AutoUpdatedHangfireConnectionString = "connection", AutoUpdatedHangfireSchemaName = "schema"},
                null, null);

            Assert.Equal(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfGoalForFirstServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfGoalOnRestartOfSingleServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfGoalForSecondServerAfterRestart()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldRoundWorkerCountUp()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(10);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());
            system.Monitor.AnnounceServer("server3", new ServerContext());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetOneIfGoalIsZero()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(0);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetOneIfGoalIsNegative()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(-1);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(101);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(50, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfMaxOneHundredWhenTwoServers()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(101);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(50, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }


        [Fact]
        public void ShouldUseDefaultGoalWorkerCount()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
                AutoUpdatedHangfireConnectionString = "connection",
                AutoUpdatedHangfireSchemaName = "schema",
                DefaultGoalWorkerCount = 12
            }, null, null);

            Assert.Equal(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMinimumWorkerCount()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(0);

            system.WorkerServerStarter.Start(new ConfigurationOptions {MinimumWorkerCount = 2}, null, null);

            Assert.Equal(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseHalfOfMaximumGoalWorkerCount()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(202);

            system.WorkerServerStarter.Start(new ConfigurationOptions {MaximumGoalWorkerCount = 200}, null, null);

            Assert.Equal(100, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMinimumServerCount()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(15);

            system.WorkerServerStarter.Start(new ConfigurationOptions {MinimumServers = 3}, null, null);

            Assert.Equal(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMinimumWorkerCount2()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(7);

            system.WorkerServerStarter.Start(new ConfigurationOptions {MinimumWorkerCount = 6}, null, null);

            Assert.Equal(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetGoalForFirstServerWhenMinimumServersIsZero()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(
                new ConfigurationOptions {MinimumServers = 0},
                null, null);

            Assert.Equal(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldDisableWorkerDeterminer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(
                new ConfigurationOptions {UseWorkerDeterminer = false},
                new BackgroundJobServerOptions {WorkerCount = 52},
                null
            );

            Assert.Equal(52, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
    }
}