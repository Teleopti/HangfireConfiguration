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
		system.WithConfiguration(new StoredConfiguration {Id = 1, WorkerBalancerEnabled = true});
		system.WithConfiguration(new StoredConfiguration {Id = 2, WorkerBalancerEnabled = true});

		system.ConfigurationApi().DisableWorkerBalancer(2);

		system.Configurations().Single(x => x.Id == 1).WorkerBalancerEnabled
			.Should().Be(true);
		system.Configurations().Single(x => x.Id == 2).WorkerBalancerEnabled
			.Should().Be(false);
	}

	[Test]
	public void ShouldEnable()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Id = 1, WorkerBalancerEnabled = false});
		system.WithConfiguration(new StoredConfiguration {Id = 2, WorkerBalancerEnabled = false});

		system.ConfigurationApi().EnableWorkerBalancer(2);

		system.Configurations().Single(x => x.Id == 1).WorkerBalancerEnabled
			.Should().Be(false);
		system.Configurations().Single(x => x.Id == 2).WorkerBalancerEnabled
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

		system.Configurations().Last().WorkerBalancerEnabled
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

		system.Configurations().Last().WorkerBalancerEnabled
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

		system.Configurations().Last().WorkerBalancerEnabled
			.Should().Be(false);
	}
}