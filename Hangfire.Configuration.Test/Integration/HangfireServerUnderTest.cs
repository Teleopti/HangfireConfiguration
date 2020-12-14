using System;
using System.Net.Http;
using Hangfire.Server;
#if !NET472
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
#else
using Microsoft.Owin.Testing;
#endif

namespace Hangfire.Configuration.Test.Integration
{
	public class HangfireServerUnderTest : IDisposable
	{
		private readonly IDisposable _server;
		private readonly HttpClient _client;

		public HangfireServerUnderTest()
		{
			var (server, client) = createServer();

			_server = server;
			_client = client;
		}

#if !NET472
		private (IDisposable server, HttpClient client) createServer()
		{
			var server = new HostBuilder()
				.ConfigureWebHost(webHost =>
				{
					webHost.UseTestServer();
					webHost.ConfigureServices(services =>
					{
						services.AddHangfire(x => { });
					});
					webHost.Configure(app =>
					{
						app
							.UseHangfireConfiguration(new ConfigurationOptions
							{
								ConnectionString = ConnectionUtils.GetConnectionString(),
								AutoUpdatedHangfireConnectionString = ConnectionUtils.GetConnectionString(),
								UseWorkerDeterminer = true
							})
							.StartWorkerServers(new IBackgroundProcess[] { })
							;
					});
				})
				.StartAsync()
				.Result;

			return (server, server.GetTestClient());
		}

#else
		private (IDisposable server, HttpClient client) createServer()
		{
			var server = TestServer.Create(app =>
			{
				app
					.UseHangfireConfiguration(new ConfigurationOptions
					{
						ConnectionString = ConnectionUtils.GetConnectionString(),
						AutoUpdatedHangfireConnectionString = ConnectionUtils.GetConnectionString(),
						UseWorkerDeterminer = true
					})
					.StartWorkerServers(new IBackgroundProcess[] { })
					;
			});

			return (server, server.HttpClient);
		}

#endif

		public HttpClient TestClient => _client;

		public void Dispose()
		{
			_client?.Dispose();
			_server?.Dispose();
		}
	}
}