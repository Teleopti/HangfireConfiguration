using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
	[Parallelizable(ParallelScope.None)]
	[CleanDatabase]
	public class ConfigurationStorageMaxWorkersPerServerTest
	{
		[Test]
		public void ShouldWriteMaxWorkersPerServer()
		{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});

			system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {MaxWorkersPerServer = 5});

			Assert.AreEqual(5, system.ConfigurationStorage.ReadConfigurations().Single().MaxWorkersPerServer);
		}
		
		[Test]
		public void ShouldUpdateMaxWorkersPerServer()
		{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});
			system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {MaxWorkersPerServer = 5});
			var existing = system.ConfigurationStorage.ReadConfigurations().Single();

			existing.MaxWorkersPerServer = 3;
			system.ConfigurationStorage.WriteConfiguration(existing);
			
			Assert.AreEqual(3, system.ConfigurationStorage.ReadConfigurations().Single().MaxWorkersPerServer);
		}
	}
}