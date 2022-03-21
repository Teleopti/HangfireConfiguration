using System;
using System.Linq;
using StackExchange.Redis;

namespace Hangfire.Configuration.Internals;

internal class RedisConfigurationVerifier : IRedisConfigurationVerifier
{
	public void VerifyConfiguration(string configuration, string prefix)
	{
		var redis = ConnectionMultiplexer.Connect(configuration);

		if (redis.GetEndPoints().Any(endPoint => redis.GetServer(endPoint).Keys(pattern: $"{prefix}*").Any()))
			throw new ArgumentException($"Prefix '{prefix}' already in use!");
	}
}