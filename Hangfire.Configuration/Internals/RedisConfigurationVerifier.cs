using StackExchange.Redis;

namespace Hangfire.Configuration.Internals;

internal class RedisConfigurationVerifier : IRedisConfigurationVerifier
{
	public void VerifyConfiguration(string configuration, string prefix)
	{
		ConnectionMultiplexer.Connect(configuration);
	}
}