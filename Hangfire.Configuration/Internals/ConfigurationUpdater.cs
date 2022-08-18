using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration.Internals
{
	internal class ConfigurationUpdater
	{
		private readonly IConfigurationStorage _storage;
		private readonly State _state;

		internal ConfigurationUpdater(IConfigurationStorage storage, State state)
		{
			_storage = storage;
			_state = state;
		}

		public bool Update(ConfigurationOptions options, IEnumerable<StoredConfiguration> stored)
		{
			if (_state.ConfigurationUpdaterRan && stored.Any())
				return false;

			_state.ConfigurationUpdaterRan = true;

			var received = buildUpdateConfigurations(options);

			if (alreadyUpToDate(received, stored))
				return false;

			var isUpdated = false;
			_storage.Transaction(() =>
			{
				_storage.LockConfiguration();
				var @fixed = fixExistingConfigurations();
				if (updateConfigurationsEnabled(options))
					isUpdated = runConfigurationUpdates(received);
				isUpdated = @fixed || isUpdated;
			});
			return isUpdated;
		}

		private static bool alreadyUpToDate(IEnumerable<UpdateStorageConfiguration> received, IEnumerable<StoredConfiguration> stored)
		{
			if (!received.Any())
				return false; //always fix stored configurations if no configuration options received

			return !(received.Any(r => notStored(stored, r)));
		}

		private static IEnumerable<UpdateStorageConfiguration> buildUpdateConfigurations(ConfigurationOptions options)
		{
			return options.UpdateConfigurations ?? Enumerable.Empty<UpdateStorageConfiguration>()
				.Where(x => x.ConnectionString != null)
				.ToArray();
		}

		private static bool notStored(IEnumerable<StoredConfiguration> stored, UpdateStorageConfiguration received) =>
			!stored.Any(s => sameConfiguration(received, s));

		private static bool sameConfiguration(UpdateStorageConfiguration received, StoredConfiguration stored) =>
			stored.Name == received.Name &&
			stored.SchemaName == received.SchemaName &&
			received.ConnectionString?.Replace(".AutoUpdate", "") == stored.ConnectionString?.Replace(".AutoUpdate", "");

		private bool fixExistingConfigurations()
		{
			var stored = _storage.ReadConfigurations();

			var ordered = stored.OrderBy(x => x.Id).ToArray();

			var legacyConfiguration = ordered.FirstOrDefault(isLegacy);
			if (legacyConfiguration != null)
			{
				legacyConfiguration.Name ??= DefaultConfigurationName.Name();
				legacyConfiguration.Active ??= true;
				_storage.WriteConfiguration(legacyConfiguration);
				return true;
			}

			var markedConfiguration = ordered.FirstOrDefault(isMarked);
			if (markedConfiguration != null)
			{
				markedConfiguration.Name = DefaultConfigurationName.Name();
				_storage.WriteConfiguration(markedConfiguration);
				return true;
			}

			return false;
		}

		private bool runConfigurationUpdates(IEnumerable<UpdateStorageConfiguration> received)
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

		private static bool updateConfigurationsEnabled(ConfigurationOptions options)
		{
			if (options.UpdateConfigurations?.Any() ?? false)
				return true;
			return false;
		}

		private static bool isLegacy(StoredConfiguration configuration) =>
			configuration.ConnectionString == null;

		private static bool isMarked(StoredConfiguration configuration)
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
}