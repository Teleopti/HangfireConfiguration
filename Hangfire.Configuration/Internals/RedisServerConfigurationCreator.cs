using System;
using System.Text.RegularExpressions;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration.Internals;

internal class RedisServerConfigurationCreator
{
	private readonly IConfigurationStorage _storage;
	private readonly IRedisConnectionVerifier _redisConnectionVerifier;

	public RedisServerConfigurationCreator(IConfigurationStorage storage, IRedisConnectionVerifier redisConnectionVerifier)
	{
		_storage = storage;
		_redisConnectionVerifier = redisConnectionVerifier;
	}

#if Redis

	public void Create(CreateRedisWorkerServer command)
	{
		var defaultSchemaName = $"{{{new RedisStorageProvider().DefaultSchemaName().TrimEnd(':')}}}:";
		var prefix = command.Prefix ?? defaultSchemaName;
		
		if(!Regex.IsMatch(prefix, @"^\{([^\{\}]+)\}:$"))
			throw new ArgumentException("Prefix must be in the format '{yourPrefix}:'!");

		_redisConnectionVerifier.VerifyConfiguration(command.Configuration, prefix);

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