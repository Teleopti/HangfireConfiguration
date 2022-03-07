using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain.Redis
{
	public class ServerStorageTest
	{
		[Fact]
		public void ShouldUseRedisOptions()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "redis$$something"});
			system.WorkerServerStarter.Start();
			
			Assert.NotNull(system.Hangfire.StartedServers.Single().storage.RedisOptions);
		}
	}
}