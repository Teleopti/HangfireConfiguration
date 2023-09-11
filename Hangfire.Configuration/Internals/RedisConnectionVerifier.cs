using System;
using System.Linq;
using System.Text.RegularExpressions;
#if Redis
using StackExchange.Redis;
#endif

namespace Hangfire.Configuration.Internals;

internal class RedisConnectionVerifier : IRedisConnectionVerifier
{
	public void VerifyConfiguration(string configuration, string prefix)
	{
#if Redis
		using var redis = ConnectionMultiplexer.Connect(configuration + ",allowAdmin=true");
		foreach (var endPoint in redis.GetEndPoints())
		{
			// no coverage, not applicable to our test redis server
			var memPolicy = redis.GetServer(endPoint).Info("memory")[0].SingleOrDefault(x => x.Key == "maxmemory_policy");
			if (memPolicy.Value != null && memPolicy.Value != "noeviction")
				throw new ArgumentException($"maxmemory_policy must be set to 'noeviction' (but was {memPolicy.Value})!");
			//
		}

		if (!redis.GetDatabase().StringSet(prefix + "dbmarker", "keep", null, When.NotExists))
			throw new ArgumentException($"Prefix '{prefix}' already in use!");
#endif
	}
}
