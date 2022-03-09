using System.Linq;
using Hangfire.Pro.Redis;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain.Redis
{
	public class ConfigurationUpdaterTest
	{
		[Test]
		public void ShouldUseRedisOptions()
		{
			var system = new SystemUnderTest();
			var options = new RedisStorageOptions();
			system.WorkerServerStarter.Start(new ConfigurationOptions
			{
				UpdateConfigurations = new []
				{
					new UpdateStorageConfiguration
					{
						ConnectionString = "redis$$connstring",
						Name = DefaultConfigurationName.Name()
					}
				}
			}, null, options);

			system.Hangfire.StartedServers.Single().storage.RedisOptions
				.Should().Be.SameInstanceAs(options);
		}
	}
}