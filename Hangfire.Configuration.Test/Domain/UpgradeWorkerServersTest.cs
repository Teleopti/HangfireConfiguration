using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class UpgradeWorkerServersTest
{
	[Test]
	public void ShouldUpgradeConfigurationSchema()
	{
		var system = new SystemUnderTest();
		system.Options.UseOptions(new ConfigurationOptions {ConnectionString = "config.conn.string"});
		
		system.ConfigurationApi.UpgradeWorkerServers(new UpgradeWorkerServers());

		system.SchemaInstaller.InstalledHangfireConfigurationSchema
			.Should().Be("config.conn.string");
	}
}