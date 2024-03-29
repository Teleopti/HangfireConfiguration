using System.Linq;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration.Internals;

internal class StateMaintainer
{
	private readonly IHangfire _hangfire;
	private readonly IConfigurationStorage _storage;
	private readonly ConfigurationUpdater _configurationUpdater;
	private readonly State _state;

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

	public void Refresh() =>
		refresh();

	public void EnsureLoaded(string connectionString, string schemaName)
	{
		var exists = _state.Configurations.Any(x => x.Matches(connectionString, schemaName));
		if (!exists)
		{
			lock (_state.Lock)
			{
				exists = _state.Configurations.Any(x => x.Matches(connectionString, schemaName));
				if (!exists)
				{
					_state.Configurations = _state.Configurations
						.Append(buildConfigurationState(null, connectionString, schemaName))
						.ToArray();
				}
				
			}
		}

		refresh();
	}

	private void refresh()
	{
		var options = _state.ReadOptions();
		if (string.IsNullOrEmpty(options.ConnectionString))
			return;

		var configurations = _storage.ReadConfigurations();
		var configurationChanged = _configurationUpdater.Update(options, configurations);
		if (configurationChanged)
			configurations = _storage.ReadConfigurations();

		lock (_state.Lock)
		{
			configurations.ForEach(c =>
			{
				var existing = _state.Configurations.SingleOrDefault(x => x.Matches(c));
				if (existing != null)
				{
					existing.Configuration = c;
					return;
				}

				_state.Configurations = _state.Configurations
					.Append(buildConfigurationState(c))
					.ToArray();
			});

			var toRemove = _state.Configurations
				.Where(x => x.Configuration?.Id.HasValue ?? false)
				.Where(x => !configurations.Select(x => x.Id).Contains(x.Configuration.Id))
				.ToArray();
			_state.Configurations = _state.Configurations.Except(toRemove).ToArray();
			
			_state.Configurations = _state.Configurations
				.OrderBy(x => x.Configuration?.Id)
				.ToArray();

		}
	}

	private ConfigurationState buildConfigurationState(StoredConfiguration configuration) =>
		buildConfigurationState(
			configuration,
			configuration.ConnectionString,
			configuration.SchemaName
		);

	private ConfigurationState buildConfigurationState(
		StoredConfiguration configuration,
		string connectionString,
		string schemaName)
	{
		var provider = connectionString.GetProvider();
		var options = getStorageOptions(provider);
		assignSchemaName(provider, schemaName, options);
		return new ConfigurationState(
			configuration,
			connectionString,
			schemaName,
			() => _hangfire.MakeJobStorage(connectionString, options)
		);
	}

	private object getStorageOptions(IStorageProvider provider)
	{
		var options = _state
			.StorageOptions
			.LastOrDefault(provider.OptionsIsSuitable);
		return provider.CopyOptions(options) ?? provider.NewOptions();
	}

	private static void assignSchemaName(IStorageProvider provider, string schemaName, object options)
	{
		if (string.IsNullOrEmpty(schemaName))
			schemaName = provider.DefaultSchemaName();
		provider.AssignSchemaName(options, schemaName);
	}
}