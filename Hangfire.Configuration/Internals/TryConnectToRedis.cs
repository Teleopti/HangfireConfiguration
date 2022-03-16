using StackExchange.Redis;

namespace Hangfire.Configuration.Internals;

internal class TryConnectToRedis : ITryConnectToRedis
{
	public void TryConnect(string configuration)
	{
		ConnectionMultiplexer.Connect(configuration);
	}
}