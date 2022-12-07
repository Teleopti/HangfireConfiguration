using System.Linq;
using Autofac;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class JobActivatorTest
{
	[Test]
	public void ShouldUseActivatorFromServerOptions()
	{
		var system = new SystemUnderTest();

		var activator = new AutofacJobActivator(new ContainerBuilder().Build());
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Activator = activator
		});

		system.ConfigurationStorage.Has(new StoredConfiguration {Name = "Hangfire"});
		system.WorkerServerStarter.Start();

		system.Hangfire.StartedServers.Single().options.Activator
			.Should().Be.SameInstanceAs(activator);
	}
}