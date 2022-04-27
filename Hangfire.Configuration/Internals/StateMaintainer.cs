using System.Linq;
using Hangfire.Configuration.Providers;

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

		var configurations = _storage.ReadConfigurations();
		var configurationChanged = _configurationUpdater.Update(options, configurations);
		if (configurationChanged)
			configurations = _storage.ReadConfigurations();

		lock (_lock)
		{
			_state.Configurations = configurations
				.OrderBy(x => x.Id)
				.Select(c =>
				{
					var existing = _state.Configurations.SingleOrDefault(x => x.Configuration.Id == c.Id);
					if (existing != null)
					{
						existing.Configuration = c;
						return existing;
					}

					return buildConfigurationState(c);
				}).ToArray();
		}
	}

	private ConfigurationState buildConfigurationState(StoredConfiguration configuration)
	{
		var provider = configuration.ConnectionString.GetProvider();
		var options = getStorageOptions(provider);
		assignSchemaName(provider, configuration.SchemaName, options);
		return new ConfigurationState(
			configuration,
			() => _hangfire.MakeJobStorage(configuration.ConnectionString, options)
		);
	}

	private object getStorageOptions(IStorageProvider provider)
	{
		var options = _state
			.StorageOptions
			.SingleOrDefault(provider.OptionsIsSuitable);
		return provider.CopyOptions(options) ?? provider.NewOptions();
	}

	private static void assignSchemaName(IStorageProvider provider, string schemaName, object options)
	{
		if (string.IsNullOrEmpty(schemaName))
			schemaName = provider.DefaultSchemaName();
		provider.AssignSchemaName(options, schemaName);
	}
}