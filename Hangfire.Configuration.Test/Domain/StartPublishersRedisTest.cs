using System.Linq;
using Hangfire.Pro.Redis;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class StartPublishersRedisTest
{
	[Test]
	public void ShouldPassDefaultStorageOptionsToHangfire()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Active = true, 
			ConnectionString = "redis"
		});

		system.PublisherStarter.Start();

		var options = new RedisStorageOptions();
		var storage = system.Hangfire.CreatedStorages.Single();
		Assert.AreEqual(options.Prefix, storage.RedisOptions.Prefix);
		Assert.AreEqual(options.MultiplexerPoolSize, storage.RedisOptions.MultiplexerPoolSize);
		Assert.AreEqual(options.Database, storage.RedisOptions.Database);
		Assert.AreEqual(options.InvisibilityTimeout, storage.RedisOptions.InvisibilityTimeout);
		Assert.AreEqual(options.MaxSucceededListLength, storage.RedisOptions.MaxSucceededListLength);
		Assert.AreEqual(options.MaxDeletedListLength, storage.RedisOptions.MaxDeletedListLength);
		Assert.AreEqual(options.MaxStateHistoryLength, storage.RedisOptions.MaxStateHistoryLength);
		Assert.AreEqual(options.CheckCertificateRevocation, storage.RedisOptions.CheckCertificateRevocation);
	}
}