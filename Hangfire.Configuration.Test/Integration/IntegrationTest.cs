using NUnit.Framework;

namespace Hangfire.Configuration.Test.Integration
{
	[Parallelizable(ParallelScope.None)]
	[CleanDatabase]
	public class IntegrationTest
	{
		[Test]
		public void ShouldStartServerWithWorkers()
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
		}
	}
}