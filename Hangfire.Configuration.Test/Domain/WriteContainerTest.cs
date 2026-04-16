using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class WriteContainerTest
{
	[Test]
	public void ShouldWriteContainer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration { Id = 1 });

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 1,
			WorkerBalancerEnabled = true,
			Workers = 10,
			MaxWorkersPerServer = 5
		});

		var container = system.Configurations().Single().Containers.Single();
		container.WorkerBalancerEnabled.Should().Be(true);
		container.GoalWorkerCount.Should().Be(10);
		container.MaxWorkersPerServer.Should().Be(5);
	}

	[Test]
	public void ShouldWriteContainerWithNullWorkerCounts()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration
				{
					GoalWorkerCount = 10,
					MaxWorkersPerServer = 5
				}
			}
		});

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 1,
			WorkerBalancerEnabled = true,
			Workers = null,
			MaxWorkersPerServer = null
		});

		var container = system.Configurations().Single().Containers.Single();
		container.GoalWorkerCount.Should().Be(null);
		container.MaxWorkersPerServer.Should().Be(null);
	}

	[Test]
	public void ShouldWriteContainerForSpecificConfiguration()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration { Id = 1 });
		system.WithConfiguration(new StoredConfiguration { Id = 2 });

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 2,
			WorkerBalancerEnabled = false,
			Workers = 20,
			MaxWorkersPerServer = 3
		});

		var container = system.Configurations().Single(x => x.Id == 2).Containers.Single();
		container.WorkerBalancerEnabled.Should().Be(false);
		container.GoalWorkerCount.Should().Be(20);
		container.MaxWorkersPerServer.Should().Be(3);
	}

	[Test]
	public void ShouldDisableWorkerBalancer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration
				{
					WorkerBalancerEnabled = true
				}
			}
		});

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 1,
			WorkerBalancerEnabled = false
		});

		system.Configurations().Single().Containers.Single()
			.WorkerBalancerEnabled.Should().Be(false);
	}

	[Test]
	public void ShouldNotAffectOtherConfigurations()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration
				{
					GoalWorkerCount = 5,
					WorkerBalancerEnabled = true
				}
			}
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 2,
			Containers = new[]
			{
				new ContainerConfiguration
				{
					GoalWorkerCount = 7,
					WorkerBalancerEnabled = true
				}
			}
		});

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 2,
			WorkerBalancerEnabled = false,
			Workers = 20
		});

		system.Configurations().Single(x => x.Id == 1).Containers.Single()
			.GoalWorkerCount.Should().Be(5);
		system.Configurations().Single(x => x.Id == 1).Containers.Single()
			.WorkerBalancerEnabled.Should().Be(true);
	}
}
