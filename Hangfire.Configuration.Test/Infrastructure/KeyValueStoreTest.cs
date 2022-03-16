using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class KeyValueStoreTest : DatabaseTestBase
	{
		public KeyValueStoreTest(string connectionString) : base(connectionString)
		{
		}

		[Test]
		public void ShouldReadValueMatchingKey()
		{
			var system = new SystemUnderInfraTest();
			system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
			system.KeyValueStore.Write("firstKey", "1");
			system.KeyValueStore.Write("secondKey", "2");

			var value1 = system.KeyValueStore.Read("firstKey");
			var value2 = system.KeyValueStore.Read("secondKey");

			Assert.AreEqual("1", value1);
			Assert.AreEqual("2", value2);
		}
	}
}