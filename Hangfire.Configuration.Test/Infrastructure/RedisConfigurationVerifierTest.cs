using NUnit.Framework;
using StackExchange.Redis;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[InstallRedis]
public class RedisConfigurationVerifierTest
{
	[Test]
	public void ShouldThrowIfUnknownServer()
	{
		var system = new SystemUnderInfraTest();

		Assert.Throws<RedisConnectionException>(() =>
		{
			system.RedisConfigurationVerifier.TryConnect("UnknownServer,ConnectTimeout=100");
		});
	}
	
	[Test]
	public void ShouldNotThrowIfPresentServer()
	{
		var system = new SystemUnderInfraTest();

		Assert.DoesNotThrow(() =>
		{
			system.RedisConfigurationVerifier.TryConnect("localhost,ConnectTimeout=100");
		});
	}
}