using System;
using System.Net.Http;
#if !NET472
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Owin.Testing;
#endif

namespace Hangfire.Configuration.Test
{
	public class ServerUnderTest : IDisposable
	{
		private readonly IDisposable _server;
		private readonly HttpClient _client;
		private readonly string _urlPathMatch;

		public ServerUnderTest(HangfireConfiguration hangfireConfiguration, string urlPathMatch = null)
		{
			_urlPathMatch = urlPathMatch ?? "/config";
			
			var (server, client) = createServer(hangfireConfiguration);

			_server = server;
			_client = client;
		}

#if !NET472
		private (IDisposable server, HttpClient client) createServer(HangfireConfiguration hangfireConfiguration)
		{
			var server = new HostBuilder()
				.ConfigureWebHost(webHost =>
				{
					webHost.UseTestServer();
					webHost.Configure(app =>
					{
						app.Properties.Add("HangfireConfiguration", hangfireConfiguration);
						app.UseHangfireConfigurationUI(_urlPathMatch, hangfireConfiguration.BuildOptions().ConfigurationOptions());
					});
				})
				.StartAsync()
				.Result;

			return (server, server.GetTestClient());
		}

#else
		private (IDisposable server, HttpClient client) createServer(HangfireConfiguration hangfireConfiguration)
		{
			var server = TestServer.Create(app =>
			{
				app.Properties.Add("HangfireConfiguration", hangfireConfiguration);
				app.UseHangfireConfigurationUI(_urlPathMatch, hangfireConfiguration.BuildOptions().ConfigurationOptions());
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