using System;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeRedisConnectionVerifier : IRedisConnectionVerifier
{
	private bool _throws;
	
	public void VerifyConfiguration(string configuration, string prefix)
	{
		if (_throws)
			throw new Exception();
		WasSucessfullyVerifiedWith = (configuration, prefix);
	}

	public void Throws()
	{
		_throws = true;
	}

	public (string, string) WasSucessfullyVerifiedWith { get; private set; }
}