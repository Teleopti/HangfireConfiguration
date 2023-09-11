using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Web;

[Parallelizable(ParallelScope.None)]
public class ConfigurationInterfaceTest
{
    [Test]
    public void ShouldFindConfigurationInterface()
    {
        using var s = new WebServerUnderTest(new SystemUnderTest(), "/config");
        var response = s.TestClient.GetAsync("/config").Result;
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public void ShouldNotFindConfigurationInterface()
    {
        using var s = new WebServerUnderTest(new SystemUnderTest(), "/config");
        var response = s.TestClient.GetAsync("/configIncorrect").Result;
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public void ShouldSaveWorkerGoalCount()
    {
        var system = new SystemUnderTest();
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 1,
            GoalWorkerCount = 3
        });

        using var s = new WebServerUnderTest(system);
        var response = s.TestClient.PostAsync(
            "/config/saveWorkerGoalCount",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("configurationId", "1"),
                new KeyValuePair<string, string>("workers", "10")
            })
        ).Result;

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, system.ConfigurationStorage.Data.Single().Id);
        Assert.AreEqual(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
    }

    [Test]
    public void ShouldReturn500WithErrorMessageWhenSaveTooManyWorkerGoalCount()
    {
        var system = new SystemUnderTest();
        system.UseOptions(new ConfigurationOptionsForTest {MaximumGoalWorkerCount = 10});
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 1,
            GoalWorkerCount = 3
        });

        using var s = new WebServerUnderTest(system);
        var response = s.TestClient.PostAsync(
            "/config/saveWorkerGoalCount",
            formContent(
                new
                {
                    configurationId = 1,
                    workers = 11
                })
        ).Result;

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        var message = response.Content.ReadAsStringAsync().Result;
        message.Should().Contain("Invalid goal worker count");
    }

    [Test]
    public void ShouldActivateServer()
    {
        var system = new SystemUnderTest();
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 2
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/activateServer",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("configurationId", "2")
            })
        ).Wait();

        Assert.True(system.ConfigurationStorage.Data.Single().Active);
    }

    [Test]
    public void ShouldCreateNewServerConfiguration()
    {
        var system = new SystemUnderTest();

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/createNewServerConfiguration",
            formContent(
                new
                {
                    server = ".",
                    database = "database",
                    user = "user",
                    password = "password",
                    schemaName = "TestSchema",
                    schemaCreatorUser = "schemaCreatorUser",
                    schemaCreatorPassword = "schemaCreatorPassword"
                })
        ).Wait();

        var storedConfiguration = system.ConfigurationStorage.Data.Single();
        Assert.AreEqual(1, storedConfiguration.Id);
        storedConfiguration.ConnectionString.Should().Contain("Data Source=.;Initial Catalog=database");
        storedConfiguration.SchemaName.Should().Be("TestSchema");
    }

    [Test]
    public void ShouldCreateNewServerConfigurationWithName()
    {
        var system = new SystemUnderTest();

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/createNewServerConfiguration",
            formContent(
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
                })
        ).Wait();

        Assert.AreEqual("name", system.ConfigurationStorage.Data.Single().Name);
    }

    [Test]
    public void ShouldNotFindUnknownResource()
    {
        using var s = new WebServerUnderTest(new SystemUnderTest(), "/config");
        var response = s.TestClient.GetAsync("/config/unknown_js").Result;
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public void ShouldNotFindUnknownAction()
    {
        using var s = new WebServerUnderTest(new SystemUnderTest(), "/config");
        var response = s.TestClient.PostAsync("/config/unknownAction", null).Result;
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public void ShouldInactivateServer()
    {
        var system = new SystemUnderTest();
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 17,
            Active = true
        });
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 3,
            Active = true
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/inactivateServer",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("configurationId", "3")
            })
        ).Wait();

        Assert.False(system.ConfigurationStorage.Data.Single(x => x.Id == 3).Active);
    }

    [Test]
    public void ShouldCreateNewServerConfigurationForPostgres()
    {
        var system = new SystemUnderTest();

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/createNewServerConfiguration",
            formContent(
                new
                {
                    server = "localhost",
                    database = "database",
                    user = "user",
                    password = "password",
                    schemaName = "TestSchema",
                    schemaCreatorUser = "schemaCreatorUser",
                    schemaCreatorPassword = "schemaCreatorPassword",
                    databaseProvider = "PostgreSql"
                })
        ).Wait();

        var storedConfiguration = system.ConfigurationStorage.Data.Single();
        Assert.AreEqual(1, storedConfiguration.Id);
        storedConfiguration.ConnectionString.Should().Contain("Host=localhost;Database=database");
        storedConfiguration.SchemaName.Should().Be("TestSchema");
    }

    [Test]
    public void ShouldCreateNewServerConfigurationForRedis()
    {
        var system = new SystemUnderTest();

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/createNewServerConfiguration",
            formContent(
                new
                {
                    server = "gurka",
                    schemaName = "{gurka}:",
                    databaseProvider = "Redis"
                })
        ).Wait();

        var storedConfiguration = system.ConfigurationStorage.Data.Single();
        Assert.AreEqual(1, storedConfiguration.Id);
        storedConfiguration.ConnectionString.Should().Be("gurka");
        storedConfiguration.SchemaName.Should().Be("{gurka}:");
    }

    private FormUrlEncodedContent formContent(object data)
    {
        var properties = data.GetType().GetProperties();
        var keyValues = properties
            .Select(x => new KeyValuePair<string, string>(x.Name, x.GetValue(data).ToString()))
            .ToArray();
        return new FormUrlEncodedContent(keyValues);
    }
}