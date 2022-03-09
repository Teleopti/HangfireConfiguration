namespace Hangfire.Configuration.Internals;

internal class RedisCreateServerConfiguration : ICreateServerConfiguration
{
	private readonly IConfigurationStorage _storage;

	public RedisCreateServerConfiguration(IConfigurationStorage storage)
	{
		_storage = storage;
	}

	public void Create(CreateServerConfiguration config)
	{
		_storage.WriteConfiguration(new StoredConfiguration
		{
			ConnectionString = $"redis$${config.Server}",
			Name = config.Name,
			Active = false
		});
	}
}