using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using static Hangfire.Configuration.Test.Extensions;

namespace Hangfire.Configuration.Test.Web;

[Parallelizable(ParallelScope.None)]
public class WorkerBalancerDisableTest
{
    [Test]
    public void ShouldDisable()
    {
        var system = new SystemUnderTest();
        system.WithConfiguration(new StoredConfiguration
        {
            Id = 2
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/disableWorkerBalancer",
            FormContent(new
            {
                configurationId = 2
            })
        ).Wait();

        system.Configurations().Single().Containers.Single().WorkerBalancerEnabled
            .Should().Be(false);
    }

    [Test]
    public void ShouldEnable()
    {
        var system = new SystemUnderTest();
        system.WithConfiguration(new StoredConfiguration
        {
            Id = 2
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/enableWorkerBalancer",
            FormContent(new
            {
                configurationId = 2
            })
        ).Wait();

        system.Configurations().Single().Containers.Single().WorkerBalancerEnabled
            .Should().Be(true);
    }
}