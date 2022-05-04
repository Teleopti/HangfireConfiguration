using Hangfire.Pro.Redis;

namespace Hangfire.Configuration.Test.Domain.Fake;

public static class FakeJobStorageEx
{
	public static RedisStorageOptions RedisOptions(this FakeJobStorage instance)
	{
		return instance.Options as RedisStorageOptions;
	}
}