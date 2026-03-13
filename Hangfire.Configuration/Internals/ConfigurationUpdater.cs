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
				isUpdated = runConfigurationUpdates(external);
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
			received.ConnectionString?.Replace(".AutoUpdate", "") == stored.ConnectionString?.Replace(".AutoUpdate", "");
		
		return !(external.Any(r => notStored(stored, r)));
	}

	private bool fixExistingConfigurations()
	{
		var configurations = _storage.ReadConfigurations();
		var @fixed = new List<StoredConfiguration>();

		var legacyConfiguration = configurations.FirstOrDefault(isLegacy);
		if (legacyConfiguration != null)
		{
			legacyConfiguration.Name ??= DefaultConfigurationName.Name();
			legacyConfiguration.Active ??= true;
			@fixed.Add(legacyConfiguration);
		}
		else
		{
			var markedConfiguration = configurations.FirstOrDefault(isAutoUpdateMarked);
			if (markedConfiguration != null)
			{
				markedConfiguration.Name = DefaultConfigurationName.Name();
				@fixed.Add(markedConfiguration);
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

		@fixed.Distinct().ForEach(x => { _storage.WriteConfiguration(x); });

		return @fixed.Any();
	}

	private bool runConfigurationUpdates(IEnumerable<ExternalConfiguration> received)
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

			if (update.Name == DefaultConfigurationName.Name())
				configuration.ConnectionString = markConnectionString(update.ConnectionString);
			else
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

	private static bool isLegacy(StoredConfiguration configuration) =>
		configuration.ConnectionString == null;

	private static bool isAutoUpdateMarked(StoredConfiguration configuration)
	{
		var applicationName = configuration.ConnectionString.ApplicationName();
		if (applicationName == null)
			return false;
		return applicationName.EndsWith(".AutoUpdate");
	}

	private static string markConnectionString(string connectionString)
	{
		static bool applicationNameIsNotSet(string connectionString)
		{
			// because builder will return a app name even though the connection string does not have one
			var applicationName = connectionString.ApplicationName();
			return string.IsNullOrEmpty(applicationName) ||
			       applicationName == ".Net SqlClient Data Provider" ||
			       applicationName == "Core .Net SqlClient Data Provider";
		}

		var applicationName = connectionString.ApplicationName();
		if (applicationNameIsNotSet(connectionString))
			applicationName = "Hangfire";
		return connectionString.ChangeApplicationName($"{applicationName}.AutoUpdate");
	}
}