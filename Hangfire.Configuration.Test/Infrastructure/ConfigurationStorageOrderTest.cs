using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class ConfigurationStorageOrderTest : DatabaseTest
	{
		public ConfigurationStorageOrderTest(string connectionString) : base(connectionString)
		{
		}

		[Test]
		public void ShouldReadConfigurationsInOrderOfId()
		{
			var system = new SystemUnderInfraTest();
			system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
			var storage = system.ConfigurationStorage;

			storage.WriteConfiguration(new StoredConfiguration {ConnectionString = "1",});
			storage.WriteConfiguration(new StoredConfiguration {ConnectionString = "2",});
			storage.WriteConfiguration(new StoredConfiguration {ConnectionString = "3",});

			system.ConfigurationApi().ActivateServer(2);
			
			var read = storage.ReadConfigurations();
			read.ElementAt(0).Id.Should().Be(1);
			read.ElementAt(1).Id.Should().Be(2);
			read.ElementAt(2).Id.Should().Be(3);
		}
	}
}