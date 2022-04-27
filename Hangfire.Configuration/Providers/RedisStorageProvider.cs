using Hangfire.Pro.Redis;

namespace Hangfire.Configuration.Providers;

public class RedisStorageProvider : IStorageProvider
{
	public object CopyOptions(object options)
	{
		return (options as RedisStorageOptions).DeepCopy();
	}
	
	public object NewOptions()
	{
		return new RedisStorageOptions();
	}

	public bool OptionsIsSuitable(object options)
	{
		return options is RedisStorageOptions;
	}

	public string DefaultSchemaName()
	{
		return new RedisStorageOptions().Prefix;
	}

	public void AssignSchemaName(object options, string schemaName)
	{
		(options as RedisStorageOptions).Prefix = schemaName;
	}

	public JobStorage NewStorage(string connectionString, object options)
	{
		return new RedisStorage(connectionString, (RedisStorageOptions) options);
	}
}