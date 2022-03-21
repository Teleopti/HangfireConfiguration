using StackExchange.Redis;

namespace Hangfire.Configuration.Internals;

internal class RedisConfigurationVerifier : IRedisConfigurationVerifier
{
	public void VerifyConfiguration(string configuration)
	{
		ConnectionMultiplexer.Connect(configuration);
	}
}