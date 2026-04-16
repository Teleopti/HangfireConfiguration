using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class CreateServerConfigurationRedisTest
{
	[Test]
	public void ShouldStoreConnectionString()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
		{
			Configuration = "AwesomeServer:425"
		});

		var storedConfiguration = system.Configurations().Single();
		storedConfiguration.ConnectionString.Should().Be("AwesomeServer:425");
	}

	[Test]
	public void ShouldStoreName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
		{
			Name = "matte"
		});

		var storedConfiguration = system.Configurations().Single();
		storedConfiguration.Name.Should().Be("matte");
	}

	[Test]
	public void ShouldDefaultToNonActive()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
		{
			Configuration = "redis"
		});

		var storedConfiguration = system.Configurations().Single();
		storedConfiguration.Active.Should().Be(false);
	}

	[Test]
	public void ShouldCreateWithSchemaName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
		{
			Prefix = "{my-prefix}:"
		});

		var storedConfiguration = system.Configurations().Last();
		storedConfiguration.SchemaName.Should().Be("{my-prefix}:");
	}

	[Test]
	public void ShouldCreateWithDefaultPrefixAsSchemaName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
		{
			Prefix = null
		});

		var storedConfiguration = system.Configurations().Last();
		storedConfiguration.SchemaName.Should().Be("{hangfire}:");
	}
	
	[Test]
	public void ShouldCallVerifier()
	{
		var system = new SystemUnderTest();
	
		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
		{
			Configuration = "AwesomeServer", 
			Prefix = "{prefix}:"
		});

		system.RedisConnectionVerifier.WasSucessfullyVerifiedWith
			.Should().Be.EqualTo(("AwesomeServer", "{prefix}:"));
	}

	[Test]
	public void ShouldCallVerifierWithCorrectPrefixWhenNotSet()
	{
		var system = new SystemUnderTest();
	
		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
		{
			Configuration = "AwesomeServer"
		});

		system.RedisConnectionVerifier.WasSucessfullyVerifiedWith
			.Should().Be.EqualTo(("AwesomeServer", "{hangfire}:"));
	}
	
	[Test]
	public void ShouldNotCreateServerConfigurationIfVerifierThrows()
	{
		var system = new SystemUnderTest();
		system.RedisConnectionVerifier.Throws();

		Assert.Catch(() =>
			system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
			{
				Configuration = "someconfig"
			}));
		system.Configurations().Should().Be.Empty();
	}
	
	[TestCase("a{prefix}:")]
	[TestCase("{prefix}:a")]
	[TestCase("{prefix}")]
	[TestCase("prefix}:")]
	[TestCase("{}:")]
	[TestCase("{pre{fix}:")]
	[TestCase("{pre}fix}:")]
	public void ShouldThrowIfPrefixIsNotCorrect(string prefix)
	{
		var system = new SystemUnderTest();

		Assert.Throws<ArgumentException>(() =>
		{
			system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
			{
				Configuration = "localhost",
				Prefix = prefix
			});
		});
	}

	[Test]
	public void ShouldHaveWorkerBalancerDisabled()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisServer
		{
			Configuration = "my-redis"
		});

		system.Configurations().Last().Containers.Single().WorkerBalancerEnabled
			.Should().Be(false);
	}
}
