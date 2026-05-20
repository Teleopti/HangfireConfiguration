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

		// First pass: check for changes against the caller's snapshot, without
		// transaction/lock and without re-reading storage. Re-reading here would open
		// a race where a concurrent writer fixes storage between the caller's read and
		// ours, leading us to report "no changes" against a snapshot the caller never
		// sees. The transactional second pass below still re-reads under the lock for
		// the actual write decision.
		var changes = createChanges(stored);
		performUpdates(options, changes);

		if (!changes.Any(x => x.Changed))
			return false;

		// Second pass: apply changes with transaction/lock
		_storage.Transaction(() =>
		{
			_storage.LockConfiguration();

			var configurations = _storage.ReadConfigurations();
			changes = createChanges(configurations);
			performUpdates(options, changes);

			changes
				.Where(x => x.Changed)
				.ForEach(x => _storage.WriteConfiguration(x.Configuration));
		});

		// The first pass detected that the caller's snapshot is out of sync with
		// storage. The transactional second pass may have found nothing left to write
		// because another writer (e.g. another pod during a rolling deploy) committed
		// an equivalent fix between the two passes — but the caller's snapshot is
		// still stale either way and the caller must re-read.
		return true;
	}

	private List<ConfigurationChange> createChanges(IEnumerable<StoredConfiguration> configurations) =>
		configurations
			.Select(x => new ConfigurationChange
			{
				Configuration = x,
				Changed = false
			})
			.ToList();

	private void performUpdates(ConfigurationOptions options, List<ConfigurationChange> changes)
	{
		updateLegacyDefaultValues(changes);
		updateShutdown(changes);
		updateExternalConfigurations(options, changes);
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

	private void updateExternalConfigurations(ConfigurationOptions options, IList<ConfigurationChange> configurations)
	{
		var haveExternal = options.ExternalConfigurations?.Any() ?? false;
		if (!haveExternal)
			return;

		options.ExternalConfigurations.ForEach(x =>
		{
			var configuration = configurations.FirstOrDefault(c => c.Configuration.Name == x.Name);
			if (configuration == null)
			{
				configuration = new ConfigurationChange
				{
					Configuration = new StoredConfiguration
					{
						Name = x.Name,
						Active = true,
						Containers =
						[
							new ContainerConfiguration
							{
								Tag = DefaultContainerTag.Tag(),
								WorkerBalancerEnabled = x.ConnectionString.GetProvider().WorkerBalancerEnabledDefault()
							}
						]
					},
					Changed = true
				};
				configurations.Add(configuration);
			}

			if (configuration.Configuration.ConnectionString != x.ConnectionString)
			{
				configuration.Configuration.ConnectionString = x.ConnectionString;
				configuration.Changed = true;
			}

			if (configuration.Configuration.SchemaName != x.SchemaName)
			{
				configuration.Configuration.SchemaName = x.SchemaName;
				configuration.Changed = true;
			}
		});
	}
}