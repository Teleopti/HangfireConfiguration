using System.Linq;
using NUnit.Framework;
using static Hangfire.Configuration.Test.Extensions;

namespace Hangfire.Configuration.Test.Web;

[Parallelizable(ParallelScope.None)]
public class MaxWorkersPerServerTest
{
    [Test]
    public void ShouldSave()
    {
        var system = new SystemUnderTest();
        system.WithConfiguration(new StoredConfiguration
        {
            Id = 1,
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/saveMaxWorkersPerServer",
            FormContent(new
            {
                configurationId = 1,
                maxWorkers = 5
            })
        ).Wait();

        Assert.AreEqual(5, system.Configurations().Single().Containers.Single().MaxWorkersPerServer);
    }

    [Test]
    public void ShouldSaveEmpty()
    {
        var system = new SystemUnderTest();
        system.WithConfiguration(new StoredConfiguration
        {
            Id = 1,
            Containers = new[]
            {
                new ContainerConfiguration
                {
                    MaxWorkersPerServer = 4
                }
            }
        });

        using var s = new WebServerUnderTest(system);
        s.TestClient.PostAsync(
            "/config/saveMaxWorkersPerServer",
            FormContent(new
            {
                configurationId = 1,
                maxWorkers = ""
            })
        ).Wait();

        Assert.Null(system.Configurations().Single().Containers.Single().MaxWorkersPerServer);
    }
}