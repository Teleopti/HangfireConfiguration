using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerDeterminerTest
    {
        [Fact]
        public void ShouldGetDefaultGoalWorkerCount()
        {
            var system = new SystemUnderTest();

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(10, workers);
        }

        [Fact]
        public void ShouldGetGoalWorkerCountForFirstServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(8, workers);
        }

        [Fact]
        public void ShouldGetGoalWorkerCountOnRestartOfSingleServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(8, workers);
        }

        [Fact]
        public void ShouldDetermineHalfOfGoalForSecondServerAfterRestart()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(4, workers);
        }

        [Fact]
        public void ShouldRoundDeterminedWorkerCountUp()
        {
            var system = new SystemUnderTest(); 
            system.Repository.HasGoalWorkerCount(10);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());
            system.Monitor.AnnounceServer("server3", new ServerContext());
            
            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(4, workers);
        }

        [Fact]
        public void ShouldDetermineToOneIfWorkerGoalCountIsZero()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(0);

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(1, workers);
        }

        [Fact]
        public void ShouldDetermineToOneIfWorkerGoalCountIsNegative()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(-1);

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(1, workers);
        }

        [Fact]
        public void ShouldDetermineToMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(101);

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(100, workers);
        }
    }
}