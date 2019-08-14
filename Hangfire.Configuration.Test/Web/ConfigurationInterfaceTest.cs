using System.Net;
using Hangfire.Configuration.Test.Infrastructure;
using Microsoft.Owin.Testing;
using Xunit;

namespace Hangfire.Configuration.Test.Web
{
    [Collection("Infrastructure")]
    public class ConfigurationInterfaceTest
    {
        [Fact, CleanDatabase]
        public void ShouldFindConfigurationInterface()
        {
            using(var server = TestServer.Create(app =>
            {
                app.UseHangfireConfigurationInterface("/config", new HangfireConfigurationInterfaceOptions
                {
                    ConnectionString = ConnectionUtils.GetConnectionString()
                });
            }))
            {
                var response = server.HttpClient.GetAsync("/config").Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}