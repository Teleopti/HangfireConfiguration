using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerCountWithGracefulShutdownTest
    {
        [Fact]
        public void ShouldCalculateWorkersOnStartOfSecondServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("runningServer", new ServerContext());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldCalculateWorkersOnStartOfThirdServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(9);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(3, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldRoundWorkerCountUp()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());
            system.Monitor.AnnounceServer("server3", new ServerContext());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(3, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldCalculateBasedOnMaxOneHundredWhenStartingFourthServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(200);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());
            system.Monitor.AnnounceServer("server3", new ServerContext());

            system.WorkerServerStarter.Start(null, null, (PostgreSqlStorageOptions) null);

            Assert.Equal(25, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldCalculateGoalBasedOnMinimumKnownOnStartOfThirdServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(100);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 4,
                ConnectionString = ConnectionUtils.GetFakeConnectionString()
			}, null, (PostgreSqlStorageOptions)null);

            Assert.Equal(25, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }        
        
    }
}