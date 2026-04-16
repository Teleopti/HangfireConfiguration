using System.Linq;
using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using static Hangfire.Configuration.Test.Extensions;

namespace Hangfire.Configuration.Test.Web;

[Parallelizable(ParallelScope.None)]
public class ContainerSaveTest
{
	[Test]
	public void ShouldSaveContainer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration
				{
					GoalWorkerCount = 3
				}
			}
		});

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.PostAsync(
			"/config/saveContainer",
			FormContent(new
			{
				configurationId = 1,
				workerBalancerEnabled = "on",
				workers = 10,
				maxWorkersPerServer = 5
			})
		).Result;

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var container = system.Configurations().Single().Containers.First();
		container.WorkerBalancerEnabled.Should().Be(true);
		container.GoalWorkerCount.Should().Be(10);
		container.MaxWorkersPerServer.Should().Be(5);
	}

	[Test]
	public void ShouldSaveContainerWithDisabledWorkerBalancer()
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

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.PostAsync(
			"/config/saveContainer",
			FormContent(new
			{
				configurationId = 1,
				workers = 10,
				maxWorkersPerServer = 5
			})
		).Result;

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		system.Configurations().Single().Containers.First()
			.WorkerBalancerEnabled.Should().Be(false);
	}

	[Test]
	public void ShouldSaveContainerWithEmptyWorkerCounts()
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

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.PostAsync(
			"/config/saveContainer",
			FormContent(new
			{
				configurationId = 1,
				workerBalancerEnabled = "on",
				workers = "",
				maxWorkersPerServer = ""
			})
		).Result;

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var container = system.Configurations().Single().Containers.First();
		container.GoalWorkerCount.Should().Be(null);
		container.MaxWorkersPerServer.Should().Be(null);
	}

	[Test]
	public void ShouldSaveContainerWithWorkerCountAboveMax()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptionsForTest { MaximumGoalWorkerCount = 10 });
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			Containers = new[]
			{
				new ContainerConfiguration
				{
					GoalWorkerCount = 3
				}
			}
		});

		using var s = new WebServerUnderTest(system);
		var response = s.TestClient.PostAsync(
			"/config/saveContainer",
			FormContent(new
			{
				configurationId = 1,
				workerBalancerEnabled = "on",
				workers = 11,
				maxWorkersPerServer = ""
			})
		).Result;

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		system.Configurations().Single().Containers.First()
			.GoalWorkerCount.Should().Be(11);
	}
}
