using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class QueryResultTest
{
	[Test]
	public void ShouldReturnSameStorage()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Active = true});

		var storage1 = system.QueryPublishers().Single().JobStorage;
		var storage2 = system.QueryPublishers().Single().JobStorage;

		storage1.Should().Be.SameInstanceAs(storage2);
	}
	
	[Test]
	public void ShouldReturnBackgroundJobClient()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Active = true});

		var publisher = system.QueryPublishers().Single();

		publisher.BackgroundJobClient.Should().Not.Be.Null();
	}

	[Test]
	public void ShouldReturnSameBackgroundJobClient()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration {Active = true});

		var publisher1 = system.QueryPublishers().Single();
		var publisher2 = system.QueryPublishers().Single();

		publisher1.BackgroundJobClient.Should()
			.Be.SameInstanceAs(publisher2.BackgroundJobClient);
	}
}