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

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
		{
			Configuration = "AwesomeServer:425"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.AreEqual("AwesomeServer:425", storedConfiguration.ConnectionString);
	}

	[Test]
	public void ShouldStoreName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
		{
			Name = "matte"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.AreEqual("matte", storedConfiguration.Name);
	}

	[Test]
	public void ShouldDefaultToNonActive()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
		{
			Configuration = "redis"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.False(storedConfiguration.Active);
	}

	[Test]
	public void ShouldCreateWithSchemaName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
		{
			Prefix = "my-prefix:"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Last();
		Assert.AreEqual("my-prefix:", storedConfiguration.SchemaName);
	}

	[Test]
	public void ShouldCreateWithDefaultPrefixAsSchemaName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
		{
			Prefix = null
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Last();
		Assert.AreEqual(DefaultSchemaName.Redis(), storedConfiguration.SchemaName);
	}
	
	[Test]
	public void ShouldCallVerifier()
	{
		var system = new SystemUnderTest();
	
		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
		{
			Configuration = "AwesomeServer", Prefix = "prefix"
		});

		system.RedisConfigurationVerifier.WasSucessfullyVerifiedWith
			.Should().Be.EqualTo(("AwesomeServer", "prefix"));
	}

	[Test]
	public void ShouldCallVerifierWithCorrectPrefixWhenNotSet()
	{
		var system = new SystemUnderTest();
	
		system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
		{
			Configuration = "AwesomeServer"
		});

		system.RedisConfigurationVerifier.WasSucessfullyVerifiedWith
			.Should().Be.EqualTo(("AwesomeServer", DefaultSchemaName.Redis()));
	}
	
	[Test]
	public void ShouldNotCreateServerConfigurationIfVerifierThrows()
	{
		var system = new SystemUnderTest();
		system.RedisConfigurationVerifier.Throws();

		Assert.Catch(() =>
			system.ConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
			{
				Configuration = "someconfig"
			}));
		system.ConfigurationStorage.Data.Should().Be.Empty();
	}
}