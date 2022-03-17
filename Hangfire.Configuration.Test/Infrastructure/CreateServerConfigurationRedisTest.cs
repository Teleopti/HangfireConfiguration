using System;
using System.Diagnostics;
using NUnit.Framework;
using StackExchange.Redis;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
public class CreateServerConfigurationRedisTest
{
	private Process redis;
	
	[SetUp]
	public void Setup()
	{
		redis = Process.Start($"{Environment.GetEnvironmentVariable("USERPROFILE")}/.nuget/packages/redis-64/3.0.503/tools/redis-server.exe");
	}
	
	[TearDown]
	public void Teardown()
	{
		redis.Kill();
	}

	[Test]
	public void ShouldThrowIfUnknownServer()
	{
		var system = new SystemUnderInfraTest();

		Assert.Throws<RedisConnectionException>(() =>
		{
			system.BuildConfigurationApi().CreateServerConfiguration(new CreateRedisWorkerServer
			{
				Configuration = "UnknownServer"
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