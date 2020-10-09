using System.Data.SqlClient;
using System.Linq;
using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerCountTest
    {
        [Fact]
        public void ShouldGetDefaultForFirstServer()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(
                new ConfigurationOptions
                {
                    AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "Hangfire"}.ToString(), 
                },
                null, null);

            Assert.Equal(10, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetGoalForFirstServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetGoalOnRestartOfSingleServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
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
        public void ShouldGetMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(101);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(100, system.Hangfire.StartedServers.Single().options.WorkerCount);
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
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "Hangfire"}.ToString(),
                DefaultGoalWorkerCount = 12
            }, null, null);

            Assert.Equal(12, system.Hangfire.StartedServers.Single().options.WorkerCount);
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
        public void ShouldUseMaximumGoalWorkerCount()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(202);

            system.WorkerServerStarter.Start(new ConfigurationOptions {MaximumGoalWorkerCount = 200}, null, null);

            Assert.Equal(200, system.Hangfire.StartedServers.Single().options.WorkerCount);
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