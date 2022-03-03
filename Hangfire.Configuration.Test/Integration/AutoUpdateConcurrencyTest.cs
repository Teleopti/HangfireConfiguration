using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hangfire.Configuration.Test.Integration
{
	[Collection("NotParallel")]
	public class AutoUpdateConcurrencyTest
	{
		[Fact, CleanDatabase]
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
					new UpdateStorage
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

			Assert.Single(new ConfigurationStorage(ConnectionUtils.GetConnectionString()).ReadConfigurations());
		}
	}
}