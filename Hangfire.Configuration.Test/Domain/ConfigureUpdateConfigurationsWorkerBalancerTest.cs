using System.Data.SqlClient;
using System.Linq;
using Npgsql;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ConfigureUpdateConfigurationsWorkerBalancerTest
{
	[Test]
	public void ShouldEnableForSqlServer()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptions
		{
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "."}.ToString(),
				}
			}
		});
		system.WorkerServerStarter.Start();

		system.ConfigurationStorage.Data.Single().WorkerBalancerEnabled
			.Should().Be(true);
	}

	[Test]
	public void ShouldDisableForPostgres()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptions
		{
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "localhost"}.ToString(),
				}
			}
		});
		system.WorkerServerStarter.Start();

		system.ConfigurationStorage.Data.Single().WorkerBalancerEnabled
			.Should().Be(false);
	}

	[Test]
	public void ShouldDisableForRedis()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptions
		{
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					ConnectionString = "my-redis"
				}
			}
		});
		system.WorkerServerStarter.Start();

		system.ConfigurationStorage.Data.Single().WorkerBalancerEnabled
			.Should().Be(false);
	}
	
	[Test]
	public void ShouldNotReenableIfConnectionStringIsChanged()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Id = 1, 
			Name = "hangfire", 
			ConnectionString = new SqlConnectionStringBuilder {DataSource = "original"}.ToString(),
			WorkerBalancerEnabled = false
		});
		
		system.UseOptions(new ConfigurationOptions
		{
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					Name = "hangfire",
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "changed"}.ToString()
				}
			}
		});
		system.WorkerServerStarter.Start();
		
		system.ConfigurationStorage.Data.Single().WorkerBalancerEnabled
			.Should().Be(false);
	}
}