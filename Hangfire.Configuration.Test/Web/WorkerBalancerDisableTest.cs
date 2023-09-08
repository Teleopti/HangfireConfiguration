using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Web;

[Parallelizable(ParallelScope.None)]
public class WorkerBalancerDisableTest
{
    [Test]
    public void ShouldDisable()
    {
        var system = new SystemUnderTest();
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 2
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/disableWorkerBalancer",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("configurationId", "2"),
            })
        ).Wait();

        system.ConfigurationStorage.Data.Single().WorkerBalancerEnabled
            .Should().Be(false);
    }

    [Test]
    public void ShouldEnable()
    {
        var system = new SystemUnderTest();
        system.ConfigurationStorage.Has(new StoredConfiguration
        {
            Id = 2
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/enableWorkerBalancer",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("configurationId", "2"),
            })
        ).Wait();

        system.ConfigurationStorage.Data.Single().WorkerBalancerEnabled
            .Should().Be(true);
    }
}