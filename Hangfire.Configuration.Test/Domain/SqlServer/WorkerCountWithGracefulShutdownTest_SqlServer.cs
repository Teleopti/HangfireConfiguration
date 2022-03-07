using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.Server;
using Hangfire.SqlServer;
using Xunit;

namespace Hangfire.Configuration.Test.Domain.SqlServer
{
    public class WorkerCountWithGracefulShutdownTest
    {
        [Fact]
        public void ShouldCalculateWorkersOnStartOfSecondServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("runningServer");

            system.WorkerServerStarter.Start(null, null, (SqlServerStorageOptions)null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldCalculateWorkersOnStartOfThirdServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(9);
            system.Monitor.AnnounceServer("server1");
            system.Monitor.AnnounceServer("server2");

            system.WorkerServerStarter.Start(null, null, (SqlServerStorageOptions)null);

            Assert.Equal(3, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldRoundWorkerCountUp()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(10);
            system.Monitor.AnnounceServer("server1");
            system.Monitor.AnnounceServer("server2");
            system.Monitor.AnnounceServer("server3");

            system.WorkerServerStarter.Start(null, null, (SqlServerStorageOptions)null);

            Assert.Equal(3, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldCalculateBasedOnMaxOneHundredWhenStartingFourthServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(200);
            system.Monitor.AnnounceServer("server1");
            system.Monitor.AnnounceServer("server2");
            system.Monitor.AnnounceServer("server3");

            system.WorkerServerStarter.Start(null, null, (SqlServerStorageOptions)null);

            Assert.Equal(25, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldCalculateGoalBasedOnMinimumKnownOnStartOfThirdServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(100);
            system.Monitor.AnnounceServer("server1");
            system.Monitor.AnnounceServer("server2");

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 4,
                ConnectionString = @"Data Source=.;Initial Catalog=fakedb;"
			}, null, (SqlServerStorageOptions)null);

            Assert.Equal(25, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }        
        
    }
}