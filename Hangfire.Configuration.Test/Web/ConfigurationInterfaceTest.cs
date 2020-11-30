using System.Linq;
using System.Net;
using System.Net.Http;
using Hangfire.Configuration.Test.Domain;
using Hangfire.Configuration.Test.Domain.Fake;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Hangfire.Configuration.Test.Web
{
    public class ConfigurationInterfaceTest
    {
        [Fact]
        public void ShouldFindConfigurationInterface()
        {
            var system = new SystemUnderTest(null, "/config");
            var response = system.TestClient.GetAsync("/config").Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void ShouldNotFindConfigurationInterface()
        {
            var system = new SystemUnderTest(null, "/config");
            var response = system.TestClient.GetAsync("/configIncorrect").Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public void ShouldSaveWorkerGoalCount()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
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
            
            Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
            Assert.Equal(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
        }
        
        [Fact]
        public void ShouldReturn500WithErrorMessageWhenSaveTooManyWorkerGoalCount()
        {
            var system = new SystemUnderTest(new ConfigurationOptionsForTest {MaximumGoalWorkerCount = 10});
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                GoalWorkerCount = 3
            });
            
            var response = system.TestClient.PostAsync(
                    "/config/saveWorkerGoalCount",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        configurationId = 1,
                        workers = 11
                    })))
                .Result;
            
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var message = response.Content.ReadAsStringAsync().Result;
            Assert.NotEmpty(message);
            Assert.DoesNotContain("<", message);
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
            
            Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
            Assert.Equal(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
        }

        [Fact]
        public void ShouldActivateServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
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
            
            Assert.True(system.ConfigurationStorage.Data.Single().Active);
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

            Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
            Assert.Contains("database", system.ConfigurationStorage.Data.Single().ConnectionString);
        }
        
        [Fact]
        public void ShouldCreateNewServerConfigurationWithName()
        {
            var system = new SystemUnderTest();
            
            var response = system.TestClient.PostAsync(
                    "/config/createNewServerConfiguration",
                    new StringContent(JsonConvert.SerializeObject(
                        new
                        {
                            server = ".",
                            name = "name",
                            database = "database",
                            user = "user",
                            password = "password",
                            schemaName = "TestSchema",
                            schemaCreatorUser = "schemaCreatorUser",
                            schemaCreatorPassword = "schemaCreatorPassword"
                        })))
                .Result;

            Assert.Equal("name", system.ConfigurationStorage.Data.Single().Name);
        }
        
        [Fact]
        public void ShouldNotFindUnknownAction()
        {
            var system = new SystemUnderTest(null, "/config");
            var response = system.TestClient.GetAsync("/config/unknownAction").Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public void ShouldInactivateServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 3,
                Active = true
            });

            var response = system.TestClient.PostAsync(
                    "/config/inactivateServer",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        configurationId = 3
                    })))
                .Result;

            Assert.False(system.ConfigurationStorage.Data.Single().Active);
        }
        
    }
}