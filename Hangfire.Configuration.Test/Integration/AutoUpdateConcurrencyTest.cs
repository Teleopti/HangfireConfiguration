using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration
{
	[Parallelizable(ParallelScope.None)]
	public class AutoUpdateConcurrencyTest
	{
		[Test]
		public void ShouldNotInsertMultiple()
		{
			DatabaseTestSetup.Setup(ConnectionStrings.SqlServer);
			Parallel.ForEach(Enumerable.Range(1, 1), (item) =>
			{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions
			{
				ConnectionString = ConnectionStrings.SqlServer,
				UpdateConfigurations = new []
				{
					new UpdateStorageConfiguration
					{
						ConnectionString = ConnectionStrings.SqlServer,
						Name = DefaultConfigurationName.Name()
					}
				}
			});
			system
				.BuildWorkerServerStarter(null)
				.Start(null);
			});

			new ConfigurationStorage(ConnectionStrings.SqlServer).ReadConfigurations()
				.Should().Have.Count.EqualTo(1);
		}
	}
}