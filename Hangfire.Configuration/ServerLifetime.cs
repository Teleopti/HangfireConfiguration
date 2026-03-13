using System;
using Hangfire.Server;
#if NET472
using System.Threading;
using Owin;
using Microsoft.Owin;
#else
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Hangfire.Configuration;

internal sealed class ServerLifetime : IDisposable
{
	private readonly IBackgroundProcessingServer[] _servers;

	internal ServerLifetime(IBackgroundProcessingServer[] servers)
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
	
	internal static void HookApplicationLifetime(object appBuilder, ServerLifetime servers)
	{
		if (appBuilder == null)
			return;
#if NET472
		var context = new OwinContext(((IAppBuilder) appBuilder).Properties);
		var token = context.Get<CancellationToken>("host.OnAppDisposing");
		if (token == default)
			token = context.Get<CancellationToken>("server.OnDispose");
		token.Register(servers.Dispose);
#else
		var lifetime = ((IApplicationBuilder) appBuilder).ApplicationServices?.GetRequiredService<IApplicationLifetime>();
		if (lifetime == null)
			return;
		lifetime.ApplicationStopping.Register(servers.SendStop);
		lifetime.ApplicationStopped.Register(servers.Dispose);
#endif
	}

}
