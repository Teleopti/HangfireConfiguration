using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class StartBackgroundProcessesTest
{
	[Test]
	public void ShouldStartBackgroundProcesses()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		system.StartBackgroundProcesses([new Worker()]);

		system.Hangfire.BackgroundProcessesStarted
			.Single().backgroundProcesses.Single().Should().Be.OfType<Worker>();
	}

	[Test]
	public void ShouldStartBackground2Processes()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		system.StartBackgroundProcesses([new Worker(), new Worker()]);

		system.Hangfire.BackgroundProcessesStarted
			.Single().backgroundProcesses.Should().Have.Count.EqualTo(2);
	}

	[Test]
	public void ShouldStartBackgroundProcessesForFirstConfiguration()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {ConnectionString = "first"});
		system.WithConfiguration(new StoredConfiguration {ConnectionString = "second"});

		system.StartBackgroundProcesses([new Worker()]);

		system.Hangfire.BackgroundProcessesStarted
			.Single().storage.ConnectionString.Should().Be.EqualTo("first");
	}

	[Test]
	public void ShouldStartBackgroundProcessesForActiveConfiguration()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Active = false, ConnectionString = "inactive"});
		system.WithConfiguration(new StoredConfiguration {Active = true, ConnectionString = "active"});

		system.StartBackgroundProcesses([new Worker()]);

		system.Hangfire.BackgroundProcessesStarted
			.Single().storage.ConnectionString.Should().Be.EqualTo("active");
	}

	[Test]
	public void ShouldConstructHangfireStorage()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			SchemaName = "schema",
			ConnectionString = "Data Source=."
		});
		system.UseStorageOptions(new SqlServerStorageOptions
		{
			PrepareSchemaIfNecessary = false
		});

		system.WorkerServerStarter.Start();

		system.Hangfire.StartedServers.Single().storage.Should().Not.Be(null);
		system.Hangfire.StartedServers.Single().storage.ConnectionString.Should().Be("Data Source=.");
		system.Hangfire.StartedServers.Single().storage.SqlServerOptions.PrepareSchemaIfNecessary.Should().Be(false);
		system.Hangfire.StartedServers.Single().storage.SqlServerOptions.SchemaName.Should().Be("schema");
	}
}