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
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfGoalForFirstServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfGoalOnRestartOfSingleServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfGoalForSecondServerAfterRestart()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);
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
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetOneIfGoalIsZero()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(0);
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetOneIfGoalIsNegative()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(-1);
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(101);
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(50, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldGetHalfOfMaxOneHundredWhenTwoServers()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(101);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());
            var configurationOptions = new ConfigurationOptions() {DefaultHangfireConnectionString = "connection", DefaultSchemaName = "schema"};

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(50, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }


        [Fact]
        public void ShouldUseDefaultGoalWorkerCount()
        {
            var system = new SystemUnderTest();
            var configurationOptions = new ConfigurationOptions()
            {
                DefaultHangfireConnectionString = "connection",
                DefaultSchemaName = "schema",
                DefaultGoalWorkerCount = 12
            };

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMinimumWorkerCount()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(0);
            var configurationOptions = new ConfigurationOptions()
            {
                DefaultHangfireConnectionString = "connection", 
                DefaultSchemaName = "schema", 
                MinimumWorkerCount = 2
            };

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseHalfOfMaximumGoalWorkerCount()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(202);
            var configurationOptions = new ConfigurationOptions()
            {
                DefaultHangfireConnectionString = "connection", 
                DefaultSchemaName = "schema", 
                MaximumGoalWorkerCount = 200
            };

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(100, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMinimumServerCount()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(15);
            var configurationOptions = new ConfigurationOptions()
            {
                DefaultHangfireConnectionString = "connection", 
                DefaultSchemaName = "schema", 
                MinimumServers = 3
            };

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Fact]
        public void ShouldUseMinimumWorkerCount2()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(7);
            var configurationOptions = new ConfigurationOptions()
            {
                DefaultHangfireConnectionString = "connection",
                DefaultSchemaName = "schema",
                MinimumWorkerCount = 6
            };

            system.ServerStarter.StartServers(configurationOptions, null, null);

            Assert.Equal(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }


    }
}