using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class StartWorkerServersInactiveShutdownTest
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

		system.StartWorkerServers();

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

		system.StartWorkerServers();

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

		system.StartWorkerServers();

		system.Hangfire.StartedServers
			.Single().storage.ConnectionString.Should().Be("active");
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

		system.StartWorkerServers();

		system.Hangfire.StartedServers
			.Single().storage.ConnectionString.Should().Be("inactive");
		system.Configurations().Single().ShutdownAt.Should().Be("2026-03-13 11:00".Utc());
	}
}