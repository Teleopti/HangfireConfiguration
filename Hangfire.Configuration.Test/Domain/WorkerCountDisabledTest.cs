using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class WorkerCountDisabledTest
{
	[Test]
	[Ignore("WIP")]
	public void ShouldGetHangfireDefault()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			//GoalWorkerCountEnabled = false,
			Active = true
		});
		
		system.StartWorkerServer();
		
		var hangfireDefault = new BackgroundJobServerOptions().WorkerCount;
		system.Hangfire.StartedServers.Single().options.WorkerCount
			.Should().Be(hangfireDefault);
	}
}