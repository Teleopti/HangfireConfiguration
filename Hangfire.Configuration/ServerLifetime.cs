using System;
using System.Linq;

namespace Hangfire.Configuration;

internal sealed class ServerLifetime : IDisposable
{
	private readonly BackgroundJobServer[] _servers;

	internal ServerLifetime(BackgroundJobServer[] servers)
	{
		_servers = servers;
	}

	public void SendStop()
	{
		foreach (var server in _servers)
			server.SendStop();
	}

	public void Dispose()
	{
		foreach (var server in _servers)
			server.Dispose();
	}
}
