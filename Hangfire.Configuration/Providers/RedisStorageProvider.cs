#if Redis
using Hangfire.Pro.Redis;

namespace Hangfire.Configuration.Providers;

internal class RedisStorageProvider : IStorageProvider
{
	public object CopyOptions(object options) => (options as RedisStorageOptions).DeepCopy();
	public object NewOptions() => new RedisStorageOptions();
	public bool OptionsIsSuitable(object options) => options is RedisStorageOptions;
	public string DefaultSchemaName() => new RedisStorageOptions().Prefix;
	public void AssignSchemaName(object options, string schemaName) => (options as RedisStorageOptions).Prefix = schemaName;
	public bool WorkerBalancerEnabledDefault() => false;

	public JobStorage NewStorage(string connectionString, object options) =>
		new RedisStorage(connectionString, (RedisStorageOptions) options);
}

#endif