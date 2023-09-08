using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Web;

[Parallelizable(ParallelScope.None)]
public class MaxWorkersPerServerTest
{
    [Test]
    public void ShouldSave()
    {
        var system = new SystemUnderTest();
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 1,
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/saveMaxWorkersPerServer",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("configurationId", "1"),
                new KeyValuePair<string, string>("maxWorkers", "5")
            })
        ).Wait();

        Assert.AreEqual(5, system.ConfigurationStorage.Data.Single().MaxWorkersPerServer);
    }

    [Test]
    public void ShouldSaveEmpty()
    {
        var system = new SystemUnderTest();
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 1,
            MaxWorkersPerServer = 4
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/saveMaxWorkersPerServer",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("configurationId", "1"),
                new KeyValuePair<string, string>("maxWorkers", "")
            })
        ).Wait();

        Assert.Null(system.ConfigurationStorage.Data.Single().MaxWorkersPerServer);
    }
}