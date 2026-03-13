using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
	public class ConfigureUpdateConfigurationsRedisTest
	{
		[Test]
		public void ShouldUseRedisOptions()
		{
			var system = new SystemUnderTest();
			system.WorkerServerStarter.Start(new ConfigurationOptions
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
}
