using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration.Internals;

internal class ConfigurationUpdater
{
	private readonly ConfigurationStorage _storage;
	private readonly State _state;
	private readonly INow _now;

	internal ConfigurationUpdater(
		ConfigurationStorage storage,
		State state,
		INow now)
	{
		_storage = storage;
		_state = state;
		_now = now;
	}

	public bool Update(ConfigurationOptions options, IEnumerable<StoredConfiguration> stored)
	{
		if (_state.ConfigurationUpdaterRan && stored.Any())
			return false;

		_state.ConfigurationUpdaterRan = true;

		var external = options.ExternalConfigurations ?? Enumerable.Empty<ExternalConfiguration>()
			.Where(x => x.ConnectionString != null)
			.ToArray();

		if (alreadyUpToDate(external, stored))
			return false;

		var isUpdated = false;
		_storage.Transaction(() =>
		{
			_storage.LockConfiguration();
			var @fixed = fixExistingConfigurations();
			if (haveExternalConfigurations(options))
				isUpdated = updateExternalConfigurations(external);
			isUpdated = @fixed || isUpdated;
		});
		return isUpdated;
	}

	private static bool alreadyUpToDate(
		IEnumerable<ExternalConfiguration> external,
		IEnumerable<StoredConfiguration> stored)
	{
		if (!external.Any())
			return false; //always fix stored configurations if no configuration options received

		bool notStored(IEnumerable<StoredConfiguration> stored, ExternalConfiguration received) =>
			!stored.Any(s => sameConfiguration(received, s));

		bool sameConfiguration(ExternalConfiguration received, StoredConfiguration stored) =>
			stored.Name == received.Name &&
			stored.SchemaName == received.SchemaName &&
			stored.ConnectionString == received.ConnectionString;

		return !(external.Any(r => notStored(stored, r)));
	}

	private bool fixExistingConfigurations()
	{
		var configurations = _storage.ReadConfigurations();
		var @fixed = new List<StoredConfiguration>();

		// pop default values if missing on first
		// tests and possibly very old installations
		var first = configurations.FirstOrDefault();
		if (first != null)
		{
			if (first.Name == null)
			{
				first.Name = DefaultConfigurationName.Name();
				@fixed.Add(first);
			}

			if (first.Active == null)
			{
				first.Active ??= true;
				@fixed.Add(first);
			}
		}

		var shutdown = configurations
			.Where(x => !x.IsActive() && x.ShutdownAt == null)
			.ToArray();
		foreach (var shutdownConfiguration in shutdown)
		{
			shutdownConfiguration.ShutdownAt = _now.UtcDateTime().AddDays(1);
			@fixed.Add(shutdownConfiguration);
		}

		@fixed.Distinct().ForEach(x =>
		{
			//
			_storage.WriteConfiguration(x);
		});

		return @fixed.Any();
	}

	private bool updateExternalConfigurations(IEnumerable<ExternalConfiguration> received)
	{
		var stored = _storage.ReadConfigurations();

		received.ForEach(update =>
		{
			var configuration = stored.FirstOrDefault(c => c.Name == update.Name) ??
			                    new StoredConfiguration
			                    {
				                    Name = update.Name,
				                    Active = true,
				                    WorkerBalancerEnabled = update.ConnectionString.GetProvider().WorkerBalancerEnabledDefault()
			                    };

			configuration.ConnectionString = update.ConnectionString;
			configuration.SchemaName = update.SchemaName;

			_storage.WriteConfiguration(configuration);
		});

		return true;
	}

	private static bool haveExternalConfigurations(ConfigurationOptions options)
	{
		if (options.ExternalConfigurations?.Any() ?? false)
			return true;
		return false;
	}
}