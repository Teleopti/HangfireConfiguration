using System;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class WorkerBalancerServerCountSamplingTest
{
	[Test]
	public void ShouldDetermineWorkersFromSample()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(10);
		system.KeyValueStore.Has(new ServerCountSample {Count = 2});

		system.WorkerServerStarter.Start();

		Assert.AreEqual(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldDetermineWorkersFromSample2()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(10);
		system.KeyValueStore.Has(new ServerCountSample {Count = 5});

		system.WorkerServerStarter.Start();

		Assert.AreEqual(10 / 5, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldDetermineWorkersWithoutSample()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(10);

		system.WorkerServerStarter.Start();

		Assert.AreEqual(10, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldApplyMinimumServerCount()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(10);
		system.KeyValueStore.Has(new ServerCountSample {Count = 1});

		system.UseOptions(new ConfigurationOptionsForTest
		{
			MinimumServerCount = 2
		});
		system.WorkerServerStarter.Start();

		Assert.AreEqual(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldDisableServerCountSampling()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(10);
		system.KeyValueStore.Has(new ServerCountSample {Count = 10});
		system.Monitor.AnnounceServer("server");

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			UseServerCountSampling = false,
		});

		Assert.AreEqual(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldCalculateWithoutServerCountFromServerRecycling()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(10);
		system.KeyValueStore.Has(new ServerCountSample {Count = 3});
		system.KeyValueStore.Has(new ServerCountSample {Count = 2});
		system.KeyValueStore.Has(new ServerCountSample {Count = 2});

		system.WorkerServerStarter.Start();

		Assert.AreEqual(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldCalculateWithoutServerCountFromServerRecycling_2()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(10);
		system.KeyValueStore.Has(new ServerCountSample {Count = 2});
		system.KeyValueStore.Has(new ServerCountSample {Count = 2});
		system.KeyValueStore.Has(new ServerCountSample {Count = 3});

		system.WorkerServerStarter.Start();

		Assert.AreEqual(10 / 2, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldCalculateWithEarliestSample()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(8);
		system.KeyValueStore.Has(new ServerCountSample
		{
			Count = 2,
			Timestamp = DateTime.Parse("2020-11-27 09:00")
		});
		system.KeyValueStore.Has(new ServerCountSample
		{
			Count = 4,
			Timestamp = DateTime.Parse("2020-11-27 08:00")
		});

		system.WorkerServerStarter.Start();

		Assert.AreEqual(8 / 4, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldIgnoreSampleWhenServerCountIsZero()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.HasGoalWorkerCount(8);
		system.KeyValueStore.Has(new ServerCountSample {Count = 0});

		system.WorkerServerStarter.Start();

		Assert.AreEqual(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}
}