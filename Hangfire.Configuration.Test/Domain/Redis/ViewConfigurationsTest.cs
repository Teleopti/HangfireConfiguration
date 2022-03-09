using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain.Redis;

public class ViewConfigurationsTest
{
	[Test]
	public void ShouldFilterRedisPrefix()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "redis$$connstring"
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		Assert.AreEqual("connstring", result.ConnectionString);
	}
}