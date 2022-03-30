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
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "something"});

			system.WorkerServerStarter.Start();

			Assert.NotNull(system.Hangfire.StartedServers.Single().storage.RedisOptions);
		}

		[Test]
		public void ShouldUseProvidedRedisOptions()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "redis"});
			var options = new RedisStorageOptions
			{
				MultiplexerPoolSize = 1,
				Database = 42,
				InvisibilityTimeout = TimeSpan.FromMinutes(11),
				MaxSucceededListLength = 22,
				MaxDeletedListLength = 33,
				MaxStateHistoryLength = 44,
				CheckCertificateRevocation = false
			};
			system.UseStorageOptions(options);

			system.WorkerServerStarter.Start();

			var actual = system.Hangfire.StartedServers.Single().storage.RedisOptions;
			actual.Should().Not.Be.SameInstanceAs(options);
			actual.MultiplexerPoolSize.Should().Be.EqualTo(1);
			actual.Database.Should().Be.EqualTo(42);
			actual.InvisibilityTimeout.Should().Be.EqualTo(TimeSpan.FromMinutes(11));
			actual.MaxSucceededListLength.Should().Be.EqualTo(22);
			actual.MaxDeletedListLength.Should().Be.EqualTo(33);
			actual.MaxStateHistoryLength.Should().Be.EqualTo(44);
			actual.CheckCertificateRevocation.Should().Be.EqualTo(false);
		}
		
		[Test]
		public void ShouldUseSchemaNameFromConfigurationAsPrefix()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Active = true, 
				SchemaName = "myprefix:",
				ConnectionString = "redis-gurka"
			});

			system.PublisherStarter.Start();

			Assert.AreEqual("myprefix:", system.Hangfire.CreatedStorages.Single().RedisOptions.Prefix);
		}
		
		[Test]
		public void ShouldUseDefaultPrefix()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Active = true,
				SchemaName = null, 
				ConnectionString =  "redis-gurka"
			});

			system.WorkerServerStarter.Start();

			Assert.AreEqual(DefaultSchemaName.Redis(),
				system.Hangfire.StartedServers.Single().storage.RedisOptions.Prefix);
		}
	}
}