using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class StartStartBackgroundJobServersInactiveShutdownTest
{
	[Test]
	public void ShouldStartInactiveServer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Active = false, 
			ConnectionString = "inactive"
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers
			.Single().storage.ConnectionString.Should().Be("inactive");
	}

	[Test]
	public void ShouldNotStartLongTimeInactiveServer()
	{
		var system = new SystemUnderTest();
		system.Now("2026-03-12 11:00");
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "long-time-inactive",
			Active = false, 
			ShutdownAt = "2026-03-12 10:00".Utc()
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers.Should().Be.Empty();
	}
	
	[Test]
	public void ShouldStartActiveThatOnceWasShutdown()
	{
		var system = new SystemUnderTest();
		system.Now("2026-03-12 11:00");
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "active",
			Active = true,
			ShutdownAt = "2026-01-01 00:00".Utc()
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers
			.Single().storage.ConnectionString.Should().Be("active");
	}
	
	[Test]
	public void ShouldStartAfterActivateAndInactivateOfLongTimeShutdownServer()
	{
		var system = new SystemUnderTest();
		system.Now("2026-01-01 00:00");
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			ConnectionString = "server",
			Active = false,
			ShutdownAt = "2026-01-01 00:00".Utc()
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 2,
			ConnectionString = "other",
			Active = true
		});

		system.Now("2026-03-12 11:00");
		system.ConfigurationApi().ActivateServer(1);
		system.ConfigurationApi().InactivateServer(1);
		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers
			.Select(x => x.storage.ConnectionString).Should().Have.SameValuesAs("server", "other");
	}

	[Test]
	public void ShouldGiveInactiveConfigurationsAShutdownTime()
	{
		var system = new SystemUnderTest();
		system.Now("2026-03-12 11:00");
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "inactive",
			Active = false
		});

		system.StartBackgroundJobServers();

		system.Hangfire.StartedServers
			.Single().storage.ConnectionString.Should().Be("inactive");
		system.Configurations().Single().ShutdownAt.Should().Be("2026-03-13 11:00".Utc());
	}
}