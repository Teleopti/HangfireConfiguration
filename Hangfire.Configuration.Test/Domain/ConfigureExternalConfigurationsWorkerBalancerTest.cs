using System.Data.SqlClient;
using System.Linq;
using Npgsql;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ConfigureExternalConfigurationsWorkerBalancerTest
{
	[Test]
	public void ShouldEnableForSqlServer()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "."}.ToString(),
				}
			}
		});
		system.BackgroundJobServerStarter.Start();

		system.Configurations().Single().WorkerBalancerEnabled
			.Should().Be(true);
	}

	[Test]
	public void ShouldDisableForPostgres()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = new NpgsqlConnectionStringBuilder {Host = "localhost"}.ToString(),
				}
			}
		});
		system.BackgroundJobServerStarter.Start();

		system.Configurations().Single().WorkerBalancerEnabled
			.Should().Be(false);
	}

	[Test]
	public void ShouldDisableForRedis()
	{
		var system = new SystemUnderTest();

		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = "my-redis"
				}
			}
		});
		system.BackgroundJobServerStarter.Start();

		system.Configurations().Single().WorkerBalancerEnabled
			.Should().Be(false);
	}
	
	[Test]
	public void ShouldNotReenableIfConnectionStringIsChanged()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1, 
			Name = "hangfire", 
			ConnectionString = new SqlConnectionStringBuilder {DataSource = "original"}.ToString(),
			WorkerBalancerEnabled = false
		});
		
		system.UseOptions(new ConfigurationOptionsForTest
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					Name = "hangfire",
					ConnectionString = new SqlConnectionStringBuilder {DataSource = "changed"}.ToString()
				}
			}
		});
		system.BackgroundJobServerStarter.Start();
		
		system.Configurations().Single().WorkerBalancerEnabled
			.Should().Be(false);
	}
}