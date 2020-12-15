using Hangfire.Configuration.Test.Domain.Fake;

namespace Hangfire.Configuration.Test
{
	public class SystemUnderInfraTest : CompositionRoot
	{
		public IKeyValueStore KeyValueStore => base.BuildKeyValueStore();
		public IConfigurationStorage ConfigurationStorage => base.BuildConfigurationStorage();

		protected override IHangfire BuildHangfire(object appBuilder) =>
			new FakeHangfire(null, new FakeMonitoringApi());

		public void WithOptions(ConfigurationOptions configurationOptions)
		{
			BuildOptions().UseOptions(configurationOptions);
		}
	}
}