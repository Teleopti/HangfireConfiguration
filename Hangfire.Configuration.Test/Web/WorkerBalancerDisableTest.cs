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
	public async Task ShouldDisable()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Id = 2
		});

		using var s = new WebServerUnderTest(system);
		await s.TestClient.PostAsync(
			"/config/disableWorkerBalancer",
			new StringContent(JsonConvert.SerializeObject(new
			{
				configurationId = 2
			})));

		system.ConfigurationStorage.Data.Single().WorkerBalancerEnabled
			.Should().Be(false);
	}
	[Test]
	public async Task ShouldEnable()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Id = 2
		});

		using var s = new WebServerUnderTest(system);
		await s.TestClient.PostAsync(
			"/config/enableWorkerBalancer",
			new StringContent(JsonConvert.SerializeObject(new
			{
				configurationId = 2
			})));

		system.ConfigurationStorage.Data.Single().WorkerBalancerEnabled
			.Should().Be(true);
	}
}