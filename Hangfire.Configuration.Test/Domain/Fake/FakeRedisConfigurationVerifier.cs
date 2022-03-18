using System;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeRedisConfigurationVerifier : IRedisConfigurationVerifier
{
	private Exception _ex;
	private bool _hasSucceded;
	
	public void TryConnect(string configuration)
	{
		if (_ex != null)
			throw _ex;
		_hasSucceded = true;
	}

	public void Throws(Exception ex)
	{
		_ex = ex;
	}

	public bool HasSucceded()
	{
		return _hasSucceded;
	}
}