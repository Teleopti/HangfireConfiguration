using System.Linq;
using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using static Hangfire.Configuration.Test.Extensions;

namespace Hangfire.Configuration.Test.Web;

[Parallelizable(ParallelScope.None)]
public class ContainerManagementTest
{
	[Test]
	public void ShouldAddContainer()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest
		{
			EnableContainerManagement = true
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1
		});

		using var s = new WebServerUnderTest(system);
		s.TestClient.PostAsync(
			"/config/addContainer",
			FormContent(new {configurationId = 1, tag = "my-tag"})
		).Wait();

		var containers = system.Configurations().Single().Containers;
		containers.Length.Should().Be(2);
		containers[1].Tag.Should().Be("my-tag");
	}

	[Test]
	public void ShouldNotAddContainerWithoutTag()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest
		{
			EnableContainerManagement = true
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1
		});

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.PostAsync(
			"/config/addContainer",
			FormContent(new {configurationId = 1})
		).Result;

		Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
		var content = response.Content.ReadAsStringAsync().Result;
		content.Should().Contain("Tag is required when adding a container");
	}

	[Test]
	public void ShouldRemoveContainer()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest
		{
			EnableContainerManagement = true
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers =
			[
				new ContainerConfiguration {Tag = "default"},
				new ContainerConfiguration {Tag = "secondary"}
			]
		});

		using var s = new WebServerUnderTest(system);
		s.TestClient.PostAsync(
			"/config/removeContainer",
			FormContent(new
			{
				configurationId = 1,
				containerIndex = 1
			})
		).Wait();

		var containers = system.Configurations().Single().Containers;
		containers.Length.Should().Be(1);
		containers.Single().Tag.Should().Be("default");
	}

	[Test]
	public void ShouldSaveContainerWithTagAndQueues()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest
		{
			EnableContainerManagement = true
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["alpha", "beta"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = "default"
				},
				new ContainerConfiguration
				{
					Tag = "old-tag"
				}
			]
		});

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.PostAsync(
			"/config/saveContainer",
			FormContent(new
			{
				configurationId = 1,
				containerIndex = 1,
				tag = "my-tag",
				queues = "alpha, beta",
				workerBalancerEnabled = "on",
				workers = 5,
				maxWorkersPerServer = 2
			})
		).Result;

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var container = system.Configurations().Single().Containers[1];
		container.Tag.Should().Be("my-tag");
		container.Queues.Should().Have.SameSequenceAs(["alpha", "beta"]);
		container.WorkerBalancerEnabled.Should().Be(true);
		container.GoalWorkerCount.Should().Be(5);
		container.MaxWorkersPerServer.Should().Be(2);
	}

	[Test]
	public void ShouldShowContainerManagementUI()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest
		{
			EnableContainerManagement = true
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = "Hangfire"
				},
				new ContainerConfiguration
				{
					Tag = "secondary",
					Queues = ["queue1"]
				}
			]
		});

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.GetAsync("/config").Result;
		var content = response.Content.ReadAsStringAsync().Result;

		content.Should().Contain("Container - Hangfire");
		content.Should().Contain("Container - secondary");
		content.Should().Contain("Add container");
		content.Should().Contain("name='tag'");
		content.Should().Contain("type='checkbox' name='queues'");
		content.Should().Contain("value='queue1' checked");
	}

	[Test]
	public void ShouldNotShowContainerManagementUI()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = "Hangfire"
				},
				new ContainerConfiguration
				{
					Tag = "secondary"
				}
			]
		});

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.GetAsync("/config").Result;
		var content = response.Content.ReadAsStringAsync().Result;

		content.Should().Not.Contain("Add container");
		content.Should().Not.Contain("name='tag'");
		content.Should().Not.Contain("name='queues'");
	}

	[Test]
	public void ShouldOnlyRefreshAddedContainer()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest
		{
			EnableContainerManagement = true
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = "default"
				}
			]
		});

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.PostAsync(
			"/config/addContainer",
			FormContent(new
			{
				configurationId = 1,
				tag = "new-tag"
			})
		).Result;

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var content = response.Content.ReadAsStringAsync().Result;
		
		content.Should().Contain("Container - new-tag");
		content.Should().Not.Contain("Container - default");
		content.Should().Contain("Add container");
	}

	[Test]
	public void ShouldOnlyRefreshSavedContainer()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest
		{
			EnableContainerManagement = true
		});
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = "Hangfire"
				},
				new ContainerConfiguration
				{
					Tag = "secondary",
					Queues = ["queue1"]
				}
			]
		});

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.PostAsync(
			"/config/saveContainer",
			FormContent(new
			{
				configurationId = 1,
				containerIndex = 1,
				tag = "secondary",
				queues = "queue1",
				workerBalancerEnabled = "on",
				workers = 5,
				maxWorkersPerServer = 2
			})
		).Result;

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var content = response.Content.ReadAsStringAsync().Result;
		
		content.Should().Contain("Container - secondary");
		content.Should().Not.Contain("Container - Hangfire");
		content.Should().Not.Contain("Add container");
	}
}