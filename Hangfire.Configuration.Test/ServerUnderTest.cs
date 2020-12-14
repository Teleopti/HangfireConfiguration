using System;
using System.Net.Http;
using Xunit;
#if !NET472
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Owin.Testing;
using Microsoft.Owin.Builder;

#endif

namespace Hangfire.Configuration.Test
{
	public class ServerUnderTest : IDisposable
	{
		private readonly IDisposable _server;
		private readonly HttpClient _client;

		public ServerUnderTest(CompositionRoot compositionRoot, string urlPathMatch = null)
		{
			var (server, client) = createServer(compositionRoot, urlPathMatch);

			_server = server;
			_client = client;
		}

#if !NET472
		private static (IDisposable server, HttpClient client) createServer(CompositionRoot compositionRoot, string urlPathMatch)
		{
			var server = new HostBuilder()
				.ConfigureWebHost(webHost =>
				{
					webHost.UseTestServer();
					webHost.Configure(app =>
					{
						var url = urlPathMatch ?? "/config";
						app.Properties.Add("CompositionRoot", compositionRoot);
						app.UseHangfireConfigurationUI(url, compositionRoot.BuildOptions().ConfigurationOptions());
					});
				})
				.StartAsync()
				.Result;

			return (server, server.GetTestClient());
		}

#else
		private static (IDisposable server, HttpClient client) createServer(CompositionRoot compositionRoot, string urlPathMatch)
		{
			var server = TestServer.Create(app =>
			{
				var url = urlPathMatch ?? "/config";
				app.Properties.Add("CompositionRoot", compositionRoot);
				app.UseHangfireConfigurationUI(url, compositionRoot.BuildOptions().ConfigurationOptions());
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