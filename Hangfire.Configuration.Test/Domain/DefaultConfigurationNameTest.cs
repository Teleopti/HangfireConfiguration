using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
	public class DefaultConfigurationNameTest
	{
		[Test]
		public void ShouldUpdateLegacyWithDefaultName()
		{
			var system = new SystemUnderTest();
			system.WithConfiguration(new StoredConfiguration {GoalWorkerCount = 3});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.AreEqual(DefaultConfigurationName.Name(), result.Single().Name);
		}

		[Test]
		public void ShouldNotUpdateNamed()
		{
			var system = new SystemUnderTest();
			system.WithConfiguration(new StoredConfiguration {Name = "name"});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.AreEqual("name", result.Single().Name);
		}

		[Test]
		public void ShouldUpdateFirstLegacyWithDefaultName()
		{
			var system = new SystemUnderTest();
			system.WithConfiguration(new StoredConfiguration {Id = 2, GoalWorkerCount = 3});
			system.WithConfiguration(new StoredConfiguration {Id = 1, GoalWorkerCount = 1});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.AreEqual(DefaultConfigurationName.Name(), result.Single(x => x.ConfigurationId == 1).Name);
		}

		[Test]
		public void ShouldUpdateAutoUpdateMarkedWithDefaultName()
		{
			var system = new SystemUnderTest();
			system.WithConfiguration(new StoredConfiguration {ConnectionString = "Application Name=AppName"});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.AreEqual(DefaultConfigurationName.Name(), result.Single().Name);
			Assert.AreEqual("Application Name=AppName", result.Single().ConnectionString);
		}
	}
}