using System;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeRedisConfigurationVerifier : IRedisConfigurationVerifier
{
	private bool _throws;
	
	public void VerifyConfiguration(string configuration)
	{
		if (_throws)
			throw new Exception();
		WasSucessfullyVerifiedWith = configuration;
	}

	public void Throws()
	{
		_throws = true;
	}

	public string WasSucessfullyVerifiedWith { get; private set; }
}