using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ContainerManagementTest
{
	[Test]
	public void ShouldAddContainer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Id = 1});

		system.ConfigurationApi().AddContainer(1, "my-tag");

		var containers = system.Configurations().Single().Containers;
		containers.Length.Should().Be(2);
		containers[1].Tag.Should().Be("my-tag");
	}

	[Test]
	public void ShouldRemoveContainer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration {Tag = "default"},
				new ContainerConfiguration {Tag = "secondary"}
			}
		});

		system.ConfigurationApi().RemoveContainer(1, 1);

		var containers = system.Configurations().Single().Containers;
		containers.Length.Should().Be(1);
		containers.Single().Tag.Should().Be("default");
	}

	[Test]
	public void ShouldWriteContainerTag()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration {Tag = "default"},
				new ContainerConfiguration {Tag = "old"}
			}
		});

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 1,
			ContainerIndex = 1,
			Tag = "new-tag"
		});

		system.Configurations().Single().Containers[1].Tag
			.Should().Be("new-tag");
	}

	[Test]
	public void ShouldWriteContainerQueues()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration {Tag = "default"},
				new ContainerConfiguration {Tag = "secondary"}
			}
		});

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 1,
			ContainerIndex = 1,
			Tag = "secondary",
			Queues = new[] {"alpha", "beta"}
		});

		system.Configurations().Single().Containers[1].Queues
			.Should().Have.SameSequenceAs(new[] {"alpha", "beta"});
	}

	[Test]
	public void ShouldNotOverwriteTagWhenNull()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration {Tag = "keep-me"}
			}
		});

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 1,
			ContainerIndex = 0,
			Tag = null,
			WorkerBalancerEnabled = true,
			Workers = 5
		});

		system.Configurations().Single().Containers[0].Tag
			.Should().Be("keep-me");
	}

	[Test]
	public void ShouldNotOverwriteQueuesWhenNull()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration
				{
					Tag = "default",
					Queues = new[] {"existing"}
				}
			}
		});

		system.ConfigurationApi().WriteContainer(new WriteContainer
		{
			ConfigurationId = 1,
			ContainerIndex = 0,
			Queues = null,
			WorkerBalancerEnabled = true
		});

		system.Configurations().Single().Containers[0].Queues
			.Should().Have.SameSequenceAs(new[] {"existing"});
	}

	[Test]
	public void ShouldAddMultipleContainers()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Id = 1});

		system.ConfigurationApi().AddContainer(1, "tag1");
		system.ConfigurationApi().AddContainer(1, "tag2");

		var containers = system.Configurations().Single().Containers;
		containers.Length.Should().Be(3);
		containers[1].Tag.Should().Be("tag1");
		containers[2].Tag.Should().Be("tag2");
	}

	[Test]
	public void ShouldNotAffectOtherConfigurationsWhenAdding()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Id = 1});
		system.WithConfiguration(new StoredConfiguration {Id = 2});

		system.ConfigurationApi().AddContainer(2, "my-tag");

		system.Configurations().Single(x => x.Id == 1).Containers.Length.Should().Be(1);
		var config2 = system.Configurations().Single(x => x.Id == 2);
		config2.Containers.Length.Should().Be(2);
		config2.Containers[1].Tag.Should().Be("my-tag");
	}
}
