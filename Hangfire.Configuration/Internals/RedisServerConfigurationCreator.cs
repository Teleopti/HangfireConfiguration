using System;
using System.Text.RegularExpressions;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration.Internals;

internal class RedisServerConfigurationCreator(
	ConfigurationStorage storage, 
	IRedisConnectionVerifier redisConnectionVerifier)
{
#if Redis

	public void Create(CreateRedisWorkerServer command)
	{
		var defaultSchemaName = $"{{{new RedisStorageProvider().DefaultSchemaName().TrimEnd(':')}}}:";
		var prefix = command.Prefix ?? defaultSchemaName;
		
		if(!Regex.IsMatch(prefix, @"^\{([^\{\}]+)\}:$"))
			throw new ArgumentException("Prefix must be in the format '{yourPrefix}:'!");

		redisConnectionVerifier.VerifyConfiguration(command.Configuration, prefix);

		storage.WriteConfiguration(new StoredConfiguration
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