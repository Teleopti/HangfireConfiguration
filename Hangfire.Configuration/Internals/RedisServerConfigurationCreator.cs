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
		_redisConfigurationVerifier.VerifyConfiguration(command.Configuration);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			SchemaName = command.Prefix ?? DefaultSchemaName.Redis(),
			ConnectionString = command.Configuration,
			Name = command.Name,
			Active = false
		});
	}
}