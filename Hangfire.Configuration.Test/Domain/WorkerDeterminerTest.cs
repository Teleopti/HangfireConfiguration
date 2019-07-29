using Hangfire.Server;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerDeterminerTest
    {
        [Fact]
        public void ShouldGetHalfOfDefaultForFirstServer()
        {
            var system = new SystemUnderTest();

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(5, workers);
        }

        [Fact]
        public void ShouldGetHalfOfGoalForFirstServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(4, workers);
        }

        [Fact]
        public void ShouldGetHalfOfGoalOnRestartOfSingleServer()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("restartedServer", new ServerContext());

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(4, workers);
        }

        [Fact]
        public void ShouldGetHalfOfGoalForSecondServerAfterRestart()
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
        public void ShouldGetOneIfGoalIsZero()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(0);

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(1, workers);
        }

        [Fact]
        public void ShouldGetOneIfGoalIsNegative()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(-1);

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(1, workers);
        }

        [Fact]
        public void ShouldGetHalfOfMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(101);

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(50, workers);
        }
        
        [Fact]
        public void ShouldGetHalfOfMaxOneHundredWhenTwoServers()
        {
            var system = new SystemUnderTest();
            system.Repository.HasGoalWorkerCount(101);
            system.Monitor.AnnounceServer("server1", new ServerContext());
            system.Monitor.AnnounceServer("server2", new ServerContext());

            var workers = system.Determiner.DetermineStartingServerWorkerCount();

            Assert.Equal(50, workers);
        }
    }
}