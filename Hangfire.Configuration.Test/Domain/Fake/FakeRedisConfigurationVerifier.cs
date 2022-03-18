namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeRedisConfigurationVerifier : IRedisConfigurationVerifier
{
	private bool _hasBeenCalled;
	
	public void VerifyConfiguration(string configuration)
	{
		_hasBeenCalled = true;
	}

	public bool HasBeenCalled()
	{
		return _hasBeenCalled;
	}
}