using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class ConfigureExternalConfigurationsRedisTest
{
	[Test]
	public void ShouldUseRedisOptions()
	{
		var system = new SystemUnderTest();
		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					Name = "redis",
					ConnectionString = "redis$$connstring",
				}
			}
		});

		var configuration = system.Configurations().Single();
		Assert.AreEqual("redis", configuration.Name);
		Assert.AreEqual("redis$$connstring", configuration.ConnectionString);
	}
}