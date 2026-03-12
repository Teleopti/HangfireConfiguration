using System.Net;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Web;

[Parallelizable(ParallelScope.None)]
public class DynamicHangfireDashboardsTest
{
	[Test]
	public void ShouldFindDashboard()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1
		});
		using var s = new WebServerUnderTest(system);
		
		var response = s.TestClient.GetAsync("/HangfireDashboard/1").Result;
		
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
	}
	
}