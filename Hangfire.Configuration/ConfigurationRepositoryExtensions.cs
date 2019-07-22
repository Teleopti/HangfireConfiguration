using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public static class ConfigurationRepositoryExtensions
    {
        public static void WriteGoalWorkerCount(this IConfigurationRepository repository, int? workers)
        {
            var configurations = repository.ReadConfigurations();
            var configuration = new StoredConfiguration();
            if (configurations.Any())
            {
                configuration = configurations.FirstOrDefault(x => x.Active.GetValueOrDefault());
                if (configuration == null)
                    configuration = configurations.First();
            }
            configuration.GoalWorkerCount = workers;
            repository.WriteConfiguration(configuration);
        }

        public static int? ReadGoalWorkerCount(this IConfigurationRepository repository) => 
            repository.ReadConfigurations().Any() ? repository.ReadConfigurations().First().GoalWorkerCount : null;
        
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