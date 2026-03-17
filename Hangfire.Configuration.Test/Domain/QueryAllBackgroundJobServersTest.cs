using System.Linq;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class QueryAllBackgroundJobServersTest
{
	[Test]
	public void ShouldQueryWorkerServers()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		var workerServers = system.Queries.QueryAllBackgroundJobServers();

		Assert.NotNull(workerServers.Single());
	}

	[Test]
	public void ShouldReturnWorkerServer()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		var workerServer = system.Queries.QueryAllBackgroundJobServers().Single();

		workerServer.JobStorage
			.Should().Be.SameInstanceAs(system.Hangfire.CreatedStorages.Single());
	}

	[Test]
	public void ShouldAutoUpdate()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration());

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new []
			{
				new ExternalConfiguration
				{
					ConnectionString = @"Data Source=.;Initial Catalog=Hangfire;",
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.Queries.QueryAllBackgroundJobServers();

		system.Configurations().Single().ConnectionString
			.Should().Contain("Hangfire");
	}

	[Test]
	public void ShouldQueryWorkerServersWithDefaultSqlStorageOptions()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {ConnectionString = @"Data Source=.;Initial Catalog=fakedb;" });

		system.UseStorageOptions(new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});
		system.Queries.QueryAllBackgroundJobServers();

		Assert.False(system.Hangfire.CreatedStorages.Single().SqlServerOptions.PrepareSchemaIfNecessary);
	}

	[Test]
	public void ShouldQueryWorkerServersWithDefaultPostgresStorageOptions()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {ConnectionString =  @"Host=localhost;Database=fakedb;" });

		system.UseStorageOptions(new PostgreSqlStorageOptions {PrepareSchemaIfNecessary = false});
		system.Queries.QueryAllBackgroundJobServers();

		Assert.False(system.Hangfire.CreatedStorages.Single().PostgresOptions.PrepareSchemaIfNecessary);
	}
        
	[Test]
	public void ShouldReturnStorageConfigurationId()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Id = 3});

		var workerServer = system.Queries.QueryAllBackgroundJobServers().Single();

		Assert.AreEqual(3, workerServer.ConfigurationId);
	}

	[Test]
	public void ShouldReturnConfigurationName()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Name = "name"});

		var workerServer = system.Queries.QueryAllBackgroundJobServers().Single();

		Assert.AreEqual("name", workerServer.Name);
	}
}