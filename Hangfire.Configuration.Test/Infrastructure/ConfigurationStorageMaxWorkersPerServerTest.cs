using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
	[Collection("NotParallel")]
	public class ConfigurationStorageMaxWorkersPerServerTest
	{
		[Fact, CleanDatabase]
		public void ShouldWriteMaxWorkersPerServer()
		{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});

			system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {MaxWorkersPerServer = 5});

			Assert.Equal(5, system.ConfigurationStorage.ReadConfigurations().Single().MaxWorkersPerServer);
		}
		
		[Fact, CleanDatabase]
		public void ShouldUpdateMaxWorkersPerServer()
		{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});
			system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {MaxWorkersPerServer = 5});
			var existing = system.ConfigurationStorage.ReadConfigurations().Single();

			existing.MaxWorkersPerServer = 3;
			system.ConfigurationStorage.WriteConfiguration(existing);
			
			Assert.Equal(3, system.ConfigurationStorage.ReadConfigurations().Single().MaxWorkersPerServer);
		}
	}
}