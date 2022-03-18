using StackExchange.Redis;

namespace Hangfire.Configuration.Internals;

internal class RedisConfigurationVerifier : IRedisConfigurationVerifier
{
	public void TryConnect(string configuration)
	{
		ConnectionMultiplexer.Connect(configuration);
	}
}