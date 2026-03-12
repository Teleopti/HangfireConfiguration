using Hangfire.Configuration.Test.Domain.Fake;

namespace Hangfire.Configuration.Test;

public class SystemUnderInfraTest : HangfireConfiguration
{
	public IKeyValueStore KeyValueStore => BuildKeyValueStore();
	public ConfigurationStorage ConfigurationStorage => BuildConfigurationStorage();
	public IRedisConnectionVerifier RedisConnectionVerifier => BuildRedisConnectionVerifier();

	public void UseRealHangfire() => _realHangfire = true;
	private bool _realHangfire;

	protected override IHangfire BuildHangfire()
	{
		if (_realHangfire)
			return base.BuildHangfire();
		return new FakeHangfire(new FakeMonitoringApi());
	}
}