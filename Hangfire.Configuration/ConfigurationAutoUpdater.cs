using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ConfigurationAutoUpdater
    {
        private readonly IConfigurationRepository _repository;
        private readonly IDistributedLock _distributedLock;
        private readonly State _state;
        
        internal ConfigurationAutoUpdater(IConfigurationRepository repository, IDistributedLock distributedLock, State state)
        {
            _repository = repository;
            _distributedLock = distributedLock;
            _state = state;
        }

        public bool Update(ConfigurationOptions options, IEnumerable<StoredConfiguration> configs)
        {
            if (!shouldUpdate(options, configs)) 
                return false;
            
            _state.ConfigurationAutoUpdaterRan = true;
            
            using (_distributedLock.Take(TimeSpan.FromSeconds(10)))
            {
                var configurations = _repository.ReadConfigurations();
                
                StoredConfiguration configuration;
                
                var legacyConfiguration = configurations.SingleOrDefault(isLegacy);
                if (legacyConfiguration != null)
                {
                    configuration = legacyConfiguration;
                    configuration.Active = true;
                }
                else if (configurations.IsEmpty())
                {
                    configuration = new StoredConfiguration {Active = true};
                }
                else
                {
                    configuration = configurations.FirstOrDefault(isMarked);
                    if (configuration == null)
                        return false;
                }
                
                configuration.ConnectionString = markConnectionString(options.AutoUpdatedHangfireConnectionString);
                configuration.SchemaName = options.AutoUpdatedHangfireSchemaName;

                _repository.WriteConfiguration(configuration);
            }

            return true;
        }

        private bool shouldUpdate(ConfigurationOptions options, IEnumerable<StoredConfiguration> configurations)
        {
            if (options?.AutoUpdatedHangfireConnectionString == null)
                return false;

            if (configurations.IsEmpty())
                return true;            
            
            if (_state.ConfigurationAutoUpdaterRan)
                return false;
            
            return true;
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