using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
	[Parallelizable(ParallelScope.None)]
	[CleanDatabase]
	public class KeyValueStoreTest
	{
		[Test]
		public void ShouldReadValueMatchingKey()
		{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});
			system.KeyValueStore.Write("firstKey", "1");
			system.KeyValueStore.Write("secondKey", "2");

			var value1 = system.KeyValueStore.Read("firstKey");
			var value2 = system.KeyValueStore.Read("secondKey");

			Assert.AreEqual("1", value1);
			Assert.AreEqual("2", value2);
		}
	}
}