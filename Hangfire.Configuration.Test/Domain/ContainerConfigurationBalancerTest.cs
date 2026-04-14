using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ContainerConfigurationBalancerTest
{
	[Test]
	public void ShouldGetFullGoalOnFirstStartup()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(), 
					Queues = ["default"], 
					GoalWorkerCount = 10
				}
			]
		});

		system.StartBackgroundJobServers();

		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(10);
		server.options.Queues.Should().Have.SameSequenceAs(["default"]);
	}

	[Test]
	public void ShouldGetFullGoalForNewContainer()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "reports"
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = ["default"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default"],
					GoalWorkerCount = 10
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"],
					GoalWorkerCount = 20
				}
			]
		});

		system.StartBackgroundJobServers();

		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(20);
		server.options.Queues.Should().Have.SameSequenceAs(["reports"]);
	}

	[Test]
	public void ShouldBalanceWithAnnouncedServerOnSameQueues()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "reports"
		});
		system.Monitor.AnnounceServer("reportsServer1", queues: ["reports"]);
		system.Monitor.AnnounceServer("defaultServer", queues: ["default"]);
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default"],
					GoalWorkerCount = 10
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"],
					GoalWorkerCount = 20
				}
			]
		});

		system.StartBackgroundJobServers();

		// 1 matching announced server + 1 starting = 2
		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(10);
		server.options.Queues.Should().Have.SameSequenceAs(["reports"]);
	}

	[Test]
	public void ShouldNotCountAnnouncedServerWithDifferentQueues()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "reports"
		});
		system.Monitor.AnnounceServer("defaultServer1", queues: ["default"]);
		system.Monitor.AnnounceServer("defaultServer2", queues: ["default"]);
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default"],
					GoalWorkerCount = 10
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"],
					GoalWorkerCount = 20
				}
			]
		});

		system.StartBackgroundJobServers();

		// no matching announced servers, only the starting one = 1
		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(20);
		server.options.Queues.Should().Have.SameSequenceAs(["reports"]);
	}

	[Test]
	public void ShouldBalanceWorkersForDefaultContainer()
	{
		var system = new SystemUnderTest();
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = ["default", "email"]
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 1,
			Queues = ["reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default", "email"],
					GoalWorkerCount = 10
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"],
					GoalWorkerCount = 50
				}
			]
		});

		system.StartBackgroundJobServers();

		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(5);
		server.options.Queues.Should().Have.SameSequenceAs(["default", "email"]);
	}

	[Test]
	public void ShouldBalanceWorkersForSpecificContainer()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "reports"
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 1,
			Queues = ["default", "email"]
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = ["reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default", "email"],
					GoalWorkerCount = 10
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"],
					GoalWorkerCount = 20
				}
			]
		});

		system.StartBackgroundJobServers();

		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(10);
		server.options.Queues.Should().Have.SameSequenceAs(["reports"]);
	}

	[Test]
	public void ShouldDisableBalancerForSpecificContainer()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "reports"
		});
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = ["reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default"],
					GoalWorkerCount = 10
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"],
					WorkerBalancerEnabled = false
				}
			]
		});

		system.StartBackgroundJobServers();

		var hangfireDefault = new BackgroundJobServerOptions().WorkerCount;
		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(hangfireDefault);
		server.options.Queues.Should().Have.SameSequenceAs(["reports"]);
	}

	[Test]
	public void ShouldCountOldSampleWithoutQueues()
	{
		var system = new SystemUnderTest();
		system.WithServerCountSample(new ServerCountSample
		{
			Count = 2,
			Queues = null
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default"],
					GoalWorkerCount = 10
				}
			]
		});

		system.StartBackgroundJobServers();

		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(5);
	}

	[Test]
	public void ShouldCountAnnouncedServerWithoutQueues()
	{
		var system = new SystemUnderTest();
		system.Monitor.AnnounceServer("oldServer");
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default"],
					GoalWorkerCount = 10
				}
			]
		});

		system.StartBackgroundJobServers();

		var server = system.Hangfire.StartedServers.Single();
		server.options.WorkerCount.Should().Be(5);
	}
}
