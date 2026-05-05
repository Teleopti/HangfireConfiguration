using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class WorkerBalancerSamplePriorityTest
{
	[Test]
	public void ShouldPreferExactRecentOverExactOld()
	{
		var system = new SystemUnderTest();
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default"],
			Timestamp = "2026-05-05 06:00".Utc()
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default"],
			Timestamp = "2026-05-05 07:00".Utc()
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = ["default"],
			Timestamp = "2026-05-05 08:30".Utc()
		});
		system.Now("2026-05-05 09:00");
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration {Queues = ["default"], GoalWorkerCount = 12}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount.Should().Be(6);
	}

	[Test]
	public void ShouldPreferExactRecentOverOverlapRecent()
	{
		var system = new SystemUnderTest();
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = ["default"],
			Timestamp = "2026-05-05 08:30".Utc()
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default", "email"],
			Timestamp = "2026-05-05 08:30".Utc()
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default", "email"],
			Timestamp = "2026-05-05 08:40".Utc()
		});
		system.Now("2026-05-05 09:00");
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration {Queues = ["default"], GoalWorkerCount = 12}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount.Should().Be(6);
	}

	[Test]
	public void ShouldPreferExactOldOverOverlapRecent()
	{
		var system = new SystemUnderTest();
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = ["default"],
			Timestamp = "2026-05-05 06:00".Utc()
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default", "email"],
			Timestamp = "2026-05-05 08:30".Utc()
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default", "email"],
			Timestamp = "2026-05-05 08:40".Utc()
		});
		system.Now("2026-05-05 09:00");
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration {Queues = ["default"], GoalWorkerCount = 12}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount.Should().Be(6);
	}

	[Test]
	public void ShouldPreferOverlapRecentOverOverlapOld()
	{
		var system = new SystemUnderTest();
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default", "email"],
			Timestamp = "2026-05-05 06:00".Utc()
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default", "email"],
			Timestamp = "2026-05-05 07:00".Utc()
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = ["default", "email"],
			Timestamp = "2026-05-05 08:30".Utc()
		});
		system.Now("2026-05-05 09:00");
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration {Queues = ["default"], GoalWorkerCount = 12}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount.Should().Be(6);
	}

	[Test]
	public void ShouldUseOverlapOldWhenNothingElse()
	{
		var system = new SystemUnderTest();
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default", "email"],
			Timestamp = "2026-05-05 06:00".Utc()
		});
		system.Now("2026-05-05 09:00");
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration {Queues = ["default"], GoalWorkerCount = 12}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount.Should().Be(3);
	}

	[Test]
	public void ShouldUseExactOldWhenNoRecent()
	{
		var system = new SystemUnderTest();
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 4,
			Queues = ["default"],
			Timestamp = "2026-05-05 06:00".Utc()
		});
		system.Now("2026-05-05 09:00");
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration {Queues = ["default"], GoalWorkerCount = 12}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount.Should().Be(3);
	}
}
