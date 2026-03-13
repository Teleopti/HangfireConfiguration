using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class WorkerBalancerTest
{
	[Test]
	public void ShouldGetDefaultForFirstServer()
	{
		var system = new SystemUnderTest();

		system.WorkerServerStarter.Start(
			new ConfigurationOptions
			{
				UpdateConfigurations = new []
				{
					new UpdateStorageConfiguration
					{
						Name = DefaultConfigurationName.Name()
					}
				}
			});

		Assert.AreEqual(10, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldGetGoalForFirstServer()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(8);

		system.WorkerServerStarter.Start();

		Assert.AreEqual(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldGetOneIfGoalIsZero()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(0);

		system.WorkerServerStarter.Start();

		Assert.AreEqual(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldGetOneIfGoalIsNegative()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(-1);

		system.WorkerServerStarter.Start();

		Assert.AreEqual(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldGetMaxOneHundred()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(101);

		system.WorkerServerStarter.Start();

		Assert.AreEqual(100, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldUseDefaultGoalWorkerCount()
	{
		var system = new SystemUnderTest();

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					Name = DefaultConfigurationName.Name()
				}
			},
			DefaultGoalWorkerCount = 12
		});

		Assert.AreEqual(12, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldUseMinimumWorkerCount()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(0);

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			MinimumWorkerCount = 2,
		});

		Assert.AreEqual(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldUseMaximumGoalWorkerCount()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(202);

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			MaximumGoalWorkerCount = 200,
		});

		Assert.AreEqual(200, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldUseMinimumServerCount()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(15);

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			MinimumServerCount = 3,
		});

		Assert.AreEqual(15 / 3, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldUseMinimumWorkerCountWithMinimumKnownServers()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(7);

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			MinimumServerCount = 2,
			MinimumWorkerCount = 6,
		});

		Assert.AreEqual(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldGetGoalForFirstServerWhenMinimumKnownServersIsZero()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(8);

		system.WorkerServerStarter.Start(
			new ConfigurationOptionsForTest
			{
				MinimumServerCount = 0,
			});
            
		Assert.AreEqual(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}
        
	[Test]
	public void ShouldGetHalfOfDefaultForFirstServer()
	{
		var system = new SystemUnderTest();

		system.WorkerServerStarter.Start(
			new ConfigurationOptionsForTest
			{
				MinimumServerCount = 2,
				UpdateConfigurations = new []
				{
					new UpdateStorageConfiguration
					{
						Name = DefaultConfigurationName.Name()
					}
				}
			});
            
		Assert.AreEqual(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldGetHalfOfGoalForFirstServer()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(8);

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			MinimumServerCount = 2,
		});

		Assert.AreEqual(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldGetHalfOfGoalOnRestartOfSingleServer()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(8);
		system.Monitor.AnnounceServer("restartedServer");

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			MinimumServerCount = 2,
		});

		Assert.AreEqual(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}
        
	[Test]
	public void ShouldGetHalfOfMaxOneHundred()
	{
		var system = new SystemUnderTest();
		system.HasGoalWorkerCount(101);

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			MinimumServerCount = 2,
		});

		Assert.AreEqual(50, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}

	[Test]
	public void ShouldUseDefaultGoalWorkerCountWithMinimumKnownServers()
	{
		var system = new SystemUnderTest();

		system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
		{
			UpdateConfigurations = new []
			{
				new UpdateStorageConfiguration
				{
					Name = DefaultConfigurationName.Name()
				}
			},
			DefaultGoalWorkerCount = 12,
			MinimumServerCount = 2
		});

		Assert.AreEqual(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
	}
        
}