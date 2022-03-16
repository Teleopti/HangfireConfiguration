using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class ByPassTryConnectToRedis : ITryConnectToRedis
{
	public void TryConnect(string configuration)
	{
	}
}