using System.Linq;
using Npgsql;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ConfigureAutoUpdatedConfigurationPostgresTest
{
	[Test]
	public void ShouldConfigureAutoUpdatedServer()
	{
		var system = new SystemUnderTest();

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "host"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		var host = new NpgsqlConnectionStringBuilder(system.Configurations().Single().ConnectionString).Host;
		Assert.AreEqual("host", host);
	}

	[Test]
	public void ShouldNotConfigureAutoUpdatedServerIfNoneGiven()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptions());
		system.BackgroundJobServerStarter.Start();

		Assert.IsEmpty(system.Configurations());
	}

	[Test]
	public void ShouldAddAutoUpdatedConfigurationIfNoMarkedExists()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {ConnectionString = new NpgsqlConnectionStringBuilder {Host = "existing"}.ToString()});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "autoupdated"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		var actual = system.Configurations().OrderBy(x => x.Id).Last();
		Assert.AreEqual("autoupdated", new NpgsqlConnectionStringBuilder(actual.ConnectionString).Host);
	}

	[Test]
	public void ShouldUpdate()
	{
		var system = new SystemUnderTest();
		var existing = new NpgsqlConnectionStringBuilder {Host = "existingDataSource", ApplicationName = "existingApplicationName.AutoUpdate"}.ToString();
		system.WithConfiguration(new StoredConfiguration {ConnectionString = existing});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "newDataSource", ApplicationName = "newApplicationName"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		var updatedConnectionString = new NpgsqlConnectionStringBuilder(system.Configurations().Single().ConnectionString);
		Assert.AreEqual("newDataSource", updatedConnectionString.Host);
		Assert.AreEqual("newApplicationName", updatedConnectionString.ApplicationName);
	}

	[Test]
	public void ShouldUpdateLegacyConfiguration()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {GoalWorkerCount = 55});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations =
			[
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "dataSource", ApplicationName = "applicationName"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			]
		});

		var expected = new NpgsqlConnectionStringBuilder {Host = "dataSource", ApplicationName = "applicationName"}.ToString();
		Assert.AreEqual(55, system.Configurations().Single().GoalWorkerCount);
		Assert.AreEqual(expected, system.Configurations().Single().ConnectionString);
	}

	[Test]
	public void ShouldUpdateOneOfTwo()
	{
		var system = new SystemUnderTest();
		var one = new NpgsqlConnectionStringBuilder {Host = "One"}.ToString();
		var two = new NpgsqlConnectionStringBuilder {Host = "Two", ApplicationName = "Two"}.ToString();
		system.WithConfiguration(new StoredConfiguration {ConnectionString = one});
		system.WithConfiguration(new StoredConfiguration {ConnectionString = two});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations =
			[
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "Updated", ApplicationName = "Updated"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			]
		});

		system.Configurations().First().ConnectionString.Should().Be("Host=Updated;Application Name=Updated");
		system.Configurations().Last().ConnectionString.Should().Be("Host=Two;Application Name=Two");
	}

	[Test]
	public void ShouldActivateOnFirstUpdate()
	{
		var system = new SystemUnderTest();

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "DataSource"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		Assert.True(system.Configurations().Single().Active);
	}

	[Test]
	public void ShouldActivateLegacyConfigurationWhenConfiguredAsDefault()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {GoalWorkerCount = 4});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "DataSource"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		Assert.True(system.Configurations().Single().Active);
	}

	[Test]
	public void ShouldBeActiveOnUpdateIfActiveBefore()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = new NpgsqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(),
			Active = true
		});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "DataSource"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		Assert.True(system.Configurations().Single().Active);
	}

	[Test]
	public void ShouldNotActivateWhenUpdating()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {ConnectionString = new NpgsqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(), Active = false});
		system.WithConfiguration(new StoredConfiguration {ConnectionString = new NpgsqlConnectionStringBuilder {Host = "DataSource"}.ToString(), Active = true});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "AutoUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		Assert.False(system.Configurations().First().Active);
		Assert.True(system.Configurations().Last().Active);
	}

	[Test]
	public void ShouldSaveSchemaName()
	{
		var system = new SystemUnderTest();

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "AutoUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name(),
					SchemaName = "schemaName"
				}
			}
		});

		Assert.AreEqual("schemaName", system.Configurations().Single().SchemaName);
	}

	[Test]
	public void ShouldSaveSchemaNameOnLegacyConfiguration()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {GoalWorkerCount = 4});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "AutoUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name(),
					SchemaName = "schemaName"
				}
			}
		});

		Assert.AreEqual("schemaName", system.Configurations().Single().SchemaName);
	}

	[Test]
	public void ShouldOnlyAutoUpdateOnce()
	{
		var system = new SystemUnderTest();
		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "FirstUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		system.UseOptions(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "SecondUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.QueryPublishers();

		var host = new NpgsqlConnectionStringBuilder(system.Configurations().Single().ConnectionString).Host;
		Assert.AreEqual("FirstUpdate", host);
	}

	[Test]
	public void ShouldAutoUpdateTwiceIfAllConfigurationsWhereRemoved()
	{
		var system = new SystemUnderTest();
		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "FirstUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.ClearConfigurations();

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "SecondUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.QueryPublishers();

		var host = new NpgsqlConnectionStringBuilder(system.Configurations().Single().ConnectionString).Host;
		Assert.AreEqual("SecondUpdate", host);
	}

	[Test]
	public void ShouldAutoUpdateWithDefaultConfigurationName()
	{
		var system = new SystemUnderTest();

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "DataSource"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		Assert.AreEqual("Hangfire", system.Configurations().Single().Name);
	}

	[Test]
	public void ShouldAutoUpdateWithDefaultConfigurationName2()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = new NpgsqlConnectionStringBuilder
			{
				ApplicationName = "ApplicationName.AutoUpdate"
			}.ToString(),
			Active = false
		});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder
					{
						Host = "DataSource"
					}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		Assert.AreEqual("Hangfire", system.Configurations().Single().Name);
	}
}