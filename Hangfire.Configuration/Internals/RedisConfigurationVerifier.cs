using System;
using System.Linq;
using StackExchange.Redis;

namespace Hangfire.Configuration.Internals;

internal class RedisConfigurationVerifier : IRedisConfigurationVerifier
{
	public void VerifyConfiguration(string configuration, string prefix)
	{
		var redis = ConnectionMultiplexer.Connect(configuration+",allowAdmin=true");

		foreach (var endPoint in redis.GetEndPoints())
		{
			if(redis.GetServer(endPoint).Keys(pattern: $"{prefix}*").Any())
				throw new ArgumentException($"Prefix '{prefix}' already in use!");
			
			// no coverage, not applicable to our test redis server
			var memPolicy = redis.GetServer(endPoint).Info("memory")[0].SingleOrDefault(x => x.Key=="maxmemory_policy");
			if (memPolicy.Value != null && memPolicy.Value != "noeviction")
				throw new ArgumentException($"maxmemory_policy must be set to 'noeviction' (but was {memPolicy.Value})!");
			//
		}
	}
}