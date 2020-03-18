using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ConfigurationUpdater
    {
        private readonly IConfigurationRepository _repository;
        private readonly IDistributedLock _distributedLock;
        private readonly State _state;

        internal ConfigurationUpdater(IConfigurationRepository repository, IDistributedLock distributedLock, State state)
        {
            _repository = repository;
            _distributedLock = distributedLock;
            _state = state;
        }

        public bool Update(ConfigurationOptions options, IEnumerable<StoredConfiguration> configs)
        {
            if (!shouldUpdate(options, configs))
                return false;

            _state.ConfigurationUpdaterRan = true;

            using (_distributedLock.Take("ConfigurationUpdater"))
            {
                var configurations = _repository.ReadConfigurations();

                var legacyConfiguration = configurations.SingleOrDefault(isLegacy);
                if (legacyConfiguration != null)
                {
                    legacyConfiguration.Name = DefaultConfigurationName.Name();
                    legacyConfiguration.Active = true;
                }

                var markedConfiguration = configurations.FirstOrDefault(isMarked);
                if (markedConfiguration != null)
                    markedConfiguration.Name = DefaultConfigurationName.Name();

                var autoUpdate = new UpdateConfiguration
                {
                    Name = DefaultConfigurationName.Name(),
                    ConnectionString = options.AutoUpdatedHangfireConnectionString,
                    SchemaName = options.AutoUpdatedHangfireSchemaName,
                };
                var updateConfigurations = new[] {autoUpdate}
                    .Concat(options.UpdateConfigurations ?? Enumerable.Empty<UpdateConfiguration>())
                    .Where(x => x.ConnectionString != null)
                    .ToArray();

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
                    _repository.WriteConfiguration(configuration);
                });
            }

            return true;
        }

        private bool shouldUpdate(ConfigurationOptions options, IEnumerable<StoredConfiguration> configurations)
        {
            if (!updateEnabled(options))
                return false;
            if (configurations.IsEmpty())
                return true;
            if (_state.ConfigurationUpdaterRan)
                return false;
            return true;
        }

        private static bool updateEnabled(ConfigurationOptions options)
        {
            if (options?.AutoUpdatedHangfireConnectionString != null)
                return true;
            if (options?.UpdateConfigurations?.Any() ?? false)
                return true;
            return false;
        }

        private static bool isLegacy(StoredConfiguration configuration) =>
            configuration.ConnectionString == null;

        private static bool isMarked(StoredConfiguration configuration) =>
            new SqlConnectionStringBuilder(configuration.ConnectionString)
                .ApplicationName
                .EndsWith(".AutoUpdate");

        private static string markConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (applicationNameIsNotSet(builder))
                builder.ApplicationName = "Hangfire";
            builder.ApplicationName += ".AutoUpdate";
            return builder.ToString();
        }

        // because builder will return a app name even though the connection string does not have one
        private static bool applicationNameIsNotSet(SqlConnectionStringBuilder builder)
        {
            return string.IsNullOrEmpty(builder.ApplicationName) ||
                   builder.ApplicationName == ".Net SqlClient Data Provider" ||
                   builder.ApplicationName == "Core .Net SqlClient Data Provider";
        }
    }
}