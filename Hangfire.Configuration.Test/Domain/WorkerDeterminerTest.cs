using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerDeterminerTest
    {
        [Fact]
        public void ShouldGetDefaultGoalWorkerCount()
        {
            var target = new WorkerDeterminer(new Configuration(new FakeConfigurationRepository()), new FakeMonitoringApi());

            var workers = target.DetermineStartingServerWorkerCount();

            Assert.Equal(10, workers);
        }

        [Fact]
        public void ShouldGetGoalWorkerCountForFirstServer()
        {
            var repository = new FakeConfigurationRepository();
            repository.HasGoalWorkerCount(8);
            var target = new WorkerDeterminer(new Configuration(repository), new FakeMonitoringApi());

            var workers = target.DetermineStartingServerWorkerCount();

            Assert.Equal(8, workers);
        }

        [Fact]
        public void ShouldGetGoalWorkerCountOnRestartOfSingleServer()
        {
            var repository = new FakeConfigurationRepository();
            repository.HasGoalWorkerCount(8);
            var monitor = new FakeMonitoringApi();
            monitor.AnnounceServer("restartedServer", new ServerContext());
            var target = new WorkerDeterminer(new Configuration(repository), monitor);

            var workers = target.DetermineStartingServerWorkerCount();

            Assert.Equal(8, workers);
        }

        [Fact]
        public void ShouldDetermineHalfOfGoalForSecondServerAfterRestart()
        {
            var repository = new FakeConfigurationRepository();
            repository.HasGoalWorkerCount(8);
            var monitor = new FakeMonitoringApi();
            monitor.AnnounceServer("server1", new ServerContext());
            monitor.AnnounceServer("restartedServer", new ServerContext());
            var target = new WorkerDeterminer(new Configuration(repository), monitor);

            var workers = target.DetermineStartingServerWorkerCount();

            Assert.Equal(4, workers);
        }

        [Fact]
        public void ShouldRoundDeterminedWorkerCountUp()
        {
            var repository = new FakeConfigurationRepository();
            repository.HasGoalWorkerCount(10);
            var monitor = new FakeMonitoringApi();
            monitor.AnnounceServer("server1", new ServerContext());
            monitor.AnnounceServer("server2", new ServerContext());
            monitor.AnnounceServer("server3", new ServerContext());

            var target = new WorkerDeterminer(new Configuration(repository), monitor);

            var workers = target.DetermineStartingServerWorkerCount();

            Assert.Equal(4, workers);
        }

        [Fact]
        public void ShouldDetermineToOneIfWorkerGoalCountIsZero()
        {
            var repository = new FakeConfigurationRepository();
            repository.HasGoalWorkerCount(0);
            var target = new WorkerDeterminer(new Configuration(repository), new FakeMonitoringApi());

            var workers = target.DetermineStartingServerWorkerCount();

            Assert.Equal(1, workers);
        }

        [Fact]
        public void ShouldDetermineToOneIfWorkerGoalCountIsNegative()
        {
            var repository = new FakeConfigurationRepository();
            repository.HasGoalWorkerCount(-1);
            var target = new WorkerDeterminer(new Configuration(repository), new FakeMonitoringApi());

            var workers = target.DetermineStartingServerWorkerCount();

            Assert.Equal(1, workers);
        }

        [Fact]
        public void ShouldDetermineToMaxOneHundred()
        {
            var repository = new FakeConfigurationRepository();
            repository.HasGoalWorkerCount(101);
            var target = new WorkerDeterminer(new Configuration(repository), new FakeMonitoringApi());

            var workers = target.DetermineStartingServerWorkerCount();

            Assert.Equal(100, workers);
        }
    }
}