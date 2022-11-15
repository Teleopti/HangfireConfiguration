using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class GetPublisherByConnectionStringTest
{
	[Test]
	public void ShouldGetPublisherByConnectionString()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = null});

		var result1 = system.GetPublisher("connection", "schema");

		var storage = result1.JobStorage as FakeJobStorage;
		storage.ConnectionString.Should().Be("connection");
		storage.RedisOptions().Prefix.Should().Be("schema");
		result1.Name.Should().Be.Null();
		result1.ConfigurationId.Should().Be(null);
		result1.Publisher.Should().Be.True();
		result1.MonitoringApi.Should().Be.OfType<FakeMonitoringApi>();
		result1.BackgroundJobClient.Should().Be.OfType<BackgroundJobClient>();
		result1.RecurringJobManager.Should().Be.InstanceOf<RecurringJobManager>();
	}
	
	[Test]
	public void ShouldGetSameStorageEveryTime()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ""});

		var result1 = system.GetPublisher("connection", "schema");
		var result2 = system.GetPublisher("connection", "schema");

		result1.JobStorage
			.Should().Be.SameInstanceAs(result2.JobStorage);
	}
	
	[Test]
	public void ShouldGetExistingConfigurationWhenAble()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = "connection"});
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Active = true, 
			ConnectionString = "connection",
			SchemaName = "schema"
		});

		var result1 = system.GetPublisher("connection", "schema");
		var result2 = system.QueryPublishers().Single();

		result1.JobStorage
			.Should().Be.SameInstanceAs(result2.JobStorage);
	}
	
	[Test]
	public void ShouldGetExistingConfigurationWhenAble2()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = "connection"});
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Active = true, 
			ConnectionString = "connection",
			SchemaName = "schema"
		});

		var result1 = system.QueryPublishers().Single();
		var result2 = system.GetPublisher("connection", "schema");

		result1.JobStorage
			.Should().Be.SameInstanceAs(result2.JobStorage);
	}
	
	[Test]
	public void ShouldGetExistingConfigurationWhenAble3()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = null});

		var result1 = system.GetPublisher("connection", "schema");
		system.UseOptions(new ConfigurationOptions {ConnectionString = "connection"});
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Id = 3,
			Active = true, 
			ConnectionString = "connection",
			SchemaName = "schema"
		});
		var result2 = system.QueryPublishers().Single();

		result1.JobStorage.Should().Be.SameInstanceAs(result2.JobStorage);
		result2.ConfigurationId.Should().Be(3);
	}
	
	[Test]
	public void ShouldGetSameStorageTogetherWithQuery()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = "connection"});
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Id = 1,
			Active = true, 
			ConnectionString = "connection1",
			SchemaName = "schema1"
		});

		var result1 = system.GetPublisher("connection2", "schema2");
		var result2 = system.QueryPublishers().Single(x => x.ConfigurationId.HasValue);
		var result3 = system.GetPublisher("connection2", "schema2");

		result1.JobStorage.Should().Be.SameInstanceAs(result3.JobStorage);
		result2.ConfigurationId.Should().Be(1);
	}
	
	[Test]
	public void ShouldGetStorageForEachConnection()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ""});

		var result1 = system.GetPublisher("connection1", "schema");
		var result2 = system.GetPublisher("connection2", "schema");

		result1.JobStorage
			.Should().Not.Be.SameInstanceAs(result2.JobStorage);
	}
	
	[Test]
	public void ShouldGetSameStorageWithSameConnection()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ""});

		var result1A = system.GetPublisher("connection1", "schema");
		var result1B = system.GetPublisher("connection1", "schema");
		var result2A = system.GetPublisher("connection2", "schema");
		var result2B = system.GetPublisher("connection2", "schema");

		result1A.JobStorage
			.Should().Be.SameInstanceAs(result1B.JobStorage);
		result2A.JobStorage
			.Should().Be.SameInstanceAs(result2B.JobStorage);
	}
	
	[Test]
	public void ShouldGetSameStorageWithSameConnection2()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ""});

		var result1A = system.GetPublisher("connection1", "schema");
		var result2A = system.GetPublisher("connection2", "schema");
		var result1B = system.GetPublisher("connection1", "schema");
		var result2B = system.GetPublisher("connection2", "schema");

		result1A.JobStorage
			.Should().Be.SameInstanceAs(result1B.JobStorage);
		result2A.JobStorage
			.Should().Be.SameInstanceAs(result2B.JobStorage);
	}
	
	[Test]
	public void ShouldGetStorageForEachSchema()
	{
		var system = new SystemUnderTest();
		system.UseOptions(new ConfigurationOptions {ConnectionString = ""});

		var result1 = system.GetPublisher("connection", "schema1");
		var result2 = system.GetPublisher("connection", "schema2");

		result1.JobStorage
			.Should().Not.Be.SameInstanceAs(result2.JobStorage);
	}

	[Test]
	public void ShouldReturnConnectionStringNSchemaFromQueryPublishers()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "connection",
			SchemaName = "schema",
			Active = true
		});

		var result = system.QueryPublishers();

		result.Single().ConnectionString.Should().Be("connection");
		result.Single().SchemaName.Should().Be("schema");
	}
}