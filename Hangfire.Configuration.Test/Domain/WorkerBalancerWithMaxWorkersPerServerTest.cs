using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class WorkerBalancerWithMaxWorkersPerServerTest
{
	[Test]
	public void ShouldUseMaxWorkersPerServer()
	{
		var system = new SystemUnderTest();
		system
			.WithGoalWorkerCount(10)
			.WithMaxWorkersPerServer(2);
            
		system.StartWorkerServer();

		Assert.AreEqual(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}
        
	[Test]
	public void ShouldUseMinimumWhenMaxIsLess()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions
		{
			WorkerBalancerOptions =
			{
				MinimumWorkerCount = 2
			}
		});
		system
			.WithGoalWorkerCount(10)
			.WithMaxWorkersPerServer(1);
            
		system.StartWorkerServer();

		Assert.AreEqual(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}
}