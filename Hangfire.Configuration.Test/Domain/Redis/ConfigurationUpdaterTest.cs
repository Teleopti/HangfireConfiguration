using System.Linq;
using Hangfire.Pro.Redis;
using Xunit;

namespace Hangfire.Configuration.Test.Domain.Redis
{
	public class ConfigurationUpdaterTest
	{
		[Fact]
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

			Assert.Same(options, system.Hangfire.StartedServers.Single().storage.RedisOptions);
		}
	}
}