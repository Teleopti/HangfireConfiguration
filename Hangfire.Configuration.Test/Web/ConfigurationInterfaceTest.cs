using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using SharpTestsEx;
using static Hangfire.Configuration.Test.Extensions;

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
    public void ShouldActivateServer()
    {
        var system = new SystemUnderTest();
        system.WithConfiguration(new StoredConfiguration
        {
            Id = 2
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/activateServer",
            FormContent(new
            {
                configurationId = 2
            })
        ).Wait();

        Assert.True(system.Configurations().Single().Active);
    }

    [Test]
    public void ShouldCreateNewServerConfiguration()
    {
        var system = new SystemUnderTest();

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/createNewServerConfiguration",
            FormContent(
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

        var storedConfiguration = system.Configurations().Single();
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
            FormContent(
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

        Assert.AreEqual("name", system.Configurations().Single().Name);
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
        system.WithConfiguration(new StoredConfiguration
        {
            Id = 17,
            Active = true
        });
        system.WithConfiguration(new StoredConfiguration
        {
            Id = 3,
            Active = true
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/inactivateServer",
            FormContent(new
            {
                configurationId = 3
            })
        ).Wait();

        Assert.False(system.Configurations().Single(x => x.Id == 3).Active);
    }

    [Test]
    public void ShouldCreateNewServerConfigurationForPostgres()
    {
        var system = new SystemUnderTest();

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/createNewServerConfiguration",
            FormContent(
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

        var storedConfiguration = system.Configurations().Single();
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
            FormContent(
                new
                {
                    server = "gurka",
                    schemaName = "{gurka}:",
                    databaseProvider = "Redis"
                })
        ).Wait();

        var storedConfiguration = system.Configurations().Single();
        Assert.AreEqual(1, storedConfiguration.Id);
        storedConfiguration.ConnectionString.Should().Be("gurka");
        storedConfiguration.SchemaName.Should().Be("{gurka}:");
    }
}