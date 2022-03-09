using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain.Redis;

public class CreateServerConfigurationTest
{
	[Test]
	public void ShouldStoreConnectionString()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration()
		{
			Server = "AwesomeServer:425",
			DatabaseProvider = "redis"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.AreEqual("redis$$AwesomeServer:425", storedConfiguration.ConnectionString);
	}
	
	[Test]
	public void ShouldStoreName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration()
		{
			DatabaseProvider = "redis",
			Name = "matte"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.AreEqual("matte", storedConfiguration.Name);
	}
	
	[Test]
	public void ShouldDefaultToNonActive()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration()
		{
			DatabaseProvider = "redis"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.False(storedConfiguration.Active);
	}
}