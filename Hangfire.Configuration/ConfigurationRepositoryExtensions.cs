using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public static class ConfigurationRepositoryExtensions
    {
        public static IEnumerable<StoredConfiguration> ReadConfiguration(this IConfigurationRepository repository) => 
            repository.ReadConfigurations();

        public static void WriteNewStorageConfiguration(this IConfigurationRepository repository, string connectionString, string schemaName, bool active)
        {
            repository.WriteConfiguration(new StoredConfiguration()
            {
                ConnectionString = connectionString,
                SchemaName = schemaName,
                Active = active
            });
        }

        public static void ActivateStorage(this IConfigurationRepository repository, int id)
        {
            var configurations = repository.ReadConfigurations();
            foreach (var configuration in configurations)
            {
                configuration.Active = configuration.Id == id;
                repository.WriteConfiguration(configuration);
            }
        }
    }
}