namespace Hangfire.Configuration;

public interface IRedisConnectionVerifier
{
	void VerifyConfiguration(string configuration, string prefix);
}