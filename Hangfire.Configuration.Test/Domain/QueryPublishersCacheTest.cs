using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class QueryPublishersCacheTest
{
	[Test]
	public void ShouldReturnCachedResult()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});

		var result1 = system.QueryPublishers();
		system.ConfigurationApi().WriteConfiguration(new StoredConfiguration {Active = true});
		var result2 = system.QueryPublishers();

		result1.Single().ConfigurationId
			.Should().Be(result2.Single().ConfigurationId);
	}

	[Test]
	public void ShouldReturnNewResultAfter1Minute()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration {Name = "first", Active = true});

		system.Now("2022-04-01 14:00");
		var firstResult = system.QueryPublishers();
		system.ConfigurationApi().WriteConfiguration(new StoredConfiguration {Name = "new", Active = true});
		system.Now("2022-04-01 14:01");
		var newResult = system.QueryPublishers();

		newResult.ElementAt(0).Name.Should().Be("first");
		newResult.ElementAt(1).Name.Should().Be("new");
	}

	[Test]
	public void ShouldNotQueryConfigurationsWhenReturningCached()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});

		system.QueryPublishers();
		system.ConfigurationApi().WriteConfiguration(new StoredConfiguration {Active = true});
		system.ConfigurationStorage.ReadConfigurationsQueryCount = 0;
		system.QueryPublishers();

		system.ConfigurationStorage.ReadConfigurationsQueryCount.Should().Be(0);
	}

	[Test]
	public void ShouldNotCreateNewStorageReturningCached()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = "first", Active = true});

		system.QueryPublishers();
		system.ConfigurationApi().WriteConfiguration(new StoredConfiguration {ConnectionString = "new", Active = true});
		system.QueryPublishers();

		system.Hangfire.CreatedStorages.Single().ConnectionString.Should().Be("first");
	}

	[Test]
	public void ShouldInvalidateOnActivateServer()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration {Name = "first", Active = true});

		system.QueryPublishers();
		system.ConfigurationApi().WriteConfiguration(new StoredConfiguration {Name = "new", Active = true});
		var newId = system.ConfigurationApi().ReadConfigurations().Max(x => x.Id.Value);
		system.ConfigurationApi().ActivateServer(newId);
		var newResult = system.QueryPublishers();

		newResult.ElementAt(0).Name.Should().Be("first");
		newResult.ElementAt(1).Name.Should().Be("new");
	}
}