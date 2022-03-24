using Hangfire.Configuration.Test.Domain.Fake;

namespace Hangfire.Configuration.Test
{
	public class SystemUnderInfraTest : HangfireConfiguration
	{
		public IKeyValueStore KeyValueStore => base.BuildKeyValueStore();
		public IConfigurationStorage ConfigurationStorage => base.BuildConfigurationStorage();
		public IRedisConfigurationVerifier RedisConfigurationVerifier => base.BuildRedisConfigurationVerifier();

		public void UseRealHangfire() => _realHangfire = true;
		private bool _realHangfire;

		protected override IHangfire BuildHangfire(object appBuilder)
		{
			if (_realHangfire)
				return base.BuildHangfire(appBuilder);
			return new FakeHangfire(null, new FakeMonitoringApi());
		}

		public void UseOptions(ConfigurationOptions configurationOptions) =>
			BuildOptions().UseOptions(configurationOptions);
	}
}