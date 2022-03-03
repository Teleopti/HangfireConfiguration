using Xunit;

namespace Hangfire.Configuration.Test.Integration
{
	[Collection("NotParallel")]
	public class IntegrationTest
	{
		[Fact, CleanDatabase]
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