using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class WorkerBalancerDisabledTest
{
	[Test]
	public void ShouldGetHangfireDefault()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			WorkerBalancerEnabled = false,
			Active = true
		});
		
		system.StartWorkerServer();
		
		var hangfireDefault = new BackgroundJobServerOptions().WorkerCount;
		system.Hangfire.StartedServers.Single().options.WorkerCount
			.Should().Be(hangfireDefault);
	}
}