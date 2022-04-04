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
			system.RedisConfigurationVerifier.VerifyConfiguration("UnknownServer,ConnectTimeout=100", "{hangfire}:");
		});
	}
	
	[Test]
	public void ShouldNotThrowIfPresentServer()
	{
		var system = new SystemUnderInfraTest();

		Assert.DoesNotThrow(() =>
		{
			system.RedisConfigurationVerifier.VerifyConfiguration("localhost,ConnectTimeout=100", "{hangfire}:");
		});
	}
	
	[Test]
	public void ShouldNotThrowIfPrefixAndServerAlreadyNotExists()
	{
		var system = new SystemUnderInfraTest();

		system.RedisConfigurationVerifier.VerifyConfiguration("localhost", "{another}:"); // bit of a hack - simulates an earlier entry

		Assert.DoesNotThrow(() => { system.RedisConfigurationVerifier.VerifyConfiguration("localhost", "{prefixet}:"); });
	}
	
	[Test]
	public void ShouldThrowIfPrefixAndServerAlreadyExists()
	{
		var system = new SystemUnderInfraTest();

		system.RedisConfigurationVerifier.VerifyConfiguration("localhost", "{prefixet}:"); // bit of a hack - simulates an earlier entry

		Assert.Throws<ArgumentException>(() => { system.RedisConfigurationVerifier.VerifyConfiguration("localhost", "{prefixet}:"); });
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
		var system = new SystemUnderInfraTest();

		Assert.Throws<ArgumentException>(() =>
		{
			system.RedisConfigurationVerifier.VerifyConfiguration("localhost", prefix);
		});
	}
}