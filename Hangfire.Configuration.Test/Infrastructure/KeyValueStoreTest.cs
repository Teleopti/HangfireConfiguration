using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
	[Collection("NotParallel")]
	public class KeyValueStoreTest
	{
		[Fact, CleanDatabase]
		public void ShouldReadValueMatchingKey()
		{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});
			system.KeyValueStore.Write("firstKey", "1");
			system.KeyValueStore.Write("secondKey", "2");

			var value1 = system.KeyValueStore.Read("firstKey");
			var value2 = system.KeyValueStore.Read("secondKey");

			Assert.Equal("1", value1);
			Assert.Equal("2", value2);
		}
	}
}