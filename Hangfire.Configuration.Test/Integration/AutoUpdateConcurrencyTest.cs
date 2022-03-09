using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration
{
	[Parallelizable(ParallelScope.None)]
	[CleanDatabase]
	public class AutoUpdateConcurrencyTest
	{
		[Test]
		public void ShouldNotInsertMultiple()
		{
			Parallel.ForEach(Enumerable.Range(1, 1), (item) =>
			{
			var system = new SystemUnderInfraTest();
			system.WithOptions(new ConfigurationOptions
			{
				ConnectionString = ConnectionUtils.GetConnectionString(),
				UpdateConfigurations = new []
				{
					new UpdateStorageConfiguration
					{
						ConnectionString = ConnectionUtils.GetConnectionString(),
						Name = DefaultConfigurationName.Name()
					}
				}
			});
			system
				.BuildWorkerServerStarter(null)
				.Start(null);
			});

			new ConfigurationStorage(ConnectionUtils.GetConnectionString()).ReadConfigurations()
				.Should().Have.Count.EqualTo(1);
		}
	}
}