using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration.Internals;

internal class ConfigurationUpdater
{
	private readonly ConfigurationStorage _storage;
	private readonly State _state;
	private readonly INow _now;
	private readonly QueueCalculator _queueCalculator;

	internal ConfigurationUpdater(
		ConfigurationStorage storage,
		State state,
		INow now,
		QueueCalculator queueCalculator)
	{
		_storage = storage;
		_state = state;
		_now = now;
		_queueCalculator = queueCalculator;
	}

	public bool Update(ConfigurationOptions options, IEnumerable<StoredConfiguration> stored)
	{
		if (_state.ConfigurationUpdaterRan && stored.Any())
			return false;

		_state.ConfigurationUpdaterRan = true;

		var external = options.ExternalConfigurations ?? Enumerable.Empty<ExternalConfiguration>();

		if (alreadyUpToDate(external, stored))
			return false;

		var updated = false;
		_storage.Transaction(() =>
		{
			_storage.LockConfiguration();

			var configurations = _storage.ReadConfigurations();
			var changes = configurations
				.Select(x => new ConfigurationChange
				{
					Configuration = x, 
					Changed = false
				})
				.ToArray();

			updateLegacyDefaultValues(changes);
			updateShutdown(changes);
			
			var @fixed = false;
			changes
				.Where(x => x.Changed)
				.ForEach(x =>
				{
					@fixed = true;
					_storage.WriteConfiguration(x.Configuration);
				});

			var isUpdated = false;
			if (haveExternalConfigurations(options))
				isUpdated = updateExternalConfigurations(external);

			updated = @fixed || isUpdated;
		});
		return updated;
	}

	private static bool alreadyUpToDate(
		IEnumerable<ExternalConfiguration> external,
		IEnumerable<StoredConfiguration> stored)
	{
		// always fix stored configurations if no external configuration received
		// why iv no idea right now
		if (!external.Any())
			return false;

		bool notStored(IEnumerable<StoredConfiguration> stored, ExternalConfiguration received) =>
			!stored.Any(s => sameConfiguration(received, s));

		bool sameConfiguration(ExternalConfiguration received, StoredConfiguration stored) =>
			stored.Name == received.Name &&
			stored.SchemaName == received.SchemaName &&
			stored.ConnectionString == received.ConnectionString;

		return !(external.Any(r => notStored(stored, r)));
	}

	private class ConfigurationChange
	{
		public StoredConfiguration Configuration;
		public bool Changed;
	}

	private void updateLegacyDefaultValues(IEnumerable<ConfigurationChange> configurations)
	{
		if (!configurations.Any())
			return;

		// set default values if missing on first
		// tests and possibly very old installations
		var first = configurations.First();
		if (first.Configuration.Name == null)
		{
			first.Configuration.Name = DefaultConfigurationName.Name();
			first.Changed = true;
		}

		if (first.Configuration.Active == null)
		{
			first.Configuration.Active ??= true;
			first.Changed = true;
		}

	}

	private void updateShutdown(IEnumerable<ConfigurationChange> configurations)
	{
		var toShutdown = configurations
			.Where(x => !x.Configuration.IsActive() && x.Configuration.ShutdownAt == null)
			.ToArray();
		foreach (var shutdown in toShutdown)
		{
			shutdown.Configuration.ShutdownAt = _now.UtcDateTime().AddDays(1);
			shutdown.Changed = true;
		}
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
				                    Containers = new[]
				                    {
					                    new ContainerConfiguration
					                    {
						                    Tag = DefaultContainerTag.Tag(),
						                    WorkerBalancerEnabled = update.ConnectionString.GetProvider().WorkerBalancerEnabledDefault()
					                    }
				                    }
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