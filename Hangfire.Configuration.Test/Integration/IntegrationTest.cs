using NUnit.Framework;

namespace Hangfire.Configuration.Test.Integration
{
	[Parallelizable(ParallelScope.None)]
	public class IntegrationTest
	{
		[Test]
		public void ShouldStartServerWithWorkers()
		{
			DatabaseTestSetup.Setup(ConnectionStrings.SqlServer);
			
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
		}
	}
}