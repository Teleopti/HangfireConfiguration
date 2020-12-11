using System;
using System.Net.Http;
using Hangfire.Configuration.Test.Web;
#if !NET472
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder.Internal;
#else
using Microsoft.Owin.Testing;
using Microsoft.Owin.Builder;
#endif

namespace Hangfire.Configuration.Test
{
	public class ServerUnderTest : IDisposable
	{
		private readonly string _test;
		private readonly TestServer _server;
		private readonly HttpClient _client;

		public ServerUnderTest(CompositionRoot compositionRoot, string urlPathMatch = null, string test = null)
		{
			_test = test;
			TestLog.WriteLine(test + "/ServerUnderTest/1");

			_server =
#if !NET472
				new TestServer(new WebHostBuilder().Configure(app =>
#else
				TestServer.Create(app =>
#endif
					{
						TestLog.WriteLine(test + "/ServerUnderTest/2");
						var url = urlPathMatch ?? "/config";
						app.Properties.Add("CompositionRoot", compositionRoot);
						app.UseHangfireConfigurationUI(url, compositionRoot.BuildOptions().ConfigurationOptions());
					})
#if !NET472
				);
#else
				;
#endif

			TestLog.WriteLine(test + "/ServerUnderTest/3");
#if !NET472
			_client = _server.CreateClient();
#else
		    _client = _server.HttpClient;
#endif
		}

		public HttpClient TestClient => _client;

		public void Dispose()
		{
			TestLog.WriteLine(_test + "/ServerUnderTest/4");
			_client.Dispose();
			_server.Dispose();
			TestLog.WriteLine(_test + "/ServerUnderTest/5");
		}
	}
}