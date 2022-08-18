using System;
using Hangfire.Configuration.Providers;

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

#if Redis

	public void Create(CreateRedisWorkerServer command)
	{
		var prefix = command.Prefix ?? new RedisStorageProvider().DefaultSchemaName();
		_redisConfigurationVerifier.VerifyConfiguration(command.Configuration, prefix);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			SchemaName = prefix,
			ConnectionString = command.Configuration,
			Name = command.Name,
			Active = false,
			WorkerBalancerEnabled = false
		});
	}
	
#else
	
	public void Create(CreateRedisWorkerServer command)
	{
		throw new NotImplementedException();
	}
	
#endif
	
}