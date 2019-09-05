using System;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ConfigurationAutoUpdater
    {
        private readonly IConfigurationRepository _repository;
        private readonly IDistributedLock _distributedLock;

        public ConfigurationAutoUpdater(IConfigurationRepository repository, IDistributedLock distributedLock)
        {
            _repository = repository;
            _distributedLock = distributedLock;
        }

        public void Update(ConfigurationOptions options)
        {
            if (options?.AutoUpdatedHangfireConnectionString == null)
                return;
            
            using (_distributedLock.Take(TimeSpan.FromSeconds(10)))
            {
                var configurations = _repository.ReadConfigurations().ToArray();
                if (configurations.IsEmpty())
                {
                    _repository.WriteConfiguration(new StoredConfiguration
                    {
                        ConnectionString = options.AutoUpdatedHangfireConnectionString,
                        SchemaName = options.AutoUpdatedHangfireSchemaName,
                        Active = true
                    });
                }
                else
                {
                    var legacyConfiguration = configurations.First();
                    legacyConfiguration.ConnectionString = options.AutoUpdatedHangfireConnectionString;
                    legacyConfiguration.SchemaName = options.AutoUpdatedHangfireSchemaName;
                    if (configurations.Where(x => (x.Active ?? false)).IsEmpty())
                        legacyConfiguration.Active = true;

                    _repository.WriteConfiguration(legacyConfiguration);
                }    
            }
        }
    }
}