using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.PostgreSql;
using Hangfire.Pro.Redis;
using Hangfire.SqlServer;
using Newtonsoft.Json;

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

					// THIS IS WRONG! CONFIG = sql DOES NOT MEAN STORAGE = sql!
					// !BOOOOOOOOOOOOOOOOOOOOO!!!!!
					var connectionString = c.ConnectionString ?? options.ConnectionString;

					var storageOptions = connectionString.ToDbVendorSelector()
						.SelectDialect<object>(
							() => _state.StorageOptionsSqlServer ?? new SqlServerStorageOptions(),
							() => _state.StorageOptionsPostgreSql ?? new PostgreSqlStorageOptions(),
							() => _state.StorageOptionsRedis ?? new RedisStorageOptions()
						);

					return makeJobStorage(connectionString, c, storageOptions);
				}).ToArray();
		}
	}

	private ConfigurationAndStorage makeJobStorage(string connectionString, StoredConfiguration configuration, object storageOptions)
	{
		var options = copyOptions(storageOptions);

		if (options is SqlServerStorageOptions o1)
			if (string.IsNullOrEmpty(configuration.SchemaName))
				o1.SchemaName = DefaultSchemaName.SqlServer();
			else
				o1.SchemaName = configuration.SchemaName;

		if (options is PostgreSqlStorageOptions o2)
			if (string.IsNullOrEmpty(configuration.SchemaName))
				o2.SchemaName = DefaultSchemaName.Postgres();
			else
				o2.SchemaName = configuration.SchemaName;

		return new ConfigurationAndStorage
		{
			JobStorageCreator = () => _hangfire.MakeJobStorage(connectionString, options),
			Configuration = configuration
		};
	}

	private static object copyOptions(object options)
	{
		if (options == null)
			return null;
		return options switch
		{
			SqlServerStorageOptions => JsonConvert.DeserializeObject<SqlServerStorageOptions>(JsonConvert.SerializeObject(options)),
			PostgreSqlStorageOptions => JsonConvert.DeserializeObject<PostgreSqlStorageOptions>(JsonConvert.SerializeObject(options)),
			RedisStorageOptions => JsonConvert.DeserializeObject<RedisStorageOptions>(JsonConvert.SerializeObject(options))
		};
	}
}