using NUnit.Framework;
using StackExchange.Redis;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[InstallRedis]
public class CreateServerConfigurationRedisTest
{
	[Test]
	public void ShouldThrowIfUnknownServer()
	{
		var system = new SystemUnderInfraTest();

		Assert.Throws<RedisConnectionException>(() =>
		{
			
			system.BuildConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
			{
				Configuration = "UnknownServer,ConnectTimeout=100"
			});
		});
	}
	
	[Test]
	public void ShouldNotThrowIfPresentServer()
	{
		var system = new SystemUnderTest();

		Assert.DoesNotThrow(() =>
		{
			system.ConfigurationApi.CreateServerConfiguration(new CreateRedisWorkerServer
			{
				Configuration = "localhost"
			});
		});
	}
}