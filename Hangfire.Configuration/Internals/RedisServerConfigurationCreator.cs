namespace Hangfire.Configuration.Internals;

internal class RedisServerConfigurationCreator
{
	private readonly IConfigurationStorage _storage;
	private readonly IRedisConfigurationVerifier _redisConfigurationVerifier;

	public RedisServerConfigurationCreator(IConfigurationStorage storage, IRedisConfigurationVerifier redisConfigurationVerifier)
	{
		_storage = storage;
		_redisConfigurationVerifier = redisConfigurationVerifier;
	}

	public void Create(CreateRedisWorkerServer command)
	{
		var prefix = command.Prefix ?? DefaultSchemaName.Redis();
		_redisConfigurationVerifier.VerifyConfiguration(command.Configuration, prefix);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			SchemaName = prefix,
			ConnectionString = command.Configuration,
			Name = command.Name,
			Active = false
		});
	}
}