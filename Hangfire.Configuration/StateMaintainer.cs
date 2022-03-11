using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.PostgreSql;
using Hangfire.Pro.Redis;
using Hangfire.SqlServer;

namespace Hangfire.Configuration;

public class StateMaintainer
{
	private readonly IHangfire _hangfire;
	private readonly IConfigurationStorage _storage;
	private readonly ConfigurationUpdater _configurationUpdater;
	private readonly State _state;
	private readonly object _lock = new();

	internal StateMaintainer(IHangfire hangfire, IConfigurationStorage storage, ConfigurationUpdater configurationUpdater, State state)
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

					return makeJobStorage(c);
				}).ToArray();
		}
	}

	private ConfigurationAndStorage makeJobStorage(StoredConfiguration configuration)
	{
		var options = getStorageOptions(configuration);
		assignSchemaName(configuration.SchemaName, options);
		return new ConfigurationAndStorage
		{
			JobStorageCreator = () => _hangfire.MakeJobStorage(configuration.ConnectionString, options),
			Configuration = configuration
		};
	}

	private object getStorageOptions(StoredConfiguration configuration) =>
		configuration.ConnectionString.ToDbVendorSelector()
			.SelectDialect<object>(
				() => _state.StorageOptionsSqlServer.DeepCopy() ?? new SqlServerStorageOptions(),
				() => _state.StorageOptionsPostgreSql.DeepCopy() ?? new PostgreSqlStorageOptions(),
				() => _state.StorageOptionsRedis.DeepCopy() ?? new RedisStorageOptions()
			);

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