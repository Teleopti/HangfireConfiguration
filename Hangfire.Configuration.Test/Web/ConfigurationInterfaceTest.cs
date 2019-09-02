using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;

namespace Hangfire.Configuration.Test.Web
{
    public class ConfigurationInterfaceTest
    {
        [Fact]
        public void ShouldFindConfigurationInterface()
        {
            var system = new SystemUnderTest();
            var response = system.TestClient.GetAsync("/config").Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void ShouldNotFindConfigurationInterface()
        {
            var system = new SystemUnderTest();
            var response = system.TestClient.GetAsync("/configIncorrect").Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public void ShouldSaveWorkerGoalCount()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 1,
                GoalWorkerCount = 3
            });
            
            var response = system.TestClient.PostAsync(
                    "/config/saveWorkerGoalCount",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        configurationId = 1, 
                        workers = 10
                    })))
                .Result;
            
            Assert.Equal(1, system.Repository.Data.Single().Id);
            Assert.Equal(10, system.Repository.Data.Single().GoalWorkerCount);
        }
        
        [Fact]
        public void ShouldSaveWorkerGoalCountWithEmptyDatabase()
        {
            var system = new SystemUnderTest();
            
            var response = system.TestClient.PostAsync(
                    "/config/saveWorkerGoalCount",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        workers = 10
                    })))
                .Result;
            
            Assert.Equal(1, system.Repository.Data.Single().Id);
            Assert.Equal(10, system.Repository.Data.Single().GoalWorkerCount);
        }

        [Fact]
        public void ShouldActivateServer()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 2
            });
            
            var response = system.TestClient.PostAsync(
                    "/config/activateServer",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        configurationId = 2
                    })))
                .Result;
            
            Assert.True(system.Repository.Data.Single().Active);
        }

        [Fact]
        public void ShouldCreateNewServerConfiguration()
        {
            var system = new SystemUnderTest();
            
            var response = system.TestClient.PostAsync(
                    "/config/createNewServerConfiguration",
                    new StringContent(JsonConvert.SerializeObject(
                        new
                        {
                            server = ".",
                            database = "database",
                            user = "user",
                            password = "password",
                            schemaName = "TestSchema",
                            schemaCreatorUser = "schemaCreatorUser",
                            schemaCreatorPassword = "schemaCreatorPassword"
                        })))
                .Result;

            Assert.Equal(1, system.Repository.Data.Single().Id);
            Assert.Contains("database", system.Repository.Data.Single().ConnectionString);
        }
    }
}