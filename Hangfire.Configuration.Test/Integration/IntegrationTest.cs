using Hangfire.Configuration.Test.Infrastructure;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Integration;

[Parallelizable(ParallelScope.None)]
public class IntegrationTest : DatabaseTestBase
{
	public IntegrationTest(string connectionString) : base(connectionString)
	{
	}

	[Test]
	public void ShouldStartServerWithWorkers()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = ConnectionString,
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					ConnectionString = ConnectionString,
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.StartWorkerServers();
	}
}