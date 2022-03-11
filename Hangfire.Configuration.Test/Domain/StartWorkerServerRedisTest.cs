using System;
using System.Linq;
using Hangfire.Pro.Redis;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
	public class StartWorkerServerRedisTest
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
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "redis$$Foo"});
			var options = new RedisStorageOptions
			{
				MultiplexerPoolSize = 1,
				Database = 42,
				InvisibilityTimeout = TimeSpan.FromMinutes(11),
				Prefix = "theprefix",
				MaxSucceededListLength = 22,
				MaxDeletedListLength = 33,
				MaxStateHistoryLength = 44,
				CheckCertificateRevocation = false
			};
			system.Options.UseStorageOptions(options);

			system.WorkerServerStarter.Start();

			var actual = system.Hangfire.StartedServers.Single().storage.RedisOptions;
			actual.Should().Not.Be.SameInstanceAs(options);
			actual.MultiplexerPoolSize.Should().Be.EqualTo(1);
			actual.Database.Should().Be.EqualTo(42);
			actual.InvisibilityTimeout.Should().Be.EqualTo(TimeSpan.FromMinutes(11));
			actual.Prefix.Should().Be.EqualTo("theprefix");
			actual.MaxSucceededListLength.Should().Be.EqualTo(22);
			actual.MaxDeletedListLength.Should().Be.EqualTo(33);
			actual.MaxStateHistoryLength.Should().Be.EqualTo(44);
			actual.CheckCertificateRevocation.Should().Be.EqualTo(false);
		}
	}
}