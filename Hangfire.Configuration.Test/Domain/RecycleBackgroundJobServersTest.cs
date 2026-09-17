using System.Linq;
using Hangfire.Server;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class RecycleBackgroundJobServersTest
{
	// the recycle interval is 23 hours, plus up to an hour of jitter,
	// so tests step to before 23 hours or past 24 hours to be deterministic

	private static SystemUnderTest systemWithRecycling()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest {RecycleBackgroundJobServers = true});
		system.HasGoalWorkerCount(8);
		return system;
	}

	private static BackgroundJobServerRecycler start(SystemUnderTest system) =>
		(BackgroundJobServerRecycler) system.BackgroundJobServerStarter.Start();

	[Test]
	public void ShouldNotRecycleWhenNotEnabled()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(8);

		var lifetime = system.BackgroundJobServerStarter.Start();

		Assert.IsNotInstanceOf<BackgroundJobServerRecycler>(lifetime);
	}

	[Test]
	public void ShouldRecycleAfterInterval()
	{
		var system = systemWithRecycling();
		var recycler = start(system);

		system.Now("2020-12-02 09:00");
		recycler.Tick();

		Assert.AreEqual(2, system.Hangfire.StartedServers.Count);
	}

	[Test]
	public void ShouldNotRecycleBeforeInterval()
	{
		var system = systemWithRecycling();
		var recycler = start(system);

		system.Now("2020-12-02 07:59");
		recycler.Tick();

		Assert.AreEqual(1, system.Hangfire.StartedServers.Count);
	}

	[Test]
	public void ShouldRecycleAgainAfterInterval()
	{
		var system = systemWithRecycling();
		var recycler = start(system);
		system.Now("2020-12-02 09:00");
		recycler.Tick();

		system.Now("2020-12-03 09:00");
		recycler.Tick();

		Assert.AreEqual(3, system.Hangfire.StartedServers.Count);
	}

	[Test]
	public void ShouldStopAndDisposeServerWhenRecycling()
	{
		var system = systemWithRecycling();
		var recycler = start(system);
		var recycled = system.Hangfire.StartedServers.Single().server;

		system.Now("2020-12-02 09:00");
		recycler.Tick();

		Assert.IsTrue(recycled.StopSent);
		Assert.IsTrue(recycled.Disposed);
		Assert.IsFalse(system.Hangfire.StartedServers.Last().server.Disposed);
	}

	[Test]
	public void ShouldCalculateWorkerCountWhenRecycling()
	{
		var system = systemWithRecycling();
		var recycler = start(system);
		Assert.AreEqual(8, system.Hangfire.StartedServers.Single().options.WorkerCount);

		system.WithAnnouncedServer("server1");
		system.WithAnnouncedServer("server2");
		system.WithAnnouncedServer("server3");
		system.Now("2020-12-02 09:00");
		recycler.Tick();

		Assert.AreEqual(2, system.Hangfire.StartedServers.Last().options.WorkerCount);
	}

	[Test]
	public void ShouldStartBackgroundProcessesWhenRecycling()
	{
		var system = systemWithRecycling();
		var backgroundProcess = new Worker();
		var recycler = (BackgroundJobServerRecycler) system.BackgroundJobServerStarter.Start(backgroundProcess);

		system.Now("2020-12-02 09:00");
		recycler.Tick();

		Assert.Contains(backgroundProcess, system.Hangfire.StartedServers.Last().backgroundProcesses);
	}

	[Test]
	public void ShouldNotRecycleWhenStopped()
	{
		var system = systemWithRecycling();
		var recycler = start(system);
		recycler.SendStop();

		system.Now("2020-12-02 09:00");
		recycler.Tick();

		Assert.AreEqual(1, system.Hangfire.StartedServers.Count);
	}

	[Test]
	public void ShouldNotRecycleWhenDisposed()
	{
		var system = systemWithRecycling();
		var recycler = start(system);
		recycler.Dispose();

		system.Now("2020-12-02 09:00");
		recycler.Tick();

		Assert.AreEqual(1, system.Hangfire.StartedServers.Count);
	}

	[Test]
	public void ShouldDisposeServerWhenDisposed()
	{
		var system = systemWithRecycling();
		var recycler = start(system);

		recycler.Dispose();

		Assert.IsTrue(system.Hangfire.StartedServers.Single().server.Disposed);
	}

	[Test]
	public void ShouldDisposeServerWhenDisposedAfterStopped()
	{
		var system = systemWithRecycling();
		var recycler = start(system);

		recycler.SendStop();
		recycler.Dispose();

		Assert.IsTrue(system.Hangfire.StartedServers.Single().server.Disposed);
	}
}
