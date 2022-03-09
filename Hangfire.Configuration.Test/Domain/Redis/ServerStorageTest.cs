using System.Linq;
using Hangfire.Pro.Redis;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain.Redis
{
	public class ServerStorageTest
	{
		[Test]
		public void ShouldUseRedisOptions()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "redis$$something"});
			system.WorkerServerStarter.Start();
			
			Assert.NotNull(system.Hangfire.StartedServers.Single().storage.RedisOptions);
		}

		[Test]
		public void ShouldAdjustConnectionString()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "redis$$connstring"});
			system.WorkerServerStarter.Start();
			
			Assert.AreEqual("connstring", system.Hangfire.StartedServers.Single().storage.ConnectionString);
		}

		[Test]
		public void ShouldUseProvidedRedisOptions()
		{
			var options = new RedisStorageOptions();
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "redis$$Foo"});
			system.Options.UseStorageOptions(options);
			system.WorkerServerStarter.Start();
			
			system.Hangfire.StartedServers.Single().storage.RedisOptions
				.Should().Be.SameInstanceAs(options);
		}
	}
}