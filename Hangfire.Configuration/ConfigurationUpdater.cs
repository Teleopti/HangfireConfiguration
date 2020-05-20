using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ConfigurationUpdater
    {
        private readonly IConfigurationRepository _repository;
        private readonly State _state;

        internal ConfigurationUpdater(IConfigurationRepository repository,  State state)
        {
            _repository = repository;
            _state = state;
        }

        public bool Update(ConfigurationOptions options, IEnumerable<StoredConfiguration> configs)
        {
            if (_state.ConfigurationUpdaterRan && configs.Any())
                return false;

            _state.ConfigurationUpdaterRan = true;

            if (nothingToUpdate(options, configs))
                return false; 

            var isUpdated = false;
            _repository.UsingTransaction(c =>
            {
                _repository.LockConfiguration(c);
                var @fixed = fixExistingConfigurations(c);
                var updated = runConfigurationUpdates(options,c);
                isUpdated = @fixed || updated;
            });
            return isUpdated;
        }

        private bool nothingToUpdate(ConfigurationOptions options, IEnumerable<StoredConfiguration> storedConfigs)
        {
            if (options == null)
                return false;
            
            var configs = buildUpdateConfigurations(options);
            
            foreach (var config in configs)
            {
                if (!storedConfigs.Any(storedConfig => 
                    storedConfig.Name == config.Name && 
                    storedConfig.SchemaName == config.SchemaName && 
                    config.ConnectionString?.Replace(".AutoUpdate", "") == storedConfig.ConnectionString?.Replace(".AutoUpdate", "")))
                    return false;
            }
            return true;
        }

        private bool runConfigurationUpdates(ConfigurationOptions options,
            IConfigurationConnection connection)
        {
            if (!updateConfigurationsEnabled(options))
                return false;

            var configurations = _repository.ReadConfigurations(connection);

            var updateConfigurations = buildUpdateConfigurations(options);

            updateConfigurations.ForEach(update =>
            {
                var configuration = configurations.FirstOrDefault(c => c.Name == update.Name) ??
                                    new StoredConfiguration
                                    {
                                        Name = update.Name,
                                        Active = true
                                    };
                if (update.Name == DefaultConfigurationName.Name())
                    configuration.ConnectionString = markConnectionString(update.ConnectionString);
                else
                    configuration.ConnectionString = update.ConnectionString;
                configuration.SchemaName = update.SchemaName;
                _repository.WriteConfiguration(configuration,connection);
            });

            return true;
        }

        private static UpdateConfiguration[] buildUpdateConfigurations(ConfigurationOptions options)
        {
            var autoUpdate = new UpdateConfiguration
            {
                Name = DefaultConfigurationName.Name(),
                ConnectionString = options?.AutoUpdatedHangfireConnectionString,
                SchemaName = options?.AutoUpdatedHangfireSchemaName,
            };
            
            return  new[] {autoUpdate}
                .Concat(options?.UpdateConfigurations ?? Enumerable.Empty<UpdateConfiguration>())
                .Where(x => x.ConnectionString != null)
                .ToArray();
        }

        private bool fixExistingConfigurations(IConfigurationConnection connection)
        {
            var configurations = _repository.ReadConfigurations(connection);
            
            var ordered = configurations.OrderBy(x => x.Id).ToArray();

            var legacyConfiguration = ordered.FirstOrDefault(isLegacy);
            if (legacyConfiguration != null)
            {
                if (legacyConfiguration.Name == null)
                    legacyConfiguration.Name = DefaultConfigurationName.Name();
                if (legacyConfiguration.Active == null)
                    legacyConfiguration.Active = true;
                _repository.WriteConfiguration(legacyConfiguration, connection);
                return true;
            }

            var markedConfiguration = ordered.FirstOrDefault(isMarked);
            if (markedConfiguration != null)
            {
                markedConfiguration.Name = DefaultConfigurationName.Name();
                _repository.WriteConfiguration(markedConfiguration, connection);
                return true;
            }

            return false;
        }

        private static bool updateConfigurationsEnabled(ConfigurationOptions options)
        {
            if (options?.AutoUpdatedHangfireConnectionString != null)
                return true;
            if (options?.UpdateConfigurations?.Any() ?? false)
                return true;
            return false;
        }

        private static bool isLegacy(StoredConfiguration configuration) =>
            configuration.ConnectionString == null;

        private static bool isMarked(StoredConfiguration configuration)
        {
            SqlConnectionStringBuilder connectionString;
            try
            {
                connectionString = new SqlConnectionStringBuilder(configuration.ConnectionString);
            }
            catch (ArgumentException e)
            {
                return false;
            }

            return connectionString
                .ApplicationName
                .EndsWith(".AutoUpdate");
        }

        private static string markConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (applicationNameIsNotSet(builder))
                builder.ApplicationName = "Hangfire";
            builder.ApplicationName += ".AutoUpdate";
            return builder.ToString();
        }

        // because builder will return a app name even though the connection string does not have one
        private static bool applicationNameIsNotSet(SqlConnectionStringBuilder builder) =>
            string.IsNullOrEmpty(builder.ApplicationName) ||
            builder.ApplicationName == ".Net SqlClient Data Provider" ||
            builder.ApplicationName == "Core .Net SqlClient Data Provider";
    }
}