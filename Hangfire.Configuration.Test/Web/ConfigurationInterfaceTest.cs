using System.Net;
using System.Net.Http;
using Hangfire.Configuration.Test.Infrastructure;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Xunit;

namespace Hangfire.Configuration.Test.Web
{
    [Collection("Infrastructure")]
    public class ConfigurationInterfaceTest
    {
        private readonly TestServer _testServer = TestServer.Create(app =>
        {
            app.UseHangfireConfigurationInterface("/config", new HangfireConfigurationInterfaceOptions
            {
                ConnectionString = ConnectionUtils.GetConnectionString()
            });
        });
            
        [Fact, CleanDatabase]
        public void ShouldFindConfigurationInterface()
        {
            using(_testServer)
            {
                var response = _testServer.HttpClient.GetAsync("/config").Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
        
        [Fact, CleanDatabase]
        public void ShouldSaveWorkerGoalCount()
        {
            using(_testServer)
            {
                var response = _testServer.HttpClient.PostAsync(
                        "/config/saveWorkerGoalCount", 
                        new StringContent(JsonConvert.SerializeObject(new {configurationId = 1, workers = 10})))
                        .Result;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
        
        [Fact, CleanDatabase]
        public void ShouldActivateServer()
        {
            using(_testServer)
            {
                var response = _testServer.HttpClient.PostAsync(
                        "/config/activateServer", 
                        new StringContent(JsonConvert.SerializeObject(new {configurationId = 1})))
                        .Result;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
        
        [Fact, CleanDatabase]
        public void ShouldCreateNewServerConfiguration()
        {
            using(_testServer)
            {
                var loginUser = ConnectionUtils.GetLoginUser();
                var loginUserPassword = ConnectionUtils.GetLoginUserPassword();
                
                var response = _testServer.HttpClient.PostAsync(
                        "/config/createNewServerConfiguration", 
                        new StringContent(JsonConvert.SerializeObject(
                            new
                            {
                                server = ".",
                                database = ConnectionUtils.GetDatabaseName(),
                                user = loginUser,
                                password = loginUserPassword,
                                schemaName = "TestSchema",
                                schemaCreatorUser = loginUser,
                                schemaCreatorPassword = loginUserPassword
                            })))
                    .Result;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}