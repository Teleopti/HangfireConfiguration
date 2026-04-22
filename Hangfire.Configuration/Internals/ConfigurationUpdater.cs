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

		// this could be optimized to check for changes
		// before opening a transaction and locking everything
		// but ofcourse then changes need to rerun inside the transaction

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
				.ToList();

			updateLegacyDefaultValues(changes);
			updateShutdown(changes);
			updateExternalConfigurations(options, changes);
			updateQueues(changes);

			changes
				.Where(x => x.Changed)
				.ForEach(x =>
				{
					updated = true;
					_storage.WriteConfiguration(x.Configuration);
				});
		});

		return updated;
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
	
	private void updateQueues(List<ConfigurationChange> changes)
	{
		changes.ForEach(x =>
		{
			var containers = x.Configuration.Containers.ToArray();
			x.Configuration.Containers.ForEach(container =>
			{
				container.Queues = _queueCalculator.CalculateAppliedQueues(container, containers);
			});
			x.Changed = true;
		});
	}
}