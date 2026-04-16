using System;
using System.Linq;
using System.Net.Http;
using Hangfire.Dashboard;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;

namespace Hangfire.Configuration.Test
{
	public class WebServerUnderTest : IDisposable
	{
		private readonly IDisposable _server;
		private readonly HttpClient _client;
		private readonly string _urlPathMatch;

		public WebServerUnderTest(HangfireConfiguration hangfireConfiguration, string urlPathMatch = null)
		{
			_urlPathMatch = urlPathMatch ?? "/config";
			
			var (server, client) = createServer(hangfireConfiguration);

			_server = server;
			_client = client;
		}

		private (IDisposable server, HttpClient client) createServer(HangfireConfiguration hangfireConfiguration)
		{
			var server = new HostBuilder()
				.ConfigureWebHost(webHost =>
				{
					webHost.UseTestServer();
					webHost.Configure(app =>
					{
						app.UseHangfireConfiguration(hangfireConfiguration);
						
						app.UseHangfireConfigurationUI(_urlPathMatch);
						
						var dashboardOptions = new DashboardOptions
						{
							Authorization = Enumerable.Empty<IDashboardAuthorizationFilter>(),
							AsyncAuthorization = Enumerable.Empty<IDashboardAsyncAuthorizationFilter>(),
							IgnoreAntiforgeryToken = true
						};
						app.UseDynamicHangfireDashboards("/HangfireDashboard", dashboardOptions);
					});
				})
				.StartAsync()
				.Result;

			return (server, server.GetTestClient());
		}

		public HttpClient TestClient => _client;

		public void Dispose()
		{
			_client?.Dispose();
			_server?.Dispose();
		}
	}
}