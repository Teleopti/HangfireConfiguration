using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class DisableWorkerBalancerTest
{
	[Test]
	public void ShouldDisable()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration {Id = 1, WorkerBalancerEnabled = true});
		system.ConfigurationStorage.Has(new StoredConfiguration {Id = 2, WorkerBalancerEnabled = true});

		system.ConfigurationApi().DisableWorkerBalancer(2);

		system.ConfigurationStorage.Data.Single(x => x.Id == 1).WorkerBalancerEnabled
			.Should().Be(true);
		system.ConfigurationStorage.Data.Single(x => x.Id == 2).WorkerBalancerEnabled
			.Should().Be(false);
	}

	[Test]
	public void ShouldEnable()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration {Id = 1, WorkerBalancerEnabled = false});
		system.ConfigurationStorage.Has(new StoredConfiguration {Id = 2, WorkerBalancerEnabled = false});

		system.ConfigurationApi().EnableWorkerBalancer(2);

		system.ConfigurationStorage.Data.Single(x => x.Id == 1).WorkerBalancerEnabled
			.Should().Be(false);
		system.ConfigurationStorage.Data.Single(x => x.Id == 2).WorkerBalancerEnabled
			.Should().Be(true);
	}

	[Test]
	public void ShouldBeEnabledForNewSqlServer()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServerWorkerServer
		{
			Server = "."
		});

		system.ConfigurationStorage.Data.Last().WorkerBalancerEnabled
			.Should().Be(true);
	}

	[Test]
	public void ShouldBeDisabledForNewPostgres()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreatePostgresWorkerServer
		{
			Server = "localhost"
		});

		system.ConfigurationStorage.Data.Last().WorkerBalancerEnabled
			.Should().Be(false);
	}

	[Test]
	public void ShouldBeDisabledForNewRedis()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
		{
			Configuration = "my-redis"
		});

		system.ConfigurationStorage.Data.Last().WorkerBalancerEnabled
			.Should().Be(false);
	}
}