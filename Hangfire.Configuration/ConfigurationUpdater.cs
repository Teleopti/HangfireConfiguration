using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ConfigurationUpdater
    {
        private readonly IConfigurationStorage _storage;
        private readonly State _state;

        internal ConfigurationUpdater(IConfigurationStorage storage,  State state)
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
            _storage.UnitOfWork(c =>
            {
                _storage.LockConfiguration(c);
                var @fixed = fixExistingConfigurations(c);
                if (updateConfigurationsEnabled(options))
                    isUpdated = runConfigurationUpdates(received, c);
                isUpdated = @fixed || isUpdated;
            });
            return isUpdated;
        }

        private static bool alreadyUpToDate(IEnumerable<UpdateConfiguration> received, IEnumerable<StoredConfiguration> stored)
        {
            if (!received.Any())
                return false; //always fix stored configurations if no configuration options received
            
            return !(received.Any(r => notStored(stored, r)));
        }
        
        private static IEnumerable<UpdateConfiguration> buildUpdateConfigurations(ConfigurationOptions options)
        {
            var autoUpdate = new UpdateConfiguration
            {
                Name = DefaultConfigurationName.Name(),
                ConnectionString = options.AutoUpdatedHangfireConnectionString,
                SchemaName = options.AutoUpdatedHangfireSchemaName,
            };
            
            return new[] {autoUpdate}
                .Concat(options.UpdateConfigurations ?? Enumerable.Empty<UpdateConfiguration>())
                .Where(x => x.ConnectionString != null)
                .ToArray();
        }        

        private static bool notStored(IEnumerable<StoredConfiguration> stored, UpdateConfiguration received) => 
            !stored.Any(s => sameConfiguration(received, s));

        private static bool sameConfiguration( UpdateConfiguration received, StoredConfiguration stored) =>
            stored.Name == received.Name && 
            stored.SchemaName == received.SchemaName && 
            received.ConnectionString?.Replace(".AutoUpdate", "") == stored.ConnectionString?.Replace(".AutoUpdate", "");

        private bool fixExistingConfigurations(IUnitOfWork connection)
        {
            var stored = _storage.ReadConfigurations(connection);
            
            var ordered = stored.OrderBy(x => x.Id).ToArray();

            var legacyConfiguration = ordered.FirstOrDefault(isLegacy);
            if (legacyConfiguration != null)
            {
                if (legacyConfiguration.Name == null)
                    legacyConfiguration.Name = DefaultConfigurationName.Name();
                if (legacyConfiguration.Active == null)
                    legacyConfiguration.Active = true;
                _storage.WriteConfiguration(legacyConfiguration, connection);
                return true;
            }

            var markedConfiguration = ordered.FirstOrDefault(isMarked);
            if (markedConfiguration != null)
            {
                markedConfiguration.Name = DefaultConfigurationName.Name();
                _storage.WriteConfiguration(markedConfiguration, connection);
                return true;
            }

            return false;
        }
        
        private bool runConfigurationUpdates(IEnumerable<UpdateConfiguration> received, IUnitOfWork connection)
        {
            var stored = _storage.ReadConfigurations(connection);
            
            received.ForEach(update =>
            {
                var configuration = stored.FirstOrDefault(c => c.Name == update.Name) ??
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
                _storage.WriteConfiguration(configuration,connection);
            });

            return true;
        }
        
        private static bool updateConfigurationsEnabled(ConfigurationOptions options)
        {
            if (options.AutoUpdatedHangfireConnectionString != null)
                return true;
            if (options.UpdateConfigurations?.Any() ?? false)
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
            catch (ArgumentException)
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