using System;
using System.Linq;

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
		var schemaName = config.SchemaName ?? DefaultSchemaName.Redis();
		if (_storage.ReadConfigurations().Any(x => string.Equals(schemaName.Trim(), x.SchemaName.Trim(), StringComparison.OrdinalIgnoreCase)))
			throw new ArgumentException($"There is already a configuration with prefix {config.SchemaName}");
		_storage.WriteConfiguration(new StoredConfiguration
		{
			SchemaName = schemaName,
			ConnectionString = config.Server,
			Name = config.Name,
			Active = false
		});
	}
}