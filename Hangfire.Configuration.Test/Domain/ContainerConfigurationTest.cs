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
			Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 20 } }
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount
			.Should().Be(20);
	}

	[Test]
	public void ShouldUseMaxWorkersPerServerFromContainer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 200, MaxWorkersPerServer = 5 } }
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Single().options.WorkerCount
			.Should().Be.LessThanOrEqualTo(5);
	}

	[Test]
	public void ShouldPreserveContainersOnRoundTrip()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[]
			{
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					GoalWorkerCount = 15,
					MaxWorkersPerServer = 3,
					WorkerBalancerEnabled = true
				}
			}
		});

		var result = system.Configurations().Single();

		result.Containers.Single().Tag.Should().Be.EqualTo(DefaultContainerTag.Tag());
		result.Containers.Single().GoalWorkerCount.Should().Be.EqualTo(15);
		result.Containers.Single().MaxWorkersPerServer.Should().Be.EqualTo(3);
		result.Containers.Single().WorkerBalancerEnabled.Should().Be.EqualTo(true);
	}

	[Test]
	public void ShouldPreserveMultipleContainersOnRoundTrip()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[]
			{
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					GoalWorkerCount = 10
				},
				new ContainerConfiguration
				{
					Tag = "worker-b",
					Queues = new[] {"reports", "emails"},
					GoalWorkerCount = 5
				}
			}
		});

		var result = system.Configurations().Single();

		result.Containers.Length.Should().Be.EqualTo(2);
		result.Containers[0].Tag.Should().Be.EqualTo(DefaultContainerTag.Tag());
		result.Containers[0].GoalWorkerCount.Should().Be.EqualTo(10);
		result.Containers[1].Tag.Should().Be.EqualTo("worker-b");
		result.Containers[1].Queues.Should().Have.SameSequenceAs("reports", "emails");
		result.Containers[1].GoalWorkerCount.Should().Be.EqualTo(5);
	}

	[Test]
	public void ShouldPreserveOtherPropertiesWhenUsingContainers()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Name = "Production",
			ConnectionString = "Data Source=.",
			SchemaName = "MySchema",
			Active = true,
			Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 10 } }
		});

		var result = system.Configurations().Single();

		result.Name.Should().Be.EqualTo("Production");
		result.ConnectionString.Should().Be.EqualTo("Data Source=.");
		result.SchemaName.Should().Be.EqualTo("MySchema");
		result.Active.Should().Be.EqualTo(true);
		result.Containers.Single().GoalWorkerCount.Should().Be.EqualTo(10);
	}

	[Test]
	public void ShouldStartServersForMultipleConfigurations()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 10 } }
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 20 } }
		});

		system.StartBackgroundJobServers();

		var workerCounts = system.Hangfire.StartedServers
			.Select(x => x.options.WorkerCount)
			.OrderBy(x => x)
			.ToArray();
		workerCounts.Should().Have.SameSequenceAs(new[] {10, 20});
	}
}
