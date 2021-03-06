using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
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
            system.ConfigurationStorage.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetOneIfGoalIsZero()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(0);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetOneIfGoalIsNegative()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(-1);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(101);

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Equal(100, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseDefaultGoalWorkerCount()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
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
            system.ConfigurationStorage.HasGoalWorkerCount(0);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumWorkerCount = 2
            }, null, null);

            Assert.Equal(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMaximumGoalWorkerCount()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(202);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MaximumGoalWorkerCount = 200
            }, null, null);

            Assert.Equal(200, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMinimumServerCount()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(15);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 3
            }, null, null);

            Assert.Equal(15 / 3, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMinimumWorkerCountWithMinimumKnownServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(7);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 2,
                MinimumWorkerCount = 6
            }, null, null);

            Assert.Equal(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetGoalForFirstServerWhenMinimumKnownServersIsZero()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(
                new ConfigurationOptionsForTest
                {
                    MinimumServerCount = 0
                },
                null, null);

            Assert.Equal(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldDisableWorkerDeterminer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(
                new ConfigurationOptions {UseWorkerDeterminer = false},
                new BackgroundJobServerOptions {WorkerCount = 52},
                null
            );

            Assert.Equal(52, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldGetHalfOfDefaultForFirstServer()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(
                new ConfigurationOptionsForTest
                {
                    MinimumServerCount = 2,
                    AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "Hangfire"}.ToString(), 
                },
                null, null);

            Assert.Equal(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfGoalForFirstServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 2
            }, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfGoalOnRestartOfSingleServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 2
            }, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldGetHalfOfMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(101);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 2
            }, null, null);

            Assert.Equal(50, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseDefaultGoalWorkerCountWithMinimumKnownServers()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "Hangfire"}.ToString(),
                DefaultGoalWorkerCount = 12,
                MinimumServerCount = 2
            }, null, null);

            Assert.Equal(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
    }
}