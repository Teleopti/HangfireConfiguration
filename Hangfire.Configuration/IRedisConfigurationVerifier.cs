namespace Hangfire.Configuration;

public interface IRedisConfigurationVerifier
{
	void VerifyConfiguration(string configuration);
}