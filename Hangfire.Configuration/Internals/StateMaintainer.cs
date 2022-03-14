using System.Linq;
using Hangfire.PostgreSql;
using Hangfire.Pro.Redis;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Internals;

internal class StateMaintainer
{
	private readonly IHangfire _hangfire;
	private readonly IConfigurationStorage _storage;
	private readonly ConfigurationUpdater _configurationUpdater;
	private readonly State _state;
	private readonly object _lock = new();

	internal StateMaintainer(
		IHangfire hangfire,
		IConfigurationStorage storage,
		ConfigurationUpdater configurationUpdater,
		State state)
	{
		_hangfire = hangfire;
		_storage = storage;
		_configurationUpdater = configurationUpdater;
		_state = state;
	}

	public void Refresh()
	{
		var options = _state.ReadOptions();

		// maybe not reload all the time
		var configurations = _storage.ReadConfigurations();
		var configurationChanged = _configurationUpdater.Update(options, configurations);
		if (configurationChanged)
			configurations = _storage.ReadConfigurations();

		lock (_lock)
		{
			_state.Configurations = configurations
				.Select(c =>
				{
					var existing = _state.Configurations.SingleOrDefault(x => x.Configuration.Id == c.Id);
					if (existing != null)
					{
						existing.Configuration = c;
						return existing;
					}

					return makeJobStorage(c, options);
				}).ToArray();
		}
	}

	private ConfigurationAndStorage makeJobStorage(StoredConfiguration configuration, ConfigurationOptions configurationOptions)
	{
		var storageOptions = getStorageOptions(configuration, configurationOptions);
		assignSchemaName(configuration.SchemaName, storageOptions);
		return new ConfigurationAndStorage
		{
			JobStorageCreator = () => _hangfire.MakeJobStorage(configuration.ConnectionString, storageOptions),
			Configuration = configuration
		};
	}

	private object getStorageOptions(StoredConfiguration configuration, ConfigurationOptions options)
	{
		if (options.StorageOptionsFactory != null)
		{
			var made = options.StorageOptionsFactory.Make(configuration);
			if (made is SqlServerStorageOptions sqlServerStorageOptions)
				return sqlServerStorageOptions.DeepCopy();
			if (made is PostgreSqlStorageOptions postgreSqlStorageOptions)
				return postgreSqlStorageOptions.DeepCopy();
			if (made is RedisStorageOptions redisStorageOptions)
				return redisStorageOptions.DeepCopy();
			return null;
		}

		return configuration.ConnectionString.ToDbVendorSelector()
			.SelectDialect<object>(
				() => _state.StorageOptionsSqlServer.DeepCopy() ?? new SqlServerStorageOptions(),
				() => _state.StorageOptionsPostgreSql.DeepCopy() ?? new PostgreSqlStorageOptions(),
				() => _state.StorageOptionsRedis.DeepCopy() ?? new RedisStorageOptions());
	}

	private static void assignSchemaName(string schemaName, object options)
	{
		if (string.IsNullOrEmpty(schemaName))
			schemaName = DefaultSchemaName.For(options);

		switch (options)
		{
			case SqlServerStorageOptions sqlServerStorageOptions:
				sqlServerStorageOptions.SchemaName = schemaName;
				break;
			case PostgreSqlStorageOptions postgreSqlStorageOptions:
				postgreSqlStorageOptions.SchemaName = schemaName;
				break;
			case RedisStorageOptions redisStorageOptions:
				redisStorageOptions.Prefix = schemaName;
				break;
		}
	}
}