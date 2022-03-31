using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class QueryResultTest
{
	[Test]
	public void ShouldReturnSameStorage()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Active = true});

		var storage1 = system.QueryPublishers().Single().JobStorage;
		var storage2 = system.QueryPublishers().Single().JobStorage;

		storage1.Should().Be.SameInstanceAs(storage2);
	}

	[Test]
	public void ShouldReturnBackgroundJobClient()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Active = true});

		var publisher = system.QueryPublishers().Single();

		publisher.BackgroundJobClient.Should().Be.OfType<BackgroundJobClient>();
	}

	[Test]
	public void ShouldReturnSameBackgroundJobClient()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Active = true});

		var publisher1 = system.QueryPublishers().Single();
		var publisher2 = system.QueryPublishers().Single();

		publisher1.BackgroundJobClient.Should()
			.Be.SameInstanceAs(publisher2.BackgroundJobClient);
	}

	[Test]
	public void ShouldReturnRecurringJobManager()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		var result = system.QueryAllWorkerServers().Single();

		result.RecurringJobManager.Should().Be.OfType<RecurringJobManager>();
	}

	[Test]
	public void ShouldReturnSameRecurringJobManager()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		var result1 = system.QueryAllWorkerServers().Single();
		var result2 = system.QueryAllWorkerServers().Single();

		result1.RecurringJobManager.Should()
			.Be.SameInstanceAs(result2.RecurringJobManager);
	}

	[Test]
	public void ShouldReturnMonitoringApi()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		var result = system.QueryAllWorkerServers().Single();

		result.MonitoringApi.Should().Be.OfType<FakeMonitoringApi>();
	}

	[Test]
	public void ShouldReturnSameMonitoringApi()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		var result1 = system.QueryAllWorkerServers().Single();
		var result2 = system.QueryAllWorkerServers().Single();

		result1.MonitoringApi.Should()
			.Be.SameInstanceAs(result2.MonitoringApi);
	}
}