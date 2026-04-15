using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ContainerConfigurationTest
{
	[Test]
	public void ShouldStartServerWithDefaultContainer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers
			.Should().Not.Be.Empty();
	}

	[Test]
	public void ShouldUseGoalWorkerCountFromContainer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = [new ContainerConfiguration {GoalWorkerCount = 20}]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount
			.Should().Be(20);
	}

	[Test]
	public void ShouldOnlyStartDefaultTagWhenNoTagSpecified()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag()
				},
				new ContainerConfiguration
				{
					Tag = "reports", 
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Contain("default");
		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Not.Contain("reports");
	}

	[Test]
	public void ShouldOnlyStartDefaultTagWhenDefaultTagSpecified()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = DefaultContainerTag.Tag()
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag()
				},
				new ContainerConfiguration
				{
					Tag = "reports", 
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Contain("default");
		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Not.Contain("reports");
	}

	[Test]
	public void ShouldOnlyStartDefaultTagWhenEmptyTagSpecified()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = ""
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag()
				},
				new ContainerConfiguration
				{
					Tag = "reports", 
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Contain("default");
		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Not.Contain("reports");
	}

	[Test]
	public void ShouldStartSpecificTagWhenTagSpecified()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "reports"
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag()
				},
				new ContainerConfiguration
				{
					Tag = "reports", 
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Not.Contain("default");
		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Contain("reports");
	}

	[Test]
	public void ShouldStartDefaultContainerWithConfiguredQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "email", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(), 
					Queues = ["default", "email"]
				},
				new ContainerConfiguration
				{
					Tag = "reports", 
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		var defaultServer = system.Hangfire.StartedServers.Single();
		defaultServer.options.Queues.Should().Contain("default");
		defaultServer.options.Queues.Should().Contain("email");
		defaultServer.options.Queues.Should().Not.Contain("reports");
	}

	[Test]
	public void ShouldPickUpNewQueueOnDefaultContainer()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "email", "reports", "notifications"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(), 
					Queues = ["default", "email"]
				},
				new ContainerConfiguration
				{
					Tag = "reports", 
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		var defaultServer = system.Hangfire.StartedServers.Single();
		defaultServer.options.Queues.Should().Contain("default");
		defaultServer.options.Queues.Should().Contain("email");
		defaultServer.options.Queues.Should().Contain("notifications");
		defaultServer.options.Queues.Should().Not.Contain("reports");
	}

	[Test]
	public void ShouldPickUpNewQueueOnDefaultContainerWithoutQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "email", "reports", "notifications"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(), 
					Queues = null
				},
				new ContainerConfiguration
				{
					Tag = "reports", 
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		var defaultServer = system.Hangfire.StartedServers.Single();
		defaultServer.options.Queues.Should().Contain("default");
		defaultServer.options.Queues.Should().Contain("email");
		defaultServer.options.Queues.Should().Contain("notifications");
		defaultServer.options.Queues.Should().Not.Contain("reports");
	}

	[Test]
	public void ShouldNotClaimQueuesFromContainerWithoutQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["default"]
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = null
				}
			]
		});

		system.StartBackgroundJobServers();

		var defaultServer = system.Hangfire.StartedServers.Single();
		defaultServer.options.Queues.Should().Contain("default");
		defaultServer.options.Queues.Should().Contain("reports");
	}

	[Test]
	public void ShouldKeepDefaultQueuesForSpecificContainerWithoutQueues()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "reports"
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag()
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = null
				}
			]
		});

		system.StartBackgroundJobServers();

		var server = system.Hangfire.StartedServers.Single();
		server.options.Queues.Should().Contain("default");
		server.options.Queues.Should().Contain("reports");
	}

	[Test]
	public void ShouldNotStartWhenContainerTagDoesNotExist()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "nonexistent"
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag()
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers
			.Should().Be.Empty();
	}

	[Test]
	[Ignore("WIP")]
	public void ShouldPreserveServerOptionsQueueOrderForSpecificContainer()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "reports"
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["default", "alpha", "beta"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag()
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["beta", "alpha"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Have.SameSequenceAs("alpha", "beta");
	}

	[Test]
	[Ignore("WIP")]
	public void ShouldPreserveServerOptionsQueueOrderForDefaultContainerWithUnclaimed()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["z-queue", "a-queue", "m-queue", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["m-queue", "a-queue"]
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Have.SameSequenceAs("z-queue", "a-queue", "m-queue");
	}

	[Test]
	[Ignore("WIP")]
	public void ShouldPreserveServerOptionsQueueOrderForDefaultContainerWithoutQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["z-queue", "a-queue", "m-queue", "reports"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = null
				},
				new ContainerConfiguration
				{
					Tag = "reports",
					Queues = ["reports"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Have.SameSequenceAs("z-queue", "a-queue", "m-queue");
	}

	[Test]
	[Ignore("WIP")]
	public void ShouldUseServerOptionsQueueOrderNotContainerOrder()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = new ConfigurationOptionsForTest().ConnectionString,
			ContainerTag = "worker"
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["alpha", "beta", "gamma"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag()
				},
				new ContainerConfiguration
				{
					Tag = "worker",
					Queues = ["gamma", "alpha", "beta"]
				}
			]
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Have.SameSequenceAs("alpha", "beta", "gamma");
	}
}