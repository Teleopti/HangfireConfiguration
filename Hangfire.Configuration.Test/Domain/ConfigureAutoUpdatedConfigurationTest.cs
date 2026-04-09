using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ConfigureAutoUpdatedConfigurationTest
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
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		var dataSource = new SqlConnectionStringBuilder(system.Configurations().Single().ConnectionString).DataSource;
		Assert.AreEqual("DataSource", dataSource);
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
		system.WithConfiguration(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {DataSource = "existing"}.ToString()});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "autoupdated"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		var actual = system.Configurations().OrderBy(x => x.Id).Last();
		Assert.AreEqual("autoupdated", new SqlConnectionStringBuilder(actual.ConnectionString).DataSource);
	}

	[Test]
	public void ShouldUpdate()
	{
		var system = new SystemUnderTest();
		var existing = new SqlConnectionStringBuilder {DataSource = "existingDataSource", ApplicationName = "existingApplicationName.AutoUpdate"}.ToString();
		system.WithConfiguration(new StoredConfiguration {ConnectionString = existing});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "newDataSource", ApplicationName = "newApplicationName"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		var updatedConnectionString = new SqlConnectionStringBuilder(system.Configurations().Single().ConnectionString);
		Assert.AreEqual("newDataSource", updatedConnectionString.DataSource);
		Assert.AreEqual("newApplicationName", updatedConnectionString.ApplicationName);
	}

	[Test]
	public void ShouldUpdateLegacyConfiguration()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 55 } }});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "dataSource", ApplicationName = "applicationName"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		var expected = new SqlConnectionStringBuilder {DataSource = "dataSource", ApplicationName = "applicationName"}.ToString();
		Assert.AreEqual(55, system.Configurations().Single().Containers.Single().GoalWorkerCount);
		Assert.AreEqual(expected, system.Configurations().Single().ConnectionString);
	}

	[Test]
	public void ShouldUpdateOneOfTwo()
	{
		var system = new SystemUnderTest();
		var one = new SqlConnectionStringBuilder {DataSource = "One"}.ToString();
		var two = new SqlConnectionStringBuilder {DataSource = "Two", ApplicationName = "Two"}.ToString();
		system.WithConfiguration(new StoredConfiguration {ConnectionString = one});
		system.WithConfiguration(new StoredConfiguration {ConnectionString = two});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "Updated", ApplicationName = "Updated"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		system.Configurations().First().ConnectionString.Should().Be("Data Source=Updated;Application Name=Updated");
		system.Configurations().Last().ConnectionString.Should().Be("Data Source=Two;Application Name=Two");
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
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(),
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
		system.WithConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 4 } }});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(),
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
			ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(),
			Active = true
		});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(),
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
		system.WithConfiguration(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(), Active = false});
		system.WithConfiguration(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(), Active = true});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "AutoUpdate"}.ToString(),
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
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "AutoUpdate"}.ToString(),
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
		system.WithConfiguration(new StoredConfiguration {Containers = new[] { new ContainerConfiguration { GoalWorkerCount = 4 } }});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "AutoUpdate"}.ToString(),
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
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "FirstUpdate"}.ToString(),
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
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "SecondUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.QueryPublishers();

		var dataSource = new SqlConnectionStringBuilder(system.Configurations().Single().ConnectionString).DataSource;
		Assert.AreEqual("FirstUpdate", dataSource);
	}

	[Test]
	public void ShouldAutoUpdateTwiceIfAllConfigurationsWhereRemoved()
	{
		var system = new SystemUnderTest();
		system.BackgroundJobServerStarter.Start(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "FirstUpdate"}.ToString(),
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
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "SecondUpdate"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.QueryPublishers();

		var dataSource = new SqlConnectionStringBuilder(system.Configurations().Single().ConnectionString).DataSource;
		Assert.AreEqual("SecondUpdate", dataSource);
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
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(),
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
		system.WithConfiguration(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(), Active = false});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(),
					Name = DefaultConfigurationName.Name()
				}
			}
		});

		Assert.AreEqual("Hangfire", system.Configurations().Single().Name);
	}
}