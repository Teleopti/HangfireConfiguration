using System;
using System.Linq;

namespace Hangfire.Configuration
{
    public class DefaultServerConfigurator
    {
        private readonly IConfigurationRepository _repository;
        private readonly IDistributedLock _distributedLock;

        public DefaultServerConfigurator(IConfigurationRepository repository, IDistributedLock distributedLock)
        {
            _repository = repository;
            _distributedLock = distributedLock;
        }

        public void Configure(ConfigurationOptions options)
        {
            if (options?.DefaultHangfireConnectionString == null)
                return;
            
            using (_distributedLock.Take(TimeSpan.FromSeconds(10)))
            {
                var configurations = _repository.ReadConfigurations().ToArray();
                if (configurations.IsEmpty())
                {
                    _repository.WriteConfiguration(new StoredConfiguration
                    {
                        ConnectionString = options.DefaultHangfireConnectionString,
                        SchemaName = options.DefaultSchemaName,
                        Active = true
                    });
                }
                else
                {
                    var legacyConfiguration = configurations.First();
                    legacyConfiguration.ConnectionString = options.DefaultHangfireConnectionString;
                    legacyConfiguration.SchemaName = options.DefaultSchemaName;
                    if (configurations.Where(x => (x.Active ?? false)).IsEmpty())
                        legacyConfiguration.Active = true;

                    _repository.WriteConfiguration(legacyConfiguration);
                }    
            }
        }
    }
}