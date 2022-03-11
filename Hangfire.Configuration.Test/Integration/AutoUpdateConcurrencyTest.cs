using System.Linq;
using System.Threading.Tasks;
using Hangfire.Configuration.Test.Infrastructure;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration;

[Parallelizable(ParallelScope.None)]
public class AutoUpdateConcurrencyTest : DatabaseTestBase
{
	public AutoUpdateConcurrencyTest(string connectionString) : base(connectionString)
	{
	}

	[Test]
	public void ShouldNotInsertMultiple()
	{
		Parallel.ForEach(Enumerable.Range(1, 10), _ =>
		{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions
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
			system
				.BuildWorkerServerStarter(null)
				.Start();
		});

		new ConfigurationStorage(ConnectionString).ReadConfigurations()
			.Should().Have.Count.EqualTo(1);
	}
}