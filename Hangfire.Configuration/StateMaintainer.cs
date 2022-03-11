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

					return makeJobStorage(c);
				}).ToArray();
		}
	}

	private ConfigurationAndStorage makeJobStorage(StoredConfiguration configuration)
	{
		var options = getStorageOptions(configuration);
		options = copyOptions(options);
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
				() => _state.StorageOptionsSqlServer ?? new SqlServerStorageOptions(),
				() => _state.StorageOptionsPostgreSql ?? new PostgreSqlStorageOptions(),
				() => _state.StorageOptionsRedis ?? new RedisStorageOptions()
			);

	private static void assignSchemaName(string schemaName, object options)
	{
		if (string.IsNullOrEmpty(schemaName))
			schemaName = DefaultSchemaName.For(options);

		(options switch
		{
			SqlServerStorageOptions => options.GetType().GetProperty("SchemaName"),
			PostgreSqlStorageOptions => options.GetType().GetProperty("SchemaName"),
			RedisStorageOptions => options.GetType().GetProperty("Prefix"),
			_ => null
		})?.SetValue(options, schemaName);
	}

	private static object copyOptions(object options)
	{
		if (options == null)
			return null;
		return options switch
		{
			SqlServerStorageOptions => JsonConvert.DeserializeObject<SqlServerStorageOptions>(JsonConvert.SerializeObject(options)),
			PostgreSqlStorageOptions => JsonConvert.DeserializeObject<PostgreSqlStorageOptions>(JsonConvert.SerializeObject(options)),
			RedisStorageOptions => JsonConvert.DeserializeObject<RedisStorageOptions>(JsonConvert.SerializeObject(options)),
			_ => null
		};
	}
}