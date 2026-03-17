using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class WorkerBalancerWithGracefulShutdownTest
{
    [Test]
    public void ShouldCalculateWorkersOnStartOfSecondServer()
    {
        var system = new SystemUnderTest();
        system.HasGoalWorkerCount(8);
        system.Monitor.AnnounceServer("runningServer");

        system.BackgroundJobServerStarter.Start();

        Assert.AreEqual(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
    }
        
    [Test]
    public void ShouldCalculateWorkersOnStartOfThirdServer()
    {
        var system = new SystemUnderTest();
        system.HasGoalWorkerCount(9);
        system.Monitor.AnnounceServer("server1");
        system.Monitor.AnnounceServer("server2");

        system.BackgroundJobServerStarter.Start();

        Assert.AreEqual(3, system.Hangfire.StartedServers.Single().options.WorkerCount);
    }
        
    [Test]
    public void ShouldRoundWorkerCountUp()
    {
        var system = new SystemUnderTest();
        system.HasGoalWorkerCount(10);
        system.Monitor.AnnounceServer("server1");
        system.Monitor.AnnounceServer("server2");
        system.Monitor.AnnounceServer("server3");

        system.BackgroundJobServerStarter.Start();

        Assert.AreEqual(3, system.Hangfire.StartedServers.Single().options.WorkerCount);
    }
        
    [Test]
    public void ShouldCalculateBasedOnMaxOneHundredWhenStartingFourthServer()
    {
        var system = new SystemUnderTest();
        system.HasGoalWorkerCount(200);
        system.Monitor.AnnounceServer("server1");
        system.Monitor.AnnounceServer("server2");
        system.Monitor.AnnounceServer("server3");

        system.BackgroundJobServerStarter.Start();

        Assert.AreEqual(25, system.Hangfire.StartedServers.Single().options.WorkerCount);
    }

    [Test]
    public void ShouldCalculateGoalBasedOnMinimumKnownOnStartOfThirdServer()
    {
        var system = new SystemUnderTest();
        system.HasGoalWorkerCount(100);
        system.Monitor.AnnounceServer("server1");
        system.Monitor.AnnounceServer("server2");

        system.BackgroundJobServerStarter.Start(new ConfigurationOptionsForTest
        {
            MinimumServerCount = 4
        });

        Assert.AreEqual(25, system.Hangfire.StartedServers.Single().options.WorkerCount);
    }        
        
}