namespace Hangfire.Configuration.Internals;

internal class RedisServerConfigurationCreator
{
	private readonly IConfigurationStorage _storage;
	private readonly ITryConnectToRedis _tryConnectToRedis;

	public RedisServerConfigurationCreator(IConfigurationStorage storage, ITryConnectToRedis tryConnectToRedis)
	{
		_storage = storage;
		_tryConnectToRedis = tryConnectToRedis;
	}

	public void Create(CreateRedisWorkerServer command)
	{
		_tryConnectToRedis.TryConnect(command.Configuration);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			SchemaName = command.Prefix ?? DefaultSchemaName.Redis(),
			ConnectionString = command.Configuration,
			Name = command.Name,
			Active = false
		});
	}
}