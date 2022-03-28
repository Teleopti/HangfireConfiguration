using System;
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
			system.RedisConfigurationVerifier.VerifyConfiguration("UnknownServer,ConnectTimeout=100", null);
		});
	}
	
	[Test]
	public void ShouldNotThrowIfPresentServer()
	{
		var system = new SystemUnderInfraTest();

		Assert.DoesNotThrow(() =>
		{
			system.RedisConfigurationVerifier.VerifyConfiguration("localhost,ConnectTimeout=100", null);
		});
	}

	[Test]
	public void ShouldThrowIfKeyWithPrefixAlreadyExists()
	{
		using(var redis = ConnectionMultiplexer.Connect("localhost"))
			redis.GetDatabase().StringSet("someprefix:foo", "bar");
		var system = new SystemUnderInfraTest();

		Assert.Throws<ArgumentException>(() => { system.RedisConfigurationVerifier.VerifyConfiguration("localhost", "someprefix:"); });
	}
}