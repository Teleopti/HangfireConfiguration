namespace Hangfire.Configuration.Internals;

public interface ITryConnectToRedis
{
	void TryConnect(string configuration);
}