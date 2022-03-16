using StackExchange.Redis;

namespace Hangfire.Configuration.Internals;

internal class RedisServerConfigurationCreator
{
	private readonly IConfigurationStorage _storage;

	public RedisServerConfigurationCreator(IConfigurationStorage storage)
	{
		_storage = storage;
	}

	public void Create(CreateRedisWorkerServer command)
	{
		ConnectionMultiplexer.Connect(command.Server);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			SchemaName = command.SchemaName ?? DefaultSchemaName.Redis(),
			ConnectionString = command.Server,
			Name = command.Name,
			Active = false
		});
	}
}