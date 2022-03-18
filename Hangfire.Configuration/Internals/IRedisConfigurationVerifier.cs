namespace Hangfire.Configuration.Internals;

public interface IRedisConfigurationVerifier
{
	void TryConnect(string configuration);
}