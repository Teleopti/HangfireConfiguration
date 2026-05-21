using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ConfigurationUpdaterConcurrencyTest
{
	[Test]
	public void ShouldNotBuildJobStorageFromStaleSnapshotWhenAnotherWriterAppliesEquivalentFixBeforeFirstPass()
	{
		var externalConnectionString = new SqlConnectionStringBuilder {DataSource = "from-external"}.ToString();
		var system = new SystemUnderTest();

		// when app uses ConfigurationOptions.ExternalConfigurations
		// it may clear the connection string in another step during deployment
		// so an existing named configuration without connection information exists
		system.WithConfiguration(new StoredConfiguration
		{
			Name = DefaultConfigurationName.Name(),
			ConnectionString = null,
			Active = true
		});

		// Simulate that after the system reads the stored configurations
		// Another process fixes the connection string, putting in the valid one
		system.KeyValueStore.AfterRead(() =>
		{
			var row = system.ConfigurationStorage.ReadConfigurations().Single();
			row.ConnectionString = externalConnectionString;
			system.ConfigurationStorage.WriteConfiguration(row);
		});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations =
			[
				new ExternalConfiguration
				{
					Name = DefaultConfigurationName.Name(),
					ConnectionString = externalConnectionString
				}
			]
		});

		var startedStorage = system.Hangfire.StartedServers.Single().storage;
		startedStorage.ConnectionString.Should().Be(externalConnectionString);
	}

	[Test]
	public void ShouldNotBuildJobStorageFromStaleSnapshotWhenAnotherWriterAppliesEquivalentFixDuringUpdate()
	{
		var externalConnectionString = new SqlConnectionStringBuilder {DataSource = "from-external"}.ToString();
		var system = new SystemUnderTest();

		// when app uses ConfigurationOptions.ExternalConfigurations
		// it may clear the connection string in another step during deployment
		// so an existing named configuration without connection information exists
		system.WithConfiguration(new StoredConfiguration
		{
			Name = DefaultConfigurationName.Name(),
			ConnectionString = null,
			Active = true
		});

		// Simulate that just before the update transaction happens
		// Another process fixes the connection string, putting in the valid one
		// So that this sytems update doesnt do anything, but this system still need to reread
		system.KeyValueStore.BeforeReadInTransaction(() =>
		{
			var row = system.ConfigurationStorage.ReadConfigurations().Single();
			row.ConnectionString = externalConnectionString;
			system.ConfigurationStorage.WriteConfiguration(row);
		});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations =
			[
				new ExternalConfiguration
				{
					Name = DefaultConfigurationName.Name(),
					ConnectionString = externalConnectionString
				}
			]
		});

		var startedStorage = system.Hangfire.StartedServers.Single().storage;
		startedStorage.ConnectionString.Should().Be(externalConnectionString);
	}
}