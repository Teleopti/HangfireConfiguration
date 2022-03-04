using Xunit;

namespace Hangfire.Configuration.Test.Domain.Redis
{
	public class ServerStorageTest
	{
		[Fact(Skip = "to be discussed/fixed")]
		public void ShouldStartRedisServer()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "redis$$something"});
			system.WorkerServerStarter.Start();
			
			//Assert.IsType<RedisStorageOptions>(system.Hangfire.StartedServers.Single().storage.Options);
		}
	}
}