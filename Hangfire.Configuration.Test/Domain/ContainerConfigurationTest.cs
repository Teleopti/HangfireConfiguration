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
			Containers = new[] {new ContainerConfiguration {GoalWorkerCount = 20}}
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount
			.Should().Be(20);
	}

	[Test]
	[Ignore("WIP")]
	public void ShouldOnlyStartDefaultTagWhenNoTagSpecified()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = new[] {"default", "reports"}
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[]
			{
				new ContainerConfiguration {Tag = DefaultContainerTag.Tag()},
				new ContainerConfiguration {Tag = "reports", Queues = new[] {"reports"}}
			}
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Contain("default");
		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Not.Contain("reports");
	}

	[Test]
	[Ignore("WIP")]
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
			Queues = new[] {"default", "reports"}
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[]
			{
				new ContainerConfiguration {Tag = DefaultContainerTag.Tag()},
				new ContainerConfiguration {Tag = "reports", Queues = new[] {"reports"}, GoalWorkerCount = 5}
			}
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Contain("default");
		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Not.Contain("reports");
	}

	[Test]
	[Ignore("WIP")]
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
			Queues = new[] {"default", "reports"}
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[]
			{
				new ContainerConfiguration {Tag = DefaultContainerTag.Tag()},
				new ContainerConfiguration {Tag = "reports", Queues = new[] {"reports"}, GoalWorkerCount = 5}
			}
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Not.Contain("default");
		system.Hangfire.StartedServers.Single().options.Queues
			.Should().Contain("reports");
	}

	[Test]
	[Ignore("WIP")]
	public void ShouldStartDefaultContainerWithConfiguredQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = new[] {"default", "email", "reports"}
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[]
			{
				new ContainerConfiguration {Tag = DefaultContainerTag.Tag(), Queues = new[] {"default", "email"}},
				new ContainerConfiguration {Tag = "reports", Queues = new[] {"reports"}}
			}
		});

		system.StartBackgroundJobServers();

		var defaultServer = system.Hangfire.StartedServers.Single();
		defaultServer.options.Queues.Should().Contain("default");
		defaultServer.options.Queues.Should().Contain("email");
		defaultServer.options.Queues.Should().Not.Contain("reports");
	}

	[Test]
	[Ignore("WIP")]
	public void ShouldPickUpNewQueueOnDefaultContainer()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = new[] {"default", "email", "reports", "notifications"}
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[]
			{
				new ContainerConfiguration {Tag = DefaultContainerTag.Tag(), Queues = new[] {"default", "email"}},
				new ContainerConfiguration {Tag = "reports", Queues = new[] {"reports"}}
			}
		});

		system.StartBackgroundJobServers();

		var defaultServer = system.Hangfire.StartedServers.Single();
		defaultServer.options.Queues.Should().Contain("default");
		defaultServer.options.Queues.Should().Contain("email");
		defaultServer.options.Queues.Should().Contain("notifications");
		defaultServer.options.Queues.Should().Not.Contain("reports");
	}
	
	[Test]
	[Ignore("WIP")]
	public void ShouldPickUpNewQueueOnDefaultContainerWithoutQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = new[] {"default", "email", "reports", "notifications"}
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[]
			{
				new ContainerConfiguration {Tag = DefaultContainerTag.Tag(), Queues = null},
				new ContainerConfiguration {Tag = "reports", Queues = new[] {"reports"}}
			}
		});

		system.StartBackgroundJobServers();

		var defaultServer = system.Hangfire.StartedServers.Single();
		defaultServer.options.Queues.Should().Contain("default");
		defaultServer.options.Queues.Should().Contain("email");
		defaultServer.options.Queues.Should().Contain("notifications");
		defaultServer.options.Queues.Should().Not.Contain("reports");
	}
}