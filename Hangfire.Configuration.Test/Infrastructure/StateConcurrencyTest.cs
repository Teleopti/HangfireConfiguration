using System.Linq;
using DbAgnostic;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

public class StateConcurrencyTest(string connectionString) : 
	DatabaseTest(connectionString)
{
	[Test]
	public void ShouldReturnCorrectNumberOfWorkServersConcurrently()
	{
		// 500 takes 5 min on postgres for some reason
		var storageCount = connectionString.PickDialect(500, 10);
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ConnectionString});
		Enumerable.Range(1, storageCount)
			.ForEach(_ => { system.ConfigurationStorage.WriteConfiguration(new StoredConfiguration {ConnectionString = ConnectionString}); });

		var hangfireConfiguration = system.StartWorkerServers();
		var run = new ConcurrencyRunner();
		run.InParallel(() =>
			{
				var actual = hangfireConfiguration.QueryAllWorkerServers().Count();
				Assert.AreEqual(storageCount, actual);
			})
			.Times(100)
			.Wait();
	}
        
	[Test]
	public void ShouldReturnCorrectNumberOfPublishersConcurrently()
	{
		const int publisherCount = 500;
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = null});
		var connectionStrings = Enumerable.Range(1, publisherCount)
			.Select(i => "connection" + i)
			.ToArray();

		var run = new ConcurrencyRunner();
		run.InParallel(() =>
			{
				connectionStrings.ForEach(x =>
				{
					system.GetPublisher(x, "schema");
				});
				var actual = system.QueryPublishers().Count();
				Assert.AreEqual(publisherCount, actual);
			})
			.Times(100)
			.Wait();
	}
}